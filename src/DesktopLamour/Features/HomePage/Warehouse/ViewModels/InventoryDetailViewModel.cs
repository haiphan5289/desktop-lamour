// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Shared.Models;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

// Drill-down target from TongHopTonKhoViewModel — "Sổ chi tiết vật tư hàng hóa" cho 1 sản phẩm:
// từng dòng Nhập/Xuất/Trả lại kèm Tồn chạy dần, kế thừa khoảng ngày/kho đang lọc ở màn tổng hợp.
public partial class InventoryDetailViewModel : ViewModelBase, INavigationParameterAware
{
    private readonly IGetInventoryDetailByProductUseCase _getDetail;
    private readonly IGetSalesOrderByIdUseCase           _getSalesOrderById;
    private readonly IGetWarehouseReceiptByIdUseCase     _getReceiptById;
    private readonly IGetCustomersUseCase                _getCustomers;
    private readonly INavigationService                  _navigationService;
    private readonly Func<SalesOrderPrintWindow>         _printWindowFactory;
    private readonly Func<WarehouseTransactionDetailWindow> _detailWindowFactory;
    private readonly ILogger<InventoryDetailViewModel>   _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasLines;
    [ObservableProperty] private string   _title = "";
    [ObservableProperty] private string   _filterSummary = "";

    [ObservableProperty] private int      _openingQty;
    [ObservableProperty] private decimal  _openingValue;
    [ObservableProperty] private int      _closingQty;
    [ObservableProperty] private decimal  _closingValue;

    // ── Per-column filter row, embedded directly in each header (no popup) ─────
    // Text columns: plain textbox, case-insensitive Contains against the cell's displayed text.
    // Date/numeric columns: an operator combo (=, ≤, ...) + a typed value, shown side by side.
    // Same pattern as SalesOrderReportDetailViewModel — see Shared/Models/ColumnFilterModels.cs.
    [ObservableProperty] private string _filterDocumentNumber = string.Empty;
    [ObservableProperty] private string _filterDescription    = string.Empty;
    [ObservableProperty] private string _filterUnit           = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value) => ApplyFilters();
    partial void OnFilterDescriptionChanged(string value)    => ApplyFilters();
    partial void OnFilterUnitChanged(string value)           => ApplyFilters();

    public DateColumnFilter AccountingDateFilter { get; } = new();
    public DateColumnFilter DocumentDateFilter   { get; } = new();

    public NumericColumnFilter ImportQtyFilter    { get; } = new();
    public NumericColumnFilter ImportValueFilter  { get; } = new();
    public NumericColumnFilter ExportQtyFilter    { get; } = new();
    public NumericColumnFilter ExportValueFilter  { get; } = new();
    public NumericColumnFilter RunningQtyFilter   { get; } = new();
    public NumericColumnFilter RunningValueFilter { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = ApplyFilters;
        DocumentDateFilter.Changed   = ApplyFilters;
        ImportQtyFilter.Changed      = ApplyFilters;
        ImportValueFilter.Changed    = ApplyFilters;
        ExportQtyFilter.Changed      = ApplyFilters;
        ExportValueFilter.Changed    = ApplyFilters;
        RunningQtyFilter.Changed     = ApplyFilters;
        RunningValueFilter.Changed   = ApplyFilters;
    }

    public ObservableCollection<InventoryDetailLine> Lines { get; } = new();

    // Full unfiltered dataset from the last LoadAsync — Lines is derived from this via ApplyFilters.
    private List<InventoryDetailLine> _allItems = new();

    private InventoryDetailFilter? _filter;

    public InventoryDetailViewModel(
        IGetInventoryDetailByProductUseCase getDetail,
        IGetSalesOrderByIdUseCase           getSalesOrderById,
        IGetWarehouseReceiptByIdUseCase     getReceiptById,
        IGetCustomersUseCase                getCustomers,
        INavigationService                  navigationService,
        Func<SalesOrderPrintWindow>         printWindowFactory,
        Func<WarehouseTransactionDetailWindow> detailWindowFactory,
        ILogger<InventoryDetailViewModel>   logger)
    {
        _getDetail           = getDetail;
        _getSalesOrderById   = getSalesOrderById;
        _getReceiptById      = getReceiptById;
        _getCustomers        = getCustomers;
        _navigationService   = navigationService;
        _printWindowFactory  = printWindowFactory;
        _detailWindowFactory = detailWindowFactory;
        _logger              = logger;

        WireColumnFilters();
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is not InventoryDetailFilter filter) return;
        _filter       = filter;
        Title         = $"Sổ chi tiết vật tư hàng hóa — {filter.ProductLabel}";
        FilterSummary = BuildFilterSummary(filter);
        _ = LoadAsync();
    }

    private static string BuildFilterSummary(InventoryDetailFilter filter)
    {
        var parts = new List<string>
        {
            $"Từ ngày {filter.FromDate:dd/MM/yyyy} đến ngày {filter.ToDate:dd/MM/yyyy}",
        };
        if (!string.IsNullOrWhiteSpace(filter.WarehouseLabel))
            parts.Add($"Kho: {filter.WarehouseLabel}");
        return string.Join(" · ", parts);
    }

    private async Task LoadAsync()
    {
        if (_filter is not { } filter) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var detail = await _getDetail.ExecuteAsync(
                filter.ProductId,
                DateOnly.FromDateTime(filter.FromDate),
                DateOnly.FromDateTime(filter.ToDate),
                filter.WarehouseIds);

            if (detail is not null)
            {
                _allItems = detail.Lines.ToList();

                OpeningQty   = detail.OpeningQty;
                OpeningValue = detail.OpeningValue;
                ClosingQty   = detail.ClosingQty;
                ClosingValue = detail.ClosingValue;
            }
            else
            {
                _allItems = new List<InventoryDetailLine>();
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // Re-derives Lines from _allItems using the active column filters.
    private void ApplyFilters()
    {
        Lines.Clear();
        foreach (var item in _allItems.Where(MatchesAllFilters))
            Lines.Add(item);

        HasLines = Lines.Count > 0;
    }

    private bool MatchesAllFilters(InventoryDetailLine item)
        => AccountingDateFilter.Matches(item.AccountingDate)
        && DocumentDateFilter.Matches(item.DocumentDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterDescription, item.Description ?? string.Empty)
        && Matches(FilterUnit, item.Unit)
        && ImportQtyFilter.Matches(item.ImportQty)
        && ImportValueFilter.Matches(item.ImportValue)
        && ExportQtyFilter.Matches(item.ExportQty)
        && ExportValueFilter.Matches(item.ExportValue)
        && RunningQtyFilter.Matches(item.RunningQty)
        && RunningValueFilter.Matches(item.RunningValue);

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    // Click "Số chứng từ" — Import mở lại phiếu nhập (popup Chi tiết cũ), Export mở popup
    // "In Hóa Đơn" (giống double-click dòng XK ở màn Kho — xem WarehouseTransactionListViewModel).
    [RelayCommand]
    private async Task OpenDocumentAsync(InventoryDetailLine? line, CancellationToken ct = default)
    {
        if (line is null || !line.IsClickable || line.SourceId is not { } sourceId) return;

        try
        {
            if (line.DocumentType == "Export")
            {
                var order = await _getSalesOrderById.ExecuteAsync(sourceId, ct);
                if (order is null)
                {
                    MessageBox.Show($"Không tìm thấy chứng từ bán hàng '{line.DocumentNumber}'.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var customers = await _getCustomers.ExecuteAsync(ct);
                var customer  = customers.FirstOrDefault(c => c.Id == order.CustomerId);

                var printWindow = _printWindowFactory();
                printWindow.Initialize(order, customer?.Phone, customer?.Address);
                printWindow.Owner = Application.Current.MainWindow;
                printWindow.ShowDialog();
                return;
            }

            if (line.DocumentType == "Import")
            {
                var receipt = await _getReceiptById.ExecuteAsync(sourceId, ct);
                if (receipt is null)
                {
                    MessageBox.Show($"Không tìm thấy phiếu nhập '{line.DocumentNumber}'.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var transaction = ToTransactionDto(receipt);
                var window = _detailWindowFactory();
                window.Initialize(transaction);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open document {DocumentNumber} from InventoryDetail", line.DocumentNumber);
            MessageBox.Show($"Không thể mở chứng từ: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Map thủ công sang shape mà WarehouseTransactionDetailWindow đang bind (chỉ dùng lại UI có
    // sẵn, không đổi gì ở đó) — khớp cách BE tự map WarehouseReceipt → WarehouseTransactionResponseDto
    // trong GetWarehouseTransactionsUseCase.MapReceipt.
    private static WarehouseTransactionResponseDto ToTransactionDto(WarehouseReceiptResponseDto r) => new()
    {
        Id                 = r.Id,
        TransactionType    = "Import",
        DocumentNumber     = r.ReceiptNumber,
        AccountingDate     = r.AccountingDate,
        DocumentDate       = r.DocumentDate,
        Description        = r.Description,
        TotalAmount        = r.TotalAmount,
        DeliveryOrReceiver = r.DeliveryPerson,
        ObjectName         = r.CustomerName ?? r.SupplierName,
        HasSalesOrder      = false,
        LedgerDate         = r.CreatedAt,
        DocumentTypeLabel  = "Nhập kho",
        Lines = r.Lines.Select(l => new WarehouseTransactionLineDto
        {
            ProductCode   = l.ProductCode,
            ProductName   = l.ProductName,
            WarehouseName = l.WarehouseName,
            DebitAccount  = l.DebitAccount,
            CreditAccount = l.CreditAccount,
            Unit          = l.Unit,
            Quantity      = l.Quantity,
            UnitPrice     = l.UnitPrice,
            Amount        = l.Amount,
        }).ToList(),
    };
}
