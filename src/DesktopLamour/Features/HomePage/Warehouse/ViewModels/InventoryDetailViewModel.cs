// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
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
    private readonly INavigationService                  _navigationService;
    private readonly Func<SalesOrderWindow>              _salesOrderWindowFactory;
    private readonly Func<WarehouseReceiptFormWindow>    _receiptWindowFactory;
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
    [ObservableProperty] private string _filterWarehouse      = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value) => ApplyFilters();
    partial void OnFilterDescriptionChanged(string value)    => ApplyFilters();
    partial void OnFilterUnitChanged(string value)           => ApplyFilters();
    partial void OnFilterWarehouseChanged(string value)      => ApplyFilters();

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
    // Số dư đầu/cuối kỳ riêng từng kho + thông tin mặt hàng — dựng dòng "Số dư đầu kỳ" và dòng nhóm.
    private List<InventoryDetailWarehouse> _warehouses = new();
    private string _productCode = string.Empty;
    private string _productName = string.Empty;
    private string _productUnit = string.Empty;

    private InventoryDetailFilter? _filter;

    public InventoryDetailViewModel(
        IGetInventoryDetailByProductUseCase getDetail,
        IGetSalesOrderByIdUseCase           getSalesOrderById,
        IGetWarehouseReceiptByIdUseCase     getReceiptById,
        INavigationService                  navigationService,
        Func<SalesOrderWindow>              salesOrderWindowFactory,
        Func<WarehouseReceiptFormWindow>    receiptWindowFactory,
        ILogger<InventoryDetailViewModel>   logger)
    {
        _getDetail           = getDetail;
        _getSalesOrderById   = getSalesOrderById;
        _getReceiptById      = getReceiptById;
        _navigationService   = navigationService;
        _salesOrderWindowFactory = salesOrderWindowFactory;
        _receiptWindowFactory    = receiptWindowFactory;
        _logger              = logger;

        WireColumnFilters();
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is not InventoryDetailFilter filter) return;
        _filter       = filter;
        Title         = string.Empty;
        FilterSummary = BuildFilterSummary(filter);
        _ = LoadAsync();
    }

    // Khớp tiêu đề MISA: "Kho: Hàng Hóa; Mặt hàng: Meso Fills; Từ ngày 01/10/2026 đến ngày 07/10/2026".
    private static string BuildFilterSummary(InventoryDetailFilter filter)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.WarehouseLabel))
            parts.Add($"Kho: {filter.WarehouseLabel}");
        parts.Add($"Mặt hàng: {filter.ProductLabel}");
        parts.Add($"Từ ngày {filter.FromDate:dd/MM/yyyy} đến ngày {filter.ToDate:dd/MM/yyyy}");
        return string.Join("; ", parts);
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
                _allItems    = detail.Lines.ToList();
                _warehouses  = detail.Warehouses.ToList();
                _productCode = detail.Code;
                _productName = detail.Name;
                _productUnit = detail.Unit;

                OpeningQty   = detail.OpeningQty;
                OpeningValue = detail.OpeningValue;
                ClosingQty   = detail.ClosingQty;
                ClosingValue = detail.ClosingValue;
            }
            else
            {
                _allItems   = new List<InventoryDetailLine>();
                _warehouses = new List<InventoryDetailWarehouse>();
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

    // Re-derives Lines from _allItems using the active column filters, then dựng bố cục MISA cho từng kho:
    //   Mã kho : HH (n)            ← dòng nhóm kho (tổng Nhập/Xuất, Tồn = tồn cuối kỳ của kho)
    //     Mã hàng : 15 (n)         ← dòng nhóm mặt hàng
    //       Số dư đầu kỳ           ← tồn đầu kỳ của kho
    //       … các dòng giao dịch (Tồn chạy dần riêng từng kho)
    // n ở nhóm mặt hàng = số dòng giao dịch + dòng số dư đầu kỳ (khớp "Mã hàng : 15 (8)" của MISA).
    private void ApplyFilters()
    {
        Lines.Clear();

        var filtered = _allItems.Where(MatchesAllFilters).ToList();

        // Thứ tự kho theo BE (đã sắp theo tên kho); kho nào chỉ có trong dòng giao dịch mà BE không liệt kê
        // thì vẫn hiện (không mất dữ liệu) với số dư đầu kỳ 0.
        var warehouses = _warehouses.ToList();
        foreach (var l in _allItems)
            if (warehouses.All(w => w.WarehouseId != l.WarehouseId))
                warehouses.Add(new InventoryDetailWarehouse
                {
                    WarehouseId = l.WarehouseId, WarehouseCode = l.WarehouseCode, WarehouseName = l.WarehouseName,
                });

        foreach (var wh in warehouses)
        {
            var lines = filtered.Where(l => l.WarehouseId == wh.WarehouseId).ToList();
            if (lines.Count == 0 && _allItems.Count > 0 && _allItems.All(l => l.WarehouseId != wh.WarehouseId)
                && wh.OpeningQty == 0 && wh.ClosingQty == 0)
                continue; // kho không dính gì tới mặt hàng này

            var importQty    = lines.Sum(l => l.ImportQty);
            var importValue  = lines.Sum(l => l.ImportValue);
            var exportQty    = lines.Sum(l => l.ExportQty);
            var exportValue  = lines.Sum(l => l.ExportValue);

            Lines.Add(new InventoryDetailLine
            {
                RowKind = InventoryDetailRowKind.WarehouseHeader,
                WarehouseId = wh.WarehouseId, WarehouseName = $"Mã kho : {wh.WarehouseCode} (1)",
                ImportQty = importQty, ImportValue = importValue, ExportQty = exportQty, ExportValue = exportValue,
                RunningQty = wh.ClosingQty, RunningValue = wh.ClosingValue,
            });
            Lines.Add(new InventoryDetailLine
            {
                RowKind = InventoryDetailRowKind.ProductHeader,
                WarehouseId = wh.WarehouseId, WarehouseName = $"Mã hàng : {_productCode} ({lines.Count + 1})",
                ImportQty = importQty, ImportValue = importValue, ExportQty = exportQty, ExportValue = exportValue,
                RunningQty = wh.ClosingQty, RunningValue = wh.ClosingValue,
            });
            Lines.Add(new InventoryDetailLine
            {
                RowKind = InventoryDetailRowKind.Opening,
                WarehouseId = wh.WarehouseId, WarehouseCode = wh.WarehouseCode, WarehouseName = wh.WarehouseName,
                ProductName = _productName, Description = "Số dư đầu kỳ", Unit = _productUnit,
                RunningQty = wh.OpeningQty, RunningValue = wh.OpeningValue,
            });
            foreach (var l in lines) Lines.Add(l);
        }

        HasLines = Lines.Count > 0;
    }

    private bool MatchesAllFilters(InventoryDetailLine item)
        => AccountingDateFilter.Matches(item.AccountingDate)
        && DocumentDateFilter.Matches(item.DocumentDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterDescription, item.Description ?? string.Empty)
        && Matches(FilterUnit, item.Unit)
        && Matches(FilterWarehouse, item.WarehouseName)
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

    // Nhấp đúp "Số chứng từ" — mở lại đúng chứng từ gốc như MISA, ở chế độ chỉ xem:
    // Export → form "Chứng từ bán hàng" (SalesOrderWindow, khóa không sửa được; cùng cách màn Kho mở dòng XK),
    // Import → form "Phiếu nhập kho" (WarehouseReceiptFormWindow, phiếu đã ghi sổ nên tự khóa).
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

                var window = _salesOrderWindowFactory();
                window.Initialize(order, isFromWarehouseExport: true, isReadOnly: true);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
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

                var window = _receiptWindowFactory();
                window.Initialize(receipt);
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
}
