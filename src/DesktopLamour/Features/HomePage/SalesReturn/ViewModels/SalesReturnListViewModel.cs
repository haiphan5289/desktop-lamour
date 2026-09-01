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
using DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;
using DesktopLamour.Features.HomePage.SalesReturn.Views;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Shared.Models;
using DesktopLamour.Shared.Utilities;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

public partial class SalesReturnListViewModel : ViewModelBase
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(400);

    // "Hàng bán bị trả lại" liên kết Phiếu Nhập Kho qua Reference=DocumentNumber + ReceiptType=2
    // (ReturnedGoods) — không có FK thật, khớp đúng cách CreateSalesReturnWarehouseReceiptUseCase
    // (BE) và SalesReturnViewModel.EnsureWarehouseReceiptPrintedAsync (WPF popup) đã dedup.
    private const int ReturnedGoodsReceiptType = 2;

    private readonly INavigationService              _navigationService;
    private readonly IGetSalesReturnsUseCase         _getReturns;
    private readonly IDeleteSalesReturnUseCase       _deleteReturn;
    private readonly IConfirmSalesReturnUseCase      _confirmReturn;
    private readonly IUnconfirmSalesReturnUseCase    _unconfirmReturn;
    private readonly IGetWarehouseReceiptsUseCase    _getWarehouseReceipts;
    private readonly Func<SalesReturnWindow>         _formWindowFactory;
    private readonly DebounceDispatcher              _searchDebounce = new();

    [ObservableProperty] private bool                  _isLoading;
    [ObservableProperty] private bool                  _hasError;
    [ObservableProperty] private string                _errorMessage    = string.Empty;
    [ObservableProperty] private bool                  _hasSalesReturns;
    [ObservableProperty] private SalesReturnListItem?  _selectedReturn;
    // Mặc định "Đầu tháng đến hiện tại" (áp dụng đồng bộ toàn app — 2026-08-31), khớp SelectedPeriod
    // bên dưới. Field initializer chạy trước constructor nên không kích hoạt OnSelectedPeriodChanged
    // — phải tự set FromDate/ToDate ở đây cho khớp giá trị SelectedPeriod hiển thị trên UI.
    [ObservableProperty] private DateTime? _filterFromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime? _filterToDate   = DateTime.Today;

    // 1 ô tìm kiếm chung (AND với FilterFromDate/FilterToDate ở trên) — khớp OR trên các trường
    // text chính, không phân biệt hoa/thường. Lọc chạy dưới SQL (server-side) — xem LoadSalesReturnsAsync.
    [ObservableProperty] private string _searchText = string.Empty;

    // "Kỳ" — chọn nhanh khoảng ngày (theo mẫu MISA), giống hệt AccountingViewModel.PeriodOptions.
    // Đổi Period ghi đè FilterFromDate/FilterToDate, 2 property đó vốn đã tự reload danh sách
    // (xem OnFilterFromDateChanged/OnFilterToDateChanged bên dưới) nên không cần gọi lại thủ công.
    public static string[] PeriodOptions { get; } =
        { "Tùy chọn", "Hôm nay", "Hôm qua", "Tuần này", "Tháng này", "Tháng trước", "Quý này", "Năm nay", "Đầu tháng đến hiện tại" };

    [ObservableProperty] private string _selectedPeriod = "Đầu tháng đến hiện tại";

    // Lọc Trạng thái/Kiêm phiếu nhập + filter theo từng cột áp lên dữ liệu ĐÃ tải (client-side, qua
    // SalesReturnsView) — không gọi lại BE, khác với FilterFromDate/ToDate/SearchText ở trên vốn đã
    // lọc server-side sẵn (xem ISalesReturnService.GetAllAsync). Cùng pattern AccountingViewModel.
    public static string[] StatusOptions      { get; } = { "Tất cả", "Nháp", "Đã ghi sổ" };
    public static string[] HasReceiptOptions  { get; } = { "Tất cả", "Có", "Chưa" };

    [ObservableProperty] private string _filterStatus     = "Tất cả";
    [ObservableProperty] private string _filterHasReceipt = "Tất cả";

    // ── Per-column filter row, embedded trực tiếp trong header DataGrid (không popup) — cùng
    // pattern AccountingViewModel/SalesOrderReportDetailView, tái dùng ColumnFilterModels.
    [ObservableProperty] private string _filterDocumentNumber = string.Empty;
    [ObservableProperty] private string _filterCustomerName   = string.Empty;
    [ObservableProperty] private string _filterEmployeeName   = string.Empty;
    [ObservableProperty] private string _filterDescription    = string.Empty;
    [ObservableProperty] private string _filterReturnType     = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value) => SalesReturnsView.Refresh();
    partial void OnFilterCustomerNameChanged(string value)   => SalesReturnsView.Refresh();
    partial void OnFilterEmployeeNameChanged(string value)   => SalesReturnsView.Refresh();
    partial void OnFilterDescriptionChanged(string value)    => SalesReturnsView.Refresh();
    partial void OnFilterReturnTypeChanged(string value)     => SalesReturnsView.Refresh();
    partial void OnFilterStatusChanged(string value)         => SalesReturnsView.Refresh();
    partial void OnFilterHasReceiptChanged(string value)     => SalesReturnsView.Refresh();

    public DateColumnFilter    AccountingDateFilter { get; } = new();
    public NumericColumnFilter TotalAmountFilter    { get; } = new();
    public NumericColumnFilter TotalDiscountFilter  { get; } = new();
    public NumericColumnFilter TotalPaymentFilter   { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = SalesReturnsView.Refresh;
        TotalAmountFilter.Changed    = SalesReturnsView.Refresh;
        TotalDiscountFilter.Changed  = SalesReturnsView.Refresh;
        TotalPaymentFilter.Changed   = SalesReturnsView.Refresh;
    }

    public ObservableCollection<SalesReturnListItem> SalesReturns { get; } = new();

    public ICollectionView SalesReturnsView { get; }

    private bool HasSelection => SelectedReturn is not null;
    private bool CanEditSelected   => SelectedReturn is { IsDraft: true };
    private bool CanGhiSoSelected  => SelectedReturn is { IsDraft: true };
    private bool CanBoGhiSelected  => SelectedReturn is { IsConfirmed: true };

    public SalesReturnListViewModel(
        INavigationService           navigationService,
        IGetSalesReturnsUseCase      getReturns,
        IDeleteSalesReturnUseCase    deleteReturn,
        IConfirmSalesReturnUseCase   confirmReturn,
        IUnconfirmSalesReturnUseCase unconfirmReturn,
        IGetWarehouseReceiptsUseCase getWarehouseReceipts,
        Func<SalesReturnWindow>      formWindowFactory)
    {
        _navigationService     = navigationService;
        _getReturns            = getReturns;
        _deleteReturn          = deleteReturn;
        _confirmReturn         = confirmReturn;
        _unconfirmReturn       = unconfirmReturn;
        _getWarehouseReceipts  = getWarehouseReceipts;
        _formWindowFactory     = formWindowFactory;

        SalesReturnsView = CollectionViewSource.GetDefaultView(SalesReturns);
        SalesReturnsView.Filter = FilterItem;
        WireColumnFilters();
    }

    partial void OnSelectedReturnChanged(SalesReturnListItem? value)
    {
        ViewSalesReturnCommand.NotifyCanExecuteChanged();
        EditSalesReturnCommand.NotifyCanExecuteChanged();
        DeleteSalesReturnCommand.NotifyCanExecuteChanged();
        GhiSoCommand.NotifyCanExecuteChanged();
        BoGhiCommand.NotifyCanExecuteChanged();
    }

    // Đổi ngày là thao tác rời rạc (không gõ liên tục như SearchText) — reload ngay, không debounce.
    partial void OnFilterFromDateChanged(DateTime? value) => _ = LoadSalesReturnsCommand.ExecuteAsync(null);
    partial void OnFilterToDateChanged(DateTime? value)   => _ = LoadSalesReturnsCommand.ExecuteAsync(null);

    // Lọc giờ chạy dưới SQL (server-side) thay vì trong RAM — gõ liên tục sẽ bắn 1 HTTP request mỗi
    // ký tự nếu không debounce. Chờ người dùng ngừng gõ 400ms rồi mới gọi lại API.
    partial void OnSearchTextChanged(string value)
        => _searchDebounce.Debounce(SearchDebounceDelay, ct => LoadSalesReturnsAsync(ct));

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
        if (obj is not SalesReturnListItem item) return false;

        if (FilterStatus == "Nháp"       && !item.IsDraft)     return false;
        if (FilterStatus == "Đã ghi sổ"  && !item.IsConfirmed) return false;

        if (FilterHasReceipt == "Có"    && !item.HasLinkedWarehouseReceipt) return false;
        if (FilterHasReceipt == "Chưa"  &&  item.HasLinkedWarehouseReceipt) return false;

        return Matches(FilterDocumentNumber, item.DocumentNumber)
            && Matches(FilterCustomerName, item.CustomerName)
            && Matches(FilterEmployeeName, item.EmployeeName ?? "")
            && Matches(FilterDescription, item.Description ?? "")
            && Matches(FilterReturnType, item.ReturnTypeLabel)
            && AccountingDateFilter.Matches(item.AccountingDate)
            && TotalAmountFilter.Matches(item.TotalAmount)
            && TotalDiscountFilter.Matches(item.TotalDiscount)
            && TotalPaymentFilter.Matches(item.TotalPayment);
    }

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    // Lọc đã tự áp dụng ngay khi đổi ngày/tìm kiếm (live filter) — nút "Lọc" chỉ để người dùng có
    // affordance rõ ràng để bấm, giống hàng lọc màn Chứng từ bán hàng.
    [RelayCommand]
    private async Task Filter() => await LoadSalesReturnsAsync();

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadSalesReturnsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getReturns.ExecuteAsync(FilterFromDate, FilterToDate, SearchText, ct);

            SalesReturns.Clear();
            foreach (var dto in items
                         .Select(SalesReturnListItem.FromDto)
                         .OrderByDescending(o => o.DocumentDate))
                SalesReturns.Add(dto);

            HasSalesReturns = SalesReturns.Count > 0;

            // "Kiêm phiếu nhập" — không có trong response, tính lại client-side bằng cách so khớp
            // với danh sách WarehouseReceipt hiện có. Lỗi ở bước này không nên chặn hiển thị danh
            // sách chính (cột này để trống/false nếu không tải được).
            try
            {
                var receipts = await _getWarehouseReceipts.ExecuteAsync(ct);
                var linkedDocNumbers = receipts
                    .Where(r => r.ReceiptType == ReturnedGoodsReceiptType && !string.IsNullOrEmpty(r.Reference))
                    .Select(r => r.Reference!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var item in SalesReturns)
                    item.HasLinkedWarehouseReceipt = linkedDocNumbers.Contains(item.DocumentNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Không thể tải danh sách phiếu nhập kho để tính 'Kiêm phiếu nhập': {ex.Message}");
            }

            SalesReturnsView.Refresh();
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
    private async Task AddSalesReturnAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    // "Xem" — mở đúng popup Sửa nhưng KHÔNG đòi hỏi Nháp (khác Sửa): xem được cả chứng từ đã Ghi
    // sổ. Chứng từ Confirmed tự bất biến ở phía BE (UpdateSalesReturnUseCase chặn 400), nên không
    // cần dựng riêng 1 chế độ chỉ-đọc — mở thẳng cùng popup, tương tự AccountingViewModel.ViewEntry.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task ViewSalesReturnAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        var window = _formWindowFactory();
        window.Initialize(SelectedReturn.Original);
        var siblings = SalesReturns.Select(r => r.Original).ToList();
        window.SetSiblingContext(siblings, siblings.IndexOf(SelectedReturn.Original));
        if (window.ShowDialog() == true)
            await LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task EditSalesReturnAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        var window = _formWindowFactory();
        window.Initialize(SelectedReturn.Original);
        // Cho phép Trước/Sau/Thêm duyệt ngay trong popup theo đúng danh sách đang hiển thị.
        var siblings = SalesReturns.Select(r => r.Original).ToList();
        window.SetSiblingContext(siblings, siblings.IndexOf(SelectedReturn.Original));
        if (window.ShowDialog() == true)
            await LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task DeleteSalesReturnAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{SelectedReturn.DocumentNumber}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _deleteReturn.ExecuteAsync(SelectedReturn.Id, ct);
            SelectedReturn = null;
            await LoadSalesReturnsAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
    }

    // "Ghi sổ" — chuyển Nháp → Đã ghi sổ, cộng tồn kho thật (side-effect nằm ở BE
    // ConfirmSalesReturnUseCase, không phải ở đây). Thao tác trực tiếp trên danh sách, không cần
    // mở popup trước — khớp đúng vị trí nút "Ghi sổ" trên toolbar màn danh sách theo mẫu MISA.
    [RelayCommand(CanExecute = nameof(CanGhiSoSelected))]
    private async Task GhiSoAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        HasError     = false;
        ErrorMessage = string.Empty;
        IsLoading    = true;
        try
        {
            await _confirmReturn.ExecuteAsync(SelectedReturn.Id, ct);
            await LoadSalesReturnsAsync(ct);
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Ghi sổ thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // "Bỏ ghi" — chuyển Đã ghi sổ → Nháp, hoàn tác tồn kho (BE UnconfirmSalesReturnUseCase từ chối
    // nếu tồn kho hiện tại không đủ hoàn tác — đã bán/xuất tiếp sau khi Ghi sổ).
    [RelayCommand(CanExecute = nameof(CanBoGhiSelected))]
    private async Task BoGhiAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        var confirm = MessageBox.Show(
            $"Bỏ ghi sổ chứng từ '{SelectedReturn.DocumentNumber}'? Tồn kho đã cộng lúc Ghi sổ sẽ được hoàn tác.",
            "Xác nhận bỏ ghi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        HasError     = false;
        ErrorMessage = string.Empty;
        IsLoading    = true;
        try
        {
            await _unconfirmReturn.ExecuteAsync(SelectedReturn.Id, ct);
            await LoadSalesReturnsAsync(ct);
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Bỏ ghi thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // Xuất đúng những dòng đang hiển thị trên lưới (đã áp mọi filter), không phải toàn bộ SalesReturns.
    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"ChungTuHangBanBiTraLai_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Hàng bán bị trả lại");

            string[] headers =
            {
                "Loại trả hàng", "Số chứng từ", "Ngày hạch toán", "Ngày chứng từ", "Khách hàng",
                "Nhân viên", "Diễn giải", "Tổng tiền hàng", "Tổng CK", "Tổng thanh toán",
                "Trạng thái", "Kiêm phiếu nhập",
            };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var item in SalesReturnsView.Cast<SalesReturnListItem>())
            {
                worksheet.Cell(row, 1).Value  = item.ReturnTypeLabel;
                worksheet.Cell(row, 2).Value  = item.DocumentNumber;
                worksheet.Cell(row, 3).Value  = item.AccountingDate;
                worksheet.Cell(row, 4).Value  = item.DocumentDate;
                worksheet.Cell(row, 5).Value  = item.CustomerName;
                worksheet.Cell(row, 6).Value  = item.EmployeeName;
                worksheet.Cell(row, 7).Value  = item.Description;
                worksheet.Cell(row, 8).Value  = item.TotalAmount;
                worksheet.Cell(row, 9).Value  = item.TotalDiscount;
                worksheet.Cell(row, 10).Value = item.TotalPayment;
                worksheet.Cell(row, 11).Value = item.StatusLabel;
                worksheet.Cell(row, 12).Value = item.HasLinkedWarehouseReceipt ? "Có" : "Chưa";
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
}
