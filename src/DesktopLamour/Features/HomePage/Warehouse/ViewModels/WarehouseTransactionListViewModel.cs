// Copyright © 2026 DesktopLamour. All rights reserved.
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseTransactionListViewModel : ViewModelBase
{
    private readonly IGetWarehouseTransactionsUseCase _getUseCase;
    private readonly IGetSalesOrderByIdUseCase        _getSalesOrderById;
    private readonly IGetWarehouseReceiptByIdUseCase  _getWarehouseReceiptById;
    private readonly IConfirmWarehouseReceiptUseCase    _confirmReceipt;
    private readonly IUnconfirmWarehouseReceiptUseCase  _unconfirmReceipt;
    private readonly IDeleteWarehouseReceiptUseCase     _deleteReceipt;
    private readonly IConfirmSalesOrderUseCase          _confirmOrder;
    private readonly IUnconfirmSalesOrderUseCase        _unconfirmOrder;
    private readonly IDeleteSalesOrderUseCase           _deleteOrder;
    private readonly Func<WarehouseReceiptPrintWindow>  _receiptPrintWindowFactory;
    private readonly IGetCustomersUseCase             _getCustomers;
    private readonly INavigationService               _navigationService;
    private readonly Func<WarehouseReceiptFormWindow>  _formWindowFactory;
    private readonly Func<SalesOrderWindow>            _salesOrderWindowFactory;
    private readonly Func<SalesOrderPrintWindow>       _printWindowFactory;
    private readonly ILogger<WarehouseTransactionListViewModel> _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasItems;
    // 2026-09-16: đổi lại mặc định "Hôm nay" (đảo ngược quyết định 2026-08-31 "Đầu tháng đến hiện
    // tại", vốn thay cho lùi 1 tháng/rolling 30 ngày trước đó nữa).
    [ObservableProperty] private DateTime? _fromDate = DateTime.Today;
    [ObservableProperty] private DateTime? _toDate   = DateTime.Today;
    [ObservableProperty] private WarehouseTransactionResponseDto? _selectedItem;

    // "Kỳ" — chọn nhanh khoảng ngày (khớp MISA, cùng bộ lựa chọn màn Chứng từ bán hàng). Đổi Kỳ ghi đè
    // FromDate/ToDate; ngược lại tự gõ lại Từ/Đến thì Kỳ chuyển về "Tùy chọn". Cả 2 đều CHỈ đổi bộ
    // lọc — dữ liệu vẫn nạp khi bấm "Lấy dữ liệu" như trước.
    public static string[] PeriodOptions { get; } =
        { "Tùy chọn", "Hôm nay", "Hôm qua", "Tuần này", "Tháng này", "Tháng trước", "Quý này", "Năm nay", "Đầu tháng đến hiện tại" };

    [ObservableProperty] private string _selectedPeriod = "Hôm nay";
    private bool _applyingPeriod;

    partial void OnSelectedPeriodChanged(string value)
    {
        var today = DateTime.Today;
        DateTime? from = null, to = null;
        switch (value)
        {
            case "Hôm nay":      from = today; to = today; break;
            case "Hôm qua":      from = today.AddDays(-1); to = today.AddDays(-1); break;
            case "Tuần này":
                from = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                to   = today;
                break;
            case "Tháng này":
                from = new DateTime(today.Year, today.Month, 1);
                to   = from.Value.AddMonths(1).AddDays(-1);
                break;
            case "Tháng trước":
                from = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                to   = from.Value.AddMonths(1).AddDays(-1);
                break;
            case "Quý này":
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                from = new DateTime(today.Year, quarterStartMonth, 1);
                to   = from.Value.AddMonths(3).AddDays(-1);
                break;
            case "Năm nay":
                from = new DateTime(today.Year, 1, 1);
                to   = new DateTime(today.Year, 12, 31);
                break;
            case "Đầu tháng đến hiện tại":
                from = new DateTime(today.Year, today.Month, 1);
                to   = today;
                break;
            default: return; // "Tùy chọn" — giữ nguyên Từ/Đến người dùng đang đặt
        }

        _applyingPeriod = true;
        FromDate = from;
        ToDate   = to;
        _applyingPeriod = false;
    }

    partial void OnFromDateChanged(DateTime? value) { if (!_applyingPeriod) SelectedPeriod = "Tùy chọn"; }
    partial void OnToDateChanged(DateTime? value)   { if (!_applyingPeriod) SelectedPeriod = "Tùy chọn"; }

    // "Trạng thái" — lọc client-side trên dữ liệu đã tải (không gọi lại BE), theo IsPosted.
    public static string[] StatusOptions { get; } = { "Tất cả", "Đã ghi sổ", "Chưa ghi sổ" };
    [ObservableProperty] private string _filterStatus = "Tất cả";
    partial void OnFilterStatusChanged(string value) => ApplyFilters();

    // Footer lưới — đếm đúng các dòng đang hiển thị sau lọc.
    public string RowCountText    => $"Số dòng = {Items.Count}";
    public string TotalAmountText => MoneyFormat.Format(Items.Sum(i => i.TotalAmount), "N0");

    // Trạng thái các nút ribbon — chỉ phụ thuộc vòng đời của dòng đang chọn (đã/chưa ghi sổ, loại chứng từ).
    private bool HasSelection        => SelectedItem is not null;
    private bool CanPost             => SelectedItem is { IsPosted: false };
    private bool CanUnpost           => SelectedItem is { IsPosted: true };
    private bool CanDeleteSelected   => SelectedItem is { IsPosted: false };

    partial void OnSelectedItemChanged(WarehouseTransactionResponseDto? value)
    {
        ShowDetailSelectedCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
        PostSelectedCommand.NotifyCanExecuteChanged();
        UnpostSelectedCommand.NotifyCanExecuteChanged();
        ViewInvoiceCommand.NotifyCanExecuteChanged();
        SendEmailCommand.NotifyCanExecuteChanged();
        SendZaloCommand.NotifyCanExecuteChanged();
    }

    // 0 = Tất cả, 1 = Nhập kho, 2 = Xuất kho
    [ObservableProperty] private int _selectedTypeIndex;

    // ── Per-column filter row, embedded directly in each header (no popup) ─────
    // Text columns: plain textbox, case-insensitive Contains against the cell's displayed text.
    // Date/numeric columns: an operator combo (=, ≤, ...) + a typed value, shown side by side.
    // Same pattern as SalesOrderReportDetailViewModel — see docs on ColumnFilterModels.
    [ObservableProperty] private string _filterDocumentNumber      = string.Empty;
    [ObservableProperty] private string _filterDescription         = string.Empty;
    [ObservableProperty] private string _filterDeliveryOrReceiver  = string.Empty;
    [ObservableProperty] private string _filterObjectName          = string.Empty;
    [ObservableProperty] private string _filterHasSalesOrder       = string.Empty;
    [ObservableProperty] private string _filterDocumentTypeLabel   = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value)     => ApplyFilters();
    partial void OnFilterDescriptionChanged(string value)        => ApplyFilters();
    partial void OnFilterDeliveryOrReceiverChanged(string value) => ApplyFilters();
    partial void OnFilterObjectNameChanged(string value)         => ApplyFilters();
    partial void OnFilterHasSalesOrderChanged(string value)      => ApplyFilters();
    partial void OnFilterDocumentTypeLabelChanged(string value)  => ApplyFilters();

    public DateColumnFilter    AccountingDateFilter { get; } = new();
    public DateColumnFilter    DocumentDateFilter   { get; } = new();
    public DateColumnFilter    LedgerDateFilter     { get; } = new();
    public NumericColumnFilter TotalAmountFilter    { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = ApplyFilters;
        DocumentDateFilter.Changed   = ApplyFilters;
        LedgerDateFilter.Changed     = ApplyFilters;
        TotalAmountFilter.Changed    = ApplyFilters;
    }

    public ObservableCollection<WarehouseTransactionResponseDto> Items { get; } = new();

    // Full unfiltered dataset from the last LoadAsync — Items is derived from this via ApplyFilters.
    private List<WarehouseTransactionResponseDto> _allItems = new();

    public WarehouseTransactionListViewModel(
        IGetWarehouseTransactionsUseCase getUseCase,
        IGetSalesOrderByIdUseCase        getSalesOrderById,
        IGetWarehouseReceiptByIdUseCase  getWarehouseReceiptById,
        IConfirmWarehouseReceiptUseCase    confirmReceipt,
        IUnconfirmWarehouseReceiptUseCase  unconfirmReceipt,
        IDeleteWarehouseReceiptUseCase     deleteReceipt,
        IConfirmSalesOrderUseCase          confirmOrder,
        IUnconfirmSalesOrderUseCase        unconfirmOrder,
        IDeleteSalesOrderUseCase           deleteOrder,
        IGetCustomersUseCase             getCustomers,
        INavigationService               navigationService,
        Func<WarehouseReceiptFormWindow>  formWindowFactory,
        Func<SalesOrderWindow>            salesOrderWindowFactory,
        Func<SalesOrderPrintWindow>       printWindowFactory,
        Func<WarehouseReceiptPrintWindow> receiptPrintWindowFactory,
        ILogger<WarehouseTransactionListViewModel> logger)
    {
        _getUseCase              = getUseCase;
        _getSalesOrderById       = getSalesOrderById;
        _getWarehouseReceiptById = getWarehouseReceiptById;
        _confirmReceipt          = confirmReceipt;
        _unconfirmReceipt        = unconfirmReceipt;
        _deleteReceipt           = deleteReceipt;
        _confirmOrder            = confirmOrder;
        _unconfirmOrder          = unconfirmOrder;
        _deleteOrder             = deleteOrder;
        _receiptPrintWindowFactory = receiptPrintWindowFactory;
        _getCustomers            = getCustomers;
        _navigationService       = navigationService;
        _formWindowFactory       = formWindowFactory;
        _salesOrderWindowFactory = salesOrderWindowFactory;
        _printWindowFactory      = printWindowFactory;
        _logger                  = logger;

        WireColumnFilters();
        // Items.Clear()/Add trong ApplyFilters bắn CollectionChanged — dùng chung 1 chỗ để cập nhật footer.
        Items.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(RowCountText));
            OnPropertyChanged(nameof(TotalAmountText));
        };
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void NavigateToTongHopTonKho()
        => _navigationService.NavigateTo(NavigationRoutes.Warehouse.TongHopTonKho);

    // "Phiếu Nhập" — mở thẳng form tạo phiếu nhập kho mới.
    [RelayCommand]
    private void OpenForm()
    {
        var window = _formWindowFactory();
        window.Owner = Application.Current.MainWindow;
        var result = window.ShowDialog();
        if (result == true)
            LoadCommand.Execute(null);
    }

    // "Phiếu Xuất" — hệ thống không có luồng tạo "phiếu xuất kho" riêng, xuất kho chỉ sinh ra
    // từ 1 Chứng từ bán hàng đã ghi sổ, nên mở thẳng form tạo Sales Order mới.
    [RelayCommand]
    private void OpenSalesOrder()
    {
        var window = _salesOrderWindowFactory();
        window.Initialize(null, isFromWarehouseExport: true);
        window.Owner = Application.Current.MainWindow;
        var result = window.ShowDialog();
        if (result == true)
            LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
        => await LoadCoreAsync(null, ct);

    // keepSelection: sau Ghi sổ/Bỏ ghi/Xóa tải lại danh sách nhưng giữ dòng vừa thao tác được chọn (nếu còn).
    private async Task LoadCoreAsync(WarehouseTransactionKey? keepSelection, CancellationToken ct)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var type = SelectedTypeIndex switch
            {
                1 => "import",
                2 => "export",
                _ => null,
            };

            var transactions = await _getUseCase.ExecuteAsync(FromDate, ToDate, type, ct);
            _allItems = transactions.OrderByDescending(t => t.DocumentDate).ToList();
            ApplyFilters();
            SelectedItem = (keepSelection is { } k
                ? Items.FirstOrDefault(i => i.Id == k.Id && i.TransactionType == k.Type)
                : null) ?? Items.FirstOrDefault();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load warehouse transactions");
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // Re-derives Items from _allItems using the active column filters.
    private void ApplyFilters()
    {
        Items.Clear();
        foreach (var item in _allItems.Where(MatchesAllFilters))
            Items.Add(item);

        HasItems = Items.Count > 0;
    }

    private bool MatchesAllFilters(WarehouseTransactionResponseDto item)
        => AccountingDateFilter.Matches(item.AccountingDate)
        && DocumentDateFilter.Matches(item.DocumentDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterDescription, item.Description ?? string.Empty)
        && TotalAmountFilter.Matches(item.TotalAmount)
        && Matches(FilterDeliveryOrReceiver, item.DeliveryOrReceiver ?? string.Empty)
        && Matches(FilterObjectName, item.ObjectName ?? string.Empty)
        && Matches(FilterHasSalesOrder, item.HasSalesOrder ? "✓ Đã lập" : string.Empty)
        && LedgerDateFilter.Matches(item.LedgerDate)
        && Matches(FilterDocumentTypeLabel, item.DocumentTypeLabel)
        && MatchesStatus(item);

    private bool MatchesStatus(WarehouseTransactionResponseDto item) => FilterStatus switch
    {
        "Đã ghi sổ"   => item.IsPosted,
        "Chưa ghi sổ" => !item.IsPosted,
        _             => true,
    };

    private readonly record struct WarehouseTransactionKey(int Id, string Type);

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    partial void OnSelectedTypeIndexChanged(int value) => LoadCommand.Execute(null);

    // ── Ribbon: Xem / Xóa / Ghi sổ / Bỏ ghi / Xem hóa đơn / Xuất khẩu ─────────────────────────────
    // Dòng Nhập kho (Id = WarehouseReceipt.Id) và Xuất kho (Id = SalesOrder.Id) dùng use case riêng
    // của từng loại chứng từ — cùng các use case mà popup của chúng đang gọi, không có logic mới ở đây.
    private bool IsExport(WarehouseTransactionResponseDto item)
        => item.TransactionType.Equals("Export", StringComparison.OrdinalIgnoreCase);

    // "Xem" — cùng hành vi double-click dòng.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private Task ShowDetailSelectedAsync(CancellationToken ct = default)
        => SelectedItem is null ? Task.CompletedTask : ShowDetailAsync(SelectedItem, ct);

    [RelayCommand(CanExecute = nameof(CanPost))]
    private Task PostSelectedAsync(CancellationToken ct = default)
        => RunOnSelectedAsync("Ghi sổ", async (item, token) =>
        {
            if (IsExport(item)) await _confirmOrder.ExecuteAsync(item.Id, token);
            else                await _confirmReceipt.ExecuteAsync(item.Id, token);
        }, ct);

    // "Bỏ ghi" — không hỏi xác nhận, mirror nút Bỏ ghi trên màn Chứng từ bán hàng (chỉ "Xóa" mới hỏi).
    [RelayCommand(CanExecute = nameof(CanUnpost))]
    private Task UnpostSelectedAsync(CancellationToken ct = default)
        => RunOnSelectedAsync("Bỏ ghi", async (item, token) =>
        {
            if (IsExport(item)) await _unconfirmOrder.ExecuteAsync(item.Id, token);
            else                await _unconfirmReceipt.ExecuteAsync(item.Id, token);
        }, ct);

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync(CancellationToken ct = default)
    {
        if (SelectedItem is not { } item) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{item.DocumentNumber}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        await RunOnSelectedAsync("Xóa", async (it, token) =>
        {
            if (IsExport(it)) await _deleteOrder.ExecuteAsync(it.Id, token);
            else              await _deleteReceipt.ExecuteAsync(it.Id, token);
        }, ct, keepSelection: false);
    }

    // Chạy 1 thao tác trên dòng đang chọn rồi tải lại danh sách; lỗi hiện qua banner đỏ của màn.
    private async Task RunOnSelectedAsync(
        string actionName,
        Func<WarehouseTransactionResponseDto, CancellationToken, Task> action,
        CancellationToken ct,
        bool keepSelection = true)
    {
        if (SelectedItem is not { } item) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            await action(item, ct);
            _logger.LogInformation("{Action} {Type} {DocumentNumber} from Kho screen", actionName, item.TransactionType, item.DocumentNumber);
        }
        catch (OperationCanceledException) { IsLoading = false; return; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Action} failed for {DocumentNumber}", actionName, item.DocumentNumber);
            HasError     = true;
            ErrorMessage = $"{actionName} thất bại: {ex.Message}";
            IsLoading    = false;
            return;
        }

        // LoadCoreAsync tự bật/tắt IsLoading — tắt trước để không chồng 2 lớp overlay.
        IsLoading = false;
        await LoadCoreAsync(keepSelection ? new WarehouseTransactionKey(item.Id, item.TransactionType) : null, ct);
    }

    // "Xem hóa đơn" — mở bản in của chứng từ đang chọn: Xuất kho → hóa đơn bán hàng, Nhập kho →
    // phiếu nhập kho.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task ViewInvoiceAsync(CancellationToken ct = default)
    {
        if (SelectedItem is not { } item) return;

        if (IsExport(item))
        {
            await ShowSalesInvoiceAsync(item, ct);
            return;
        }

        try
        {
            var receipt = await _getWarehouseReceiptById.ExecuteAsync(item.Id, ct);
            if (receipt is null)
            {
                MessageBox.Show($"Không tìm thấy phiếu nhập kho '{item.DocumentNumber}'.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = _receiptPrintWindowFactory();
            window.Initialize(receipt, null);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open print preview for warehouse receipt {DocumentNumber}", item.DocumentNumber);
            MessageBox.Show($"Không thể mở bản in: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Bản in hóa đơn bán hàng của dòng Xuất kho (popup "In Hóa Đơn", cùng nút 🖨 trong Chứng từ bán hàng).
    private async Task ShowSalesInvoiceAsync(WarehouseTransactionResponseDto item, CancellationToken ct)
    {
        try
        {
            var order = await _getSalesOrderById.ExecuteAsync(item.Id, ct);
            if (order is null)
            {
                MessageBox.Show($"Không tìm thấy chứng từ bán hàng '{item.DocumentNumber}'.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // SalesOrderResponseDto không mang Phone/Address khách hàng (chỉ CustomerId/Name)
            // — tra thêm qua danh sách khách hàng để hóa đơn in đủ thông tin, giống cách
            // SalesOrderViewModel.ShowPrintPreview lấy Phone/Address từ SelectedCustomer.
            var customers = await _getCustomers.ExecuteAsync(ct);
            var customer  = customers.FirstOrDefault(c => c.Id == order.CustomerId);

            var printWindow = _printWindowFactory();
            // Trừ cọc không được lưu lại trên SalesOrder đã ghi sổ (chỉ gửi riêng qua
            // DepositDeduction — xem sales.md), nên không tái tạo được số trừ cọc gốc ở đây;
            // hóa đơn in từ màn Kho tạm thời không hiện dòng trừ cọc (depositDeductionAmount=0).
            printWindow.Initialize(order, customer?.Phone, customer?.Address);
            printWindow.Owner = Application.Current.MainWindow;
            printWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sales order {DocumentNumber} for print from Kho screen", item.DocumentNumber);
            MessageBox.Show($"Không thể tải hóa đơn: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // "Xuất khẩu" — xuất đúng các dòng đang hiển thị (đã áp mọi bộ lọc) ra .xlsx.
    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"NhapXuatKho_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = BuildWorkbook(Items);
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Workbook danh sách giao dịch — dùng chung cho "Xuất khẩu" (các dòng đang hiện) và "Gửi email, Zalo" (dòng đang chọn).
    private static XLWorkbook BuildWorkbook(IEnumerable<WarehouseTransactionResponseDto> rows)
    {
        var workbook  = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Nhập, xuất kho");

        string[] headers =
        {
            "Ngày hạch toán", "Ngày chứng từ", "Số chứng từ", "Diễn giải", "Tổng tiền",
            "Người giao/Người nhận", "Đối tượng", "Đã lập CT bán hàng", "Ngày ghi sổ kho", "Loại chứng từ", "Trạng thái",
        };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value           = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var t in rows)
        {
            worksheet.Cell(row, 1).Value  = t.AccountingDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 2).Value  = t.DocumentDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 3).Value  = t.DocumentNumber;
            worksheet.Cell(row, 4).Value  = t.Description ?? string.Empty;
            worksheet.Cell(row, 5).Value  = t.TotalAmount;
            worksheet.Cell(row, 6).Value  = t.DeliveryOrReceiver ?? string.Empty;
            worksheet.Cell(row, 7).Value  = t.ObjectName ?? string.Empty;
            worksheet.Cell(row, 8).Value  = t.HasSalesOrder ? "Đã lập" : string.Empty;
            worksheet.Cell(row, 9).Value  = t.LedgerDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 10).Value = t.DocumentTypeLabel;
            worksheet.Cell(row, 11).Value = t.IsPosted ? "Đã ghi sổ" : "Chưa ghi sổ";
            row++;
        }

        worksheet.Columns().AdjustToContents();
        return workbook;
    }

    // "Gửi email, Zalo" (chuột phải) — như Chứng từ bán hàng: app chưa tích hợp SMTP/Zalo OA thật, nên
    // chỉ xuất file của dòng đang chọn rồi mở Email/Zalo client của máy để người dùng tự đính kèm.
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void SendEmail()
    {
        if (SelectedItem is not { } item) return;
        try
        {
            using var workbook = BuildWorkbook(new[] { item });
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, $"ChungTu_{item.DocumentNumber}");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenMailClient(
                $"Chứng từ kho - {item.DocumentNumber}",
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
        if (SelectedItem is not { } item) return;
        try
        {
            using var workbook = BuildWorkbook(new[] { item });
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, $"ChungTu_{item.DocumentNumber}");
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

    // Double-click / nút "Xem" 1 dòng chứng từ:
    // - Dòng Xuất kho (TransactionType="Export") LUÔN phát sinh từ 1 Sales Order (số chứng từ mang
    //   prefix XK — xem GetWarehouseTransactionsUseCase.MapSalesOrder phía BE) → mở form Chứng từ
    //   bán hàng (SalesOrderWindow) của chứng từ đó, khớp màn hình MISA (có Bỏ ghi/Sửa/In...). Lịch sử:
    //   trước đây mở thẳng bản in hóa đơn; nay in hóa đơn tách ra nút "Xem hóa đơn" (ShowInvoiceAsync).
    //   Không truyền SetSiblingContext vì danh sách ở đây chỉ có DTO rút gọn (không phải
    //   SalesOrderResponseDto đầy đủ) — Trước/Sau trong popup này không duyệt sang chứng từ khác.
    // - Dòng Nhập kho (TransactionType="Import") → mở thẳng phiếu nhập kho thật
    //   (WarehouseReceiptFormWindow, Id trùng với WarehouseReceipt.Id) — cho phép "Bỏ ghi" rồi sửa
    //   rồi "Ghi sổ" lại ngay từ đây.
    [RelayCommand]
    private async Task ShowDetailAsync(WarehouseTransactionResponseDto? item, CancellationToken ct = default)
    {
        if (item is null) return;

        if (item.TransactionType.Equals("Export", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var order = await _getSalesOrderById.ExecuteAsync(item.Id, ct);
                if (order is null)
                {
                    MessageBox.Show($"Không tìm thấy chứng từ bán hàng '{item.DocumentNumber}'.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var window = _salesOrderWindowFactory();
                window.Initialize(order, isFromWarehouseExport: true);
                window.Owner = Application.Current.MainWindow;
                if (window.ShowDialog() == true)
                    await LoadCoreAsync(new WarehouseTransactionKey(item.Id, item.TransactionType), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load sales order {DocumentNumber} from Kho screen", item.DocumentNumber);
                MessageBox.Show($"Không thể tải chứng từ bán hàng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return;
        }

        try
        {
            var receipt = await _getWarehouseReceiptById.ExecuteAsync(item.Id, ct);
            if (receipt is null)
            {
                MessageBox.Show($"Không tìm thấy phiếu nhập kho '{item.DocumentNumber}'.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = _formWindowFactory();
            window.Initialize(receipt);
            // Cho phép Trước/Sau/Thêm duyệt ngay trong popup — chỉ tính các dòng Nhập kho
            // ("Import") trong danh sách đang xem, bỏ qua dòng Xuất kho (mở popup khác hẳn).
            var siblingIds = Items
                .Where(i => i.TransactionType.Equals("Import", StringComparison.OrdinalIgnoreCase))
                .Select(i => i.Id)
                .ToList();
            window.SetSiblingContext(siblingIds, siblingIds.IndexOf(item.Id));
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true)
                LoadCommand.Execute(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load warehouse receipt {DocumentNumber} for edit from Kho screen", item.DocumentNumber);
            MessageBox.Show($"Không thể tải phiếu nhập kho: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
