// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Utilities;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderListViewModel : ViewModelBase
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(400);

    private readonly INavigationService          _navigationService;
    private readonly IGetSalesOrdersUseCase      _getOrders;
    private readonly IDeleteSalesOrderUseCase    _deleteOrder;
    private readonly IHoldSalesOrderUseCase      _holdOrder;
    private readonly IDuplicateSalesOrderUseCase _duplicateOrder;
    private readonly Func<SalesOrderWindow>      _formWindowFactory;
    private readonly DebounceDispatcher          _searchDebounce = new();

    [ObservableProperty] private bool                _isLoading;
    [ObservableProperty] private bool                _hasError;
    [ObservableProperty] private string              _errorMessage   = string.Empty;
    [ObservableProperty] private bool                _hasSalesOrders;
    [ObservableProperty] private SalesOrderListItem? _selectedOrder;
    [ObservableProperty] private DateTime?           _filterFromDate;
    [ObservableProperty] private DateTime?           _filterToDate;

    // Tổng dòng footer — cộng dồn trên danh sách SalesOrders đang hiển thị (đã lọc SẴN từ BE).
    [ObservableProperty] private int                 _totalCount;
    [ObservableProperty] private decimal             _totalGrossSum;
    [ObservableProperty] private decimal             _totalDiscountSum;
    [ObservableProperty] private decimal             _totalPaymentSum;

    // 1 ô tìm kiếm chung (AND với FilterFromDate/FilterToDate ở trên) — khớp OR trên các trường
    // text chính, không phân biệt hoa/thường. Lọc chạy dưới SQL (server-side) — xem LoadSalesOrdersAsync.
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<SalesOrderListItem> SalesOrders { get; } = new();

    private bool HasSelection => SelectedOrder is not null;

    public SalesOrderListViewModel(
        INavigationService         navigationService,
        IGetSalesOrdersUseCase     getOrders,
        IDeleteSalesOrderUseCase   deleteOrder,
        IHoldSalesOrderUseCase     holdOrder,
        IDuplicateSalesOrderUseCase duplicateOrder,
        Func<SalesOrderWindow>     formWindowFactory)
    {
        _navigationService = navigationService;
        _getOrders         = getOrders;
        _deleteOrder       = deleteOrder;
        _holdOrder         = holdOrder;
        _duplicateOrder    = duplicateOrder;
        _formWindowFactory = formWindowFactory;

        // Mặc định mở màn hình chỉ hiện chứng từ của HÔM NAY (không dồn hết lịch sử lại) —
        // giống MISA. Người dùng vẫn có thể đổi Từ ngày/Đến ngày để xem ngày khác.
        _filterFromDate = DateTime.Today;
        _filterToDate   = DateTime.Today;
    }

    partial void OnSelectedOrderChanged(SalesOrderListItem? value)
    {
        EditSalesOrderCommand.NotifyCanExecuteChanged();
        DeleteSalesOrderCommand.NotifyCanExecuteChanged();
        HoldSalesOrderCommand.NotifyCanExecuteChanged();
        DuplicateSalesOrderCommand.NotifyCanExecuteChanged();
        SendEmailCommand.NotifyCanExecuteChanged();
        SendZaloCommand.NotifyCanExecuteChanged();
    }

    // Đổi ngày là thao tác rời rạc (không gõ liên tục như SearchText) — reload ngay, không debounce.
    partial void OnFilterFromDateChanged(DateTime? value) => _ = LoadSalesOrdersCommand.ExecuteAsync(null);
    partial void OnFilterToDateChanged(DateTime? value)   => _ = LoadSalesOrdersCommand.ExecuteAsync(null);

    // Lọc giờ chạy dưới SQL (server-side) thay vì trong RAM — gõ liên tục sẽ bắn 1 HTTP request mỗi
    // ký tự nếu không debounce. Chờ người dùng ngừng gõ 400ms rồi mới gọi lại API.
    partial void OnSearchTextChanged(string value)
        => _searchDebounce.Debounce(SearchDebounceDelay, ct => LoadSalesOrdersAsync(ct));

    // Lọc đã tự áp dụng ngay khi đổi ngày/tìm kiếm (live filter) — nút "Lọc" chỉ để người dùng có
    // affordance rõ ràng để bấm, giống hàng lọc màn Quỹ.
    [RelayCommand]
    private async Task Filter() => await LoadSalesOrdersAsync();

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadSalesOrdersAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getOrders.ExecuteAsync(FilterFromDate, FilterToDate, SearchText, ct);

            SalesOrders.Clear();
            foreach (var dto in items
                         .Select(SalesOrderListItem.FromDto)
                         .OrderByDescending(o => o.DocumentDate))
                SalesOrders.Add(dto);

            HasSalesOrders   = SalesOrders.Count > 0;
            TotalCount       = SalesOrders.Count;
            TotalGrossSum    = SalesOrders.Sum(o => o.TotalGross);
            TotalDiscountSum = SalesOrders.Sum(o => o.TotalDiscount);
            TotalPaymentSum  = SalesOrders.Sum(o => o.TotalPayment);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AddSalesOrderAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn chỉnh sửa chứng từ '{SelectedOrder.DocumentNumber}'?",
            "Xác nhận chỉnh sửa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        var window = _formWindowFactory();
        window.Initialize(SelectedOrder.Original);
        if (window.ShowDialog() == true)
            await LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task HoldSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        try
        {
            await _holdOrder.ExecuteAsync(SelectedOrder.Id, ct);
            await LoadSalesOrdersAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Treo đơn thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{SelectedOrder.DocumentNumber}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _deleteOrder.ExecuteAsync(SelectedOrder.Id, ct);
            SelectedOrder = null;
            await LoadSalesOrdersAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
    }

    // "Nhân bản" — tạo NGAY 1 chứng từ bán hàng mới (số chứng từ tự sinh, ngày = hôm nay, cùng
    // khách hàng/dòng hàng) qua DuplicateSalesOrderUseCase → BE tái dùng CreateSalesOrderUseCase nên
    // tồn kho được trừ lại như 1 giao dịch bán hàng thật, không phải chỉ copy dữ liệu.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DuplicateSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        // Chụp lại trước — LoadSalesOrdersAsync bên dưới Clear() rồi build lại SalesOrders, khiến
        // DataGrid.SelectedItem (two-way bind SelectedOrder) rơi về null ngay khi collection đổi,
        // nên đọc SelectedOrder.DocumentNumber SAU khi reload sẽ NullReferenceException.
        var sourceId             = SelectedOrder.Id;
        var sourceDocumentNumber = SelectedOrder.DocumentNumber;

        try
        {
            var duplicated = await _duplicateOrder.ExecuteAsync(sourceId, ct);
            await LoadSalesOrdersAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem

            MessageBox.Show(
                $"Đã nhân bản chứng từ '{sourceDocumentNumber}' thành '{duplicated.DocumentNumber}'.",
                "Nhân bản thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Nhân bản thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // "Gửi email, Zalo" — mirror ReportSharingHelper pattern đã dùng ở SalesOrderReportDetailViewModel:
    // app chưa tích hợp SMTP/Zalo OA thật, nên chỉ xuất file rồi giao cho Email/Zalo client của máy
    // người dùng tự đính kèm, không tự gửi được.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void SendEmail()
    {
        if (SelectedOrder is null) return;

        try
        {
            using var workbook = BuildSingleOrderWorkbook(SelectedOrder);
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, $"ChungTu_{SelectedOrder.DocumentNumber}");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenMailClient(
                $"Chứng từ bán hàng - {SelectedOrder.DocumentNumber}",
                $"File chứng từ đã được lưu tại:\n{path}\n\nVui lòng đính kèm file này vào email trước khi gửi.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Email thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void SendZalo()
    {
        if (SelectedOrder is null) return;

        try
        {
            using var workbook = BuildSingleOrderWorkbook(SelectedOrder);
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, $"ChungTu_{SelectedOrder.DocumentNumber}");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenZaloApp();

            MessageBox.Show("Đã mở Zalo và thư mục chứa file chứng từ. Vui lòng kéo-thả file để đính kèm.",
                "Gửi Zalo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Zalo thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static XLWorkbook BuildSingleOrderWorkbook(SalesOrderListItem item)
    {
        var order     = item.Original;
        var workbook  = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Chứng từ");

        worksheet.Cell(1, 1).Value = "Số chứng từ:";
        worksheet.Cell(1, 2).Value = order.DocumentNumber;
        worksheet.Cell(2, 1).Value = "Ngày:";
        worksheet.Cell(2, 2).Value = order.DocumentDate.ToLocalTime().ToString("dd/MM/yyyy");
        worksheet.Cell(3, 1).Value = "Khách hàng:";
        worksheet.Cell(3, 2).Value = order.CustomerName;
        worksheet.Cell(4, 1).Value = "Nhân viên:";
        worksheet.Cell(4, 2).Value = order.EmployeeName;

        string[] headers = { "Mã hàng", "Tên hàng", "SL", "Đơn giá", "CK (%)", "Thành tiền", "Thuế suất", "Tổng cộng" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(6, i + 1);
            cell.Value           = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 7;
        foreach (var line in order.Lines)
        {
            worksheet.Cell(row, 1).Value = line.ProductCode;
            worksheet.Cell(row, 2).Value = line.ProductName;
            worksheet.Cell(row, 3).Value = line.Quantity;
            worksheet.Cell(row, 4).Value = line.UnitPrice;
            worksheet.Cell(row, 5).Value = line.DiscountRate;
            worksheet.Cell(row, 6).Value = line.Amount;
            worksheet.Cell(row, 7).Value = line.TaxRate;
            worksheet.Cell(row, 8).Value = line.Amount + line.TaxAmount;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        return workbook;
    }
}
