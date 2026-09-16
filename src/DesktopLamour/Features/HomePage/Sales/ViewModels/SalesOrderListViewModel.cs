// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using DesktopLamour.Shared.Utilities;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderListViewModel : ViewModelBase
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(400);

    private readonly INavigationService          _navigationService;
    private readonly IGetSalesOrdersUseCase      _getOrders;
    private readonly IDeleteSalesOrderUseCase    _deleteOrder;
    private readonly IUnconfirmSalesOrderUseCase _unconfirmOrder;
    private readonly IConfirmSalesOrderUseCase   _confirmOrder;
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
    [ObservableProperty] private decimal             _totalTaxSum;
    [ObservableProperty] private decimal             _totalPaymentSum;

    // 1 ô tìm kiếm chung (AND với FilterFromDate/FilterToDate ở trên) — khớp OR trên các trường
    // text chính, không phân biệt hoa/thường. Lọc chạy dưới SQL (server-side) — xem LoadSalesOrdersAsync.
    [ObservableProperty] private string _searchText = string.Empty;

    // "Kỳ" — chọn nhanh khoảng ngày (theo mẫu MISA), mirror SalesReturnListViewModel (2026-09-13,
    // trước đây thiếu hẳn ở màn này). Đổi Period ghi đè FilterFromDate/FilterToDate, 2 property đó
    // vốn đã tự reload danh sách (xem OnFilterFromDateChanged/OnFilterToDateChanged bên dưới).
    public static string[] PeriodOptions { get; } =
        { "Tùy chọn", "Hôm nay", "Hôm qua", "Tuần này", "Tháng này", "Tháng trước", "Quý này", "Năm nay", "Đầu tháng đến hiện tại" };

    // 2026-09-16: đổi mặc định "Đầu tháng đến hiện tại" (2026-08-31) → "Hôm nay" theo yêu cầu — mỗi
    // ngày mở màn này chỉ thấy đúng chứng từ hôm nay, muốn xem ngày trước tự đổi Kỳ/Từ-Đến ngày.
    // Đồng bộ lại với 6 màn khác cùng đổi (xem SalesReturnListViewModel/AccountingViewModel/
    // WarehouseTransactionListViewModel/BulkCustomerReceiptSearchViewModel/
    // DepositDeductionReportViewModel/SalesOrderReportFilterViewModel).
    [ObservableProperty] private string _selectedPeriod = "Hôm nay";

    // Lọc Trạng thái + filter theo từng cột áp lên dữ liệu ĐÃ tải (client-side, qua SalesOrdersView)
    // — không gọi lại BE, khác với FilterFromDate/ToDate/SearchText ở trên vốn đã lọc server-side
    // sẵn. Mirror SalesReturnListViewModel (2026-09-13, trước đây thiếu hẳn ở màn này).
    public static string[] StatusOptions { get; } = { "Tất cả", "Đã ghi sổ", "Treo" };

    [ObservableProperty] private string _filterStatus = "Tất cả";

    // ── Per-column filter row, embedded trực tiếp trong header DataGrid (không popup) — cùng
    // pattern SalesReturnListViewModel, tái dùng ColumnFilterModels.
    [ObservableProperty] private string _filterDocumentNumber = string.Empty;
    [ObservableProperty] private string _filterCustomerName   = string.Empty;
    [ObservableProperty] private string _filterEmployeeName   = string.Empty;
    [ObservableProperty] private string _filterDescription    = string.Empty;

    partial void OnFilterStatusChanged(string value)         => SalesOrdersView.Refresh();
    partial void OnFilterDocumentNumberChanged(string value) => SalesOrdersView.Refresh();
    partial void OnFilterCustomerNameChanged(string value)   => SalesOrdersView.Refresh();
    partial void OnFilterEmployeeNameChanged(string value)   => SalesOrdersView.Refresh();
    partial void OnFilterDescriptionChanged(string value)    => SalesOrdersView.Refresh();

    // 2026-09-16: thêm AccountingDateFilter — cột "Ngày hạch toán" (thêm 2026-09-14, mirror
    // SalesReturnListViewModel) trước đó chỉ hiển thị, chưa có ô lọc riêng.
    public DateColumnFilter    AccountingDateFilter { get; } = new();
    public DateColumnFilter    DocumentDateFilter { get; } = new();
    public NumericColumnFilter TotalGrossFilter    { get; } = new();
    public NumericColumnFilter TotalDiscountFilter { get; } = new();
    public NumericColumnFilter TotalPaymentFilter  { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = SalesOrdersView.Refresh;
        DocumentDateFilter.Changed  = SalesOrdersView.Refresh;
        TotalGrossFilter.Changed    = SalesOrdersView.Refresh;
        TotalDiscountFilter.Changed = SalesOrdersView.Refresh;
        TotalPaymentFilter.Changed  = SalesOrdersView.Refresh;
    }

    public ObservableCollection<SalesOrderListItem> SalesOrders { get; } = new();

    public ICollectionView SalesOrdersView { get; }

    // 2026-09-14: đổi private → public — nút toolbar "Gửi email, Zalo" (mở dropdown thay vì gán
    // thẳng Command) cần bind IsEnabled="{Binding HasSelection}" từ XAML để TỰ nó xám lại khi chưa
    // chọn dòng nào, khớp đúng hành vi "Sửa"/"Xóa" (Command gán thẳng, WPF tự gray theo CanExecute).
    // Trước đây không bind gì — nút luôn sáng, bấm ra dropdown nhưng 2 mục Email/Zalo bên trong lại
    // xám (do CanExecute=HasSelection), nhìn như nút "không work".
    public bool HasSelection => SelectedOrder is not null;
    // "Bỏ ghi" chỉ khả dụng khi dòng chọn đang thật sự Normal (Status == 0 = "Ghi sổ") — khớp
    // đúng CanExecute=IsConfirmed của UnconfirmCommand trong popup (SalesOrderViewModel).
    private bool CanUnconfirmSelected => SelectedOrder is { Status: 0 };
    // "Ghi sổ" thẳng từ danh sách — mirror CanUnconfirmSelected, khả dụng khi dòng chọn CHƯA Normal
    // (Held=1 hoặc Draft=2 — gộp thành "Treo" cùng ngày, xem SalesOrderListItem.StatusLabel).
    private bool CanConfirmSelected => SelectedOrder is { Status: 1 or 2 };

    public SalesOrderListViewModel(
        INavigationService         navigationService,
        IGetSalesOrdersUseCase     getOrders,
        IDeleteSalesOrderUseCase   deleteOrder,
        IUnconfirmSalesOrderUseCase unconfirmOrder,
        IConfirmSalesOrderUseCase   confirmOrder,
        IDuplicateSalesOrderUseCase duplicateOrder,
        Func<SalesOrderWindow>     formWindowFactory)
    {
        _navigationService = navigationService;
        _getOrders         = getOrders;
        _deleteOrder       = deleteOrder;
        _unconfirmOrder    = unconfirmOrder;
        _confirmOrder      = confirmOrder;
        _duplicateOrder    = duplicateOrder;
        _formWindowFactory = formWindowFactory;

        // 2026-09-16: đổi lại mặc định "Hôm nay" (đảo ngược quyết định 2026-08-31 "Đầu tháng đến
        // hiện tại") — người dùng vẫn có thể đổi Kỳ/Từ ngày/Đến ngày để xem khoảng khác.
        _filterFromDate = DateTime.Today;
        _filterToDate   = DateTime.Today;

        SalesOrdersView = CollectionViewSource.GetDefaultView(SalesOrders);
        SalesOrdersView.Filter = FilterItem;
        WireColumnFilters();
    }

    partial void OnSelectedOrderChanged(SalesOrderListItem? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        EditSalesOrderCommand.NotifyCanExecuteChanged();
        DeleteSalesOrderCommand.NotifyCanExecuteChanged();
        UnconfirmSalesOrderCommand.NotifyCanExecuteChanged();
        ConfirmSalesOrderCommand.NotifyCanExecuteChanged();
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

    partial void OnSelectedPeriodChanged(string value)
    {
        var today = DateTime.Today;
        switch (value)
        {
            case "Hôm nay":
                FilterFromDate = today;
                FilterToDate   = today;
                break;
            case "Hôm qua":
                FilterFromDate = today.AddDays(-1);
                FilterToDate   = today.AddDays(-1);
                break;
            case "Tuần này":
                FilterFromDate = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                FilterToDate   = today;
                break;
            case "Tháng này":
                FilterFromDate = new DateTime(today.Year, today.Month, 1);
                FilterToDate   = FilterFromDate.Value.AddMonths(1).AddDays(-1);
                break;
            case "Tháng trước":
                FilterFromDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                FilterToDate   = FilterFromDate.Value.AddMonths(1).AddDays(-1);
                break;
            case "Quý này":
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                FilterFromDate = new DateTime(today.Year, quarterStartMonth, 1);
                FilterToDate   = FilterFromDate.Value.AddMonths(3).AddDays(-1);
                break;
            case "Năm nay":
                FilterFromDate = new DateTime(today.Year, 1, 1);
                FilterToDate   = new DateTime(today.Year, 12, 31);
                break;
            case "Đầu tháng đến hiện tại":
                FilterFromDate = new DateTime(today.Year, today.Month, 1);
                FilterToDate   = today;
                break;
            case "Tùy chọn":
            default:
                break;
        }
    }

    private bool FilterItem(object obj)
    {
        if (obj is not SalesOrderListItem item) return false;

        if (FilterStatus == "Đã ghi sổ" && !item.IsConfirmed) return false;
        if (FilterStatus == "Treo"      && !item.IsHeld)      return false;

        return Matches(FilterDocumentNumber, item.DocumentNumber)
            && Matches(FilterCustomerName, item.CustomerName)
            && Matches(FilterEmployeeName, item.EmployeeName ?? "")
            && Matches(FilterDescription, item.Description ?? "")
            && AccountingDateFilter.Matches(item.AccountingDate)
            && DocumentDateFilter.Matches(item.DocumentDate)
            && TotalGrossFilter.Matches(item.TotalGross)
            && TotalDiscountFilter.Matches(item.TotalDiscount)
            && TotalPaymentFilter.Matches(item.TotalPayment);
    }

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

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
            TotalTaxSum      = SalesOrders.Sum(o => o.TotalTax);
            TotalPaymentSum  = SalesOrders.Sum(o => o.TotalPayment);

            SalesOrdersView.Refresh();
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

    // 2026-09-13: bỏ hộp thoại "Xác nhận chỉnh sửa" (Yes/No) — mirror SalesReturnListViewModel.
    // EditSalesReturnAsync, vốn mở thẳng popup không hỏi gì. "Xóa" (DeleteSalesOrderAsync) vẫn
    // giữ nguyên xác nhận riêng.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        var window = _formWindowFactory();
        window.Initialize(SelectedOrder.Original);
        // Cho phép Trước/Sau/Thêm duyệt ngay trong popup theo đúng danh sách đang hiển thị
        // (đã lọc theo Từ ngày/Đến ngày/tìm kiếm) — không cần đóng mở lại từng chứng từ.
        var siblings = SalesOrders.Select(o => o.Original).ToList();
        window.SetSiblingContext(siblings, siblings.IndexOf(SelectedOrder.Original));
        if (window.ShowDialog() == true)
            await LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    // "Bỏ ghi" thẳng từ danh sách — dùng chung IUnconfirmSalesOrderUseCase với popup
    // (SalesOrderViewModel.UnconfirmAsync), không mở popup trước. Mirror SalesReturnListViewModel
    // cùng ngày.
    // 2026-09-11: bỏ hẳn dialog xác nhận Yes/No — bấm là vào thẳng, không hỏi lại (mirror
    // SalesReturnListViewModel.UnconfirmSalesReturnAsync cùng ngày, theo yêu cầu đồng bộ sang
    // SalesOrder). Popup chi tiết (SalesOrderViewModel.ToggleConfirmAsync) KHÔNG đổi.
    [RelayCommand(CanExecute = nameof(CanUnconfirmSelected))]
    private async Task UnconfirmSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        try
        {
            await _unconfirmOrder.ExecuteAsync(SelectedOrder.Id, ct);
            await LoadSalesOrdersAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Bỏ ghi thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // "Ghi sổ" thẳng từ danh sách — mirror UnconfirmSalesOrderAsync ở trên (dùng chung
    // IConfirmSalesOrderUseCase với popup, không mở popup trước, không hỏi xác nhận). Thêm
    // 2026-09-11 theo yêu cầu — trước đó danh sách chỉ có "Bỏ ghi", không có action đối xứng.
    [RelayCommand(CanExecute = nameof(CanConfirmSelected))]
    private async Task ConfirmSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        try
        {
            await _confirmOrder.ExecuteAsync(SelectedOrder.Id, ct);
            await LoadSalesOrdersAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ghi sổ thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
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

    // Xuất đúng những dòng đang hiển thị trên lưới (đã áp mọi filter), không phải toàn bộ SalesOrders
    // — mirror SalesReturnListViewModel.ExportExcel (2026-09-13, trước đây thiếu hẳn ở màn này).
    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"ChungTuBanHang_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Bán hàng");

            string[] headers =
            {
                "Số chứng từ", "Ngày", "Diễn giải", "Khách hàng", "Nhân viên",
                "Tổng tiền hàng", "Tổng CK", "Tiền thuế GTGT", "Tổng thanh toán", "Ghi chú", "Trạng thái",
            };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var item in SalesOrdersView.Cast<SalesOrderListItem>())
            {
                worksheet.Cell(row, 1).Value  = item.DocumentNumber;
                worksheet.Cell(row, 2).Value  = item.DocumentDate;
                worksheet.Cell(row, 3).Value  = item.Description;
                worksheet.Cell(row, 4).Value  = item.CustomerName;
                worksheet.Cell(row, 5).Value  = item.EmployeeName;
                worksheet.Cell(row, 6).Value  = item.TotalGross;
                worksheet.Cell(row, 7).Value  = item.TotalDiscount;
                worksheet.Cell(row, 8).Value  = item.TotalTax;
                worksheet.Cell(row, 9).Value  = item.TotalPayment;
                worksheet.Cell(row, 10).Value = item.Notes;
                worksheet.Cell(row, 11).Value = item.StatusLabel;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
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
