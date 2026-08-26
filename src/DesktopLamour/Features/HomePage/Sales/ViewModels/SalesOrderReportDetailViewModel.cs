// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

// Drill-down target from SalesOrderReportViewModel — shows the raw sales lines behind
// one summary row (e.g. one customer, one product) instead of the aggregated total.
public partial class SalesOrderReportDetailViewModel : ViewModelBase, INavigationParameterAware
{
    private readonly IGetSalesOrderReportUseCase _getReport;
    private readonly INavigationService          _navigationService;
    private readonly IGetSalesOrderByIdUseCase   _getOrderById;
    private readonly Func<SalesOrderWindow>      _salesOrderWindowFactory;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasLines;
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _filterSummary = "";
    [ObservableProperty] private int    _rowCount;

    [ObservableProperty] private int     _totalQuantity;
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private decimal _totalTaxAmount;
    [ObservableProperty] private decimal _totalGrandTotal;

    // ── Per-column filter row, embedded directly in each header (no popup) ─────
    // Text columns: plain textbox, case-insensitive Contains against the cell's displayed text.
    // Date/numeric columns: an operator combo (=, ≤, ...) + a typed value, shown side by side.
    // Items always holds the FILTERED subset — Print/Xuất Excel/Email/Zalo iterate Items too, so
    // they automatically reflect the active filters without any extra wiring.
    [ObservableProperty] private string _filterDocumentNumber = string.Empty;
    [ObservableProperty] private string _filterCustomerName   = string.Empty;
    [ObservableProperty] private string _filterEmployeeName   = string.Empty;
    [ObservableProperty] private string _filterProductCode    = string.Empty;
    [ObservableProperty] private string _filterProductName    = string.Empty;
    [ObservableProperty] private string _filterUnit           = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value) => ApplyFilters();
    partial void OnFilterCustomerNameChanged(string value)   => ApplyFilters();
    partial void OnFilterEmployeeNameChanged(string value)   => ApplyFilters();
    partial void OnFilterProductCodeChanged(string value)    => ApplyFilters();
    partial void OnFilterProductNameChanged(string value)    => ApplyFilters();
    partial void OnFilterUnitChanged(string value)           => ApplyFilters();

    public DateColumnFilter AccountingDateFilter { get; } = new();

    public NumericColumnFilter QuantityFilter     { get; } = new();
    public NumericColumnFilter UnitPriceFilter    { get; } = new();
    public NumericColumnFilter DiscountRateFilter { get; } = new();
    public NumericColumnFilter AmountFilter       { get; } = new();
    public NumericColumnFilter TaxRateFilter      { get; } = new();
    public NumericColumnFilter TaxAmountFilter    { get; } = new();
    public NumericColumnFilter GrandTotalFilter   { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = ApplyFilters;
        QuantityFilter.Changed       = ApplyFilters;
        UnitPriceFilter.Changed      = ApplyFilters;
        DiscountRateFilter.Changed   = ApplyFilters;
        AmountFilter.Changed         = ApplyFilters;
        TaxRateFilter.Changed        = ApplyFilters;
        TaxAmountFilter.Changed      = ApplyFilters;
        GrandTotalFilter.Changed     = ApplyFilters;
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FilterDocumentNumber = FilterCustomerName = FilterEmployeeName =
            FilterProductCode = FilterProductName = FilterUnit = string.Empty;

        AccountingDateFilter.Operator = FilterOperator.Equal;
        AccountingDateFilter.Value    = null;

        foreach (var f in new[] { QuantityFilter, UnitPriceFilter, DiscountRateFilter, AmountFilter, TaxRateFilter, TaxAmountFilter, GrandTotalFilter })
        {
            f.Operator  = FilterOperator.LessOrEqual;
            f.ValueText = string.Empty;
        }
    }

    public ObservableCollection<SalesOrderReportLineItem> Items { get; } = new();

    // Full unfiltered dataset from the last LoadAsync — Items is derived from this via ApplyFilters.
    private List<SalesOrderReportLineItem> _allItems = new();

    private SalesOrderDetailFilter? _filter;

    public SalesOrderReportDetailViewModel(
        IGetSalesOrderReportUseCase getReport,
        INavigationService          navigationService,
        IGetSalesOrderByIdUseCase   getOrderById,
        Func<SalesOrderWindow>      salesOrderWindowFactory)
    {
        _getReport               = getReport;
        _navigationService       = navigationService;
        _getOrderById            = getOrderById;
        _salesOrderWindowFactory = salesOrderWindowFactory;

        WireColumnFilters();
    }

    // Double-click 1 dòng trong "Sổ chi tiết bán hàng" → mở lại popup "Chứng từ bán hàng" ở chế
    // độ chỉ xem (IsReadOnly=true) — xem code-behind DetailGrid_MouseDoubleClick.
    [RelayCommand]
    private async Task OpenOrderAsync(SalesOrderReportLineItem? row, CancellationToken ct = default)
    {
        if (row is null) return;

        IsLoading = true;
        try
        {
            var order = await _getOrderById.ExecuteAsync(row.OrderId, ct);
            if (order is null)
            {
                MessageBox.Show($"Không tìm thấy chứng từ '{row.DocumentNumber}'.", "Không tìm thấy",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = _salesOrderWindowFactory();
            window.Initialize(order, isReadOnly: true);
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải chứng từ: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsLoading = false; }
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is not SalesOrderDetailFilter filter) return;
        _filter        = filter;
        Title          = filter.Title;
        FilterSummary  = BuildFilterSummary(filter);
        _ = LoadAsync();
    }

    private static string BuildFilterSummary(SalesOrderDetailFilter filter)
    {
        var parts = new List<string>();
        if (filter.FromDate.HasValue) parts.Add($"Từ ngày {filter.FromDate.Value:dd/MM/yyyy}");
        if (filter.ToDate.HasValue)   parts.Add($"đến ngày {filter.ToDate.Value:dd/MM/yyyy}");
        if (!string.IsNullOrWhiteSpace(filter.Unit))     parts.Add($"ĐVT: {filter.Unit}");
        if (!string.IsNullOrWhiteSpace(filter.Category)) parts.Add($"Nhóm VTHH: {filter.Category}");
        return parts.Count > 0 ? string.Join(" · ", parts) : "Tất cả chứng từ";
    }

    private async Task LoadAsync()
    {
        if (_filter is not { } filter) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var productIds = filter.ProductId.HasValue ? new[] { filter.ProductId.Value } : null;
            var lines = await _getReport.ExecuteAsync(
                productIds, filter.EmployeeId, filter.CustomerId,
                filter.Unit, filter.Category, filter.FromDate, filter.ToDate);

            _allItems = lines.OrderByDescending(l => l.AccountingDate)
                .Select(SalesOrderReportLineItem.FromDto)
                .ToList();

            ApplyFilters();
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // Re-derives Items (and the footer totals) from _allItems using the active column filters.
    private void ApplyFilters()
    {
        Items.Clear();
        foreach (var item in _allItems.Where(MatchesAllFilters))
            Items.Add(item);

        HasLines        = Items.Count > 0;
        RowCount        = Items.Count;
        TotalQuantity   = Items.Sum(i => i.Quantity);
        TotalAmount     = Items.Sum(i => i.Amount);
        TotalTaxAmount  = Items.Sum(i => i.TaxAmount);
        TotalGrandTotal = Items.Sum(i => i.GrandTotal);
    }

    private bool MatchesAllFilters(SalesOrderReportLineItem item)
        => AccountingDateFilter.Matches(item.AccountingDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterCustomerName, item.CustomerName)
        && Matches(FilterEmployeeName, item.EmployeeName)
        && Matches(FilterProductCode, item.ProductCode)
        && Matches(FilterProductName, item.ProductName)
        && Matches(FilterUnit, item.Unit)
        && QuantityFilter.Matches(item.Quantity)
        && UnitPriceFilter.Matches(item.UnitPrice)
        && DiscountRateFilter.Matches(item.DiscountRate)
        && AmountFilter.Matches(item.Amount)
        && TaxRateFilter.Matches(item.TaxRate)
        && TaxAmountFilter.Matches(item.TaxAmount)
        && GrandTotalFilter.Matches(item.GrandTotal);

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void Print()
    {
        var document = BuildReportDocument();

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = printDialog.PrintableAreaHeight;
        document.PageWidth   = printDialog.PrintableAreaWidth;
        document.PagePadding = new Thickness(30);
        document.ColumnWidth = printDialog.PrintableAreaWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Sổ chi tiết bán hàng - {Title}");
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"SoChiTietBanHang_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = BuildWorkbook();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void SendEmail()
    {
        try
        {
            using var workbook = BuildWorkbook();
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, "SoChiTietBanHang");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenMailClient(
                $"Sổ chi tiết bán hàng - {Title}",
                $"File báo cáo đã được lưu tại:\n{path}\n\nVui lòng đính kèm file này vào email trước khi gửi.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Email thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void SendZalo()
    {
        try
        {
            using var workbook = BuildWorkbook();
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, "SoChiTietBanHang");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenZaloApp();

            MessageBox.Show("Đã mở Zalo và thư mục chứa file báo cáo. Vui lòng kéo-thả file để đính kèm.",
                "Gửi Zalo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Zalo thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private ClosedXML.Excel.XLWorkbook BuildWorkbook()
    {
        var workbook  = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sổ chi tiết");

        string[] headers =
        {
            "Ngày hạch toán", "Số chứng từ", "Khách hàng", "Nhân viên",
            "Mã hàng", "Tên hàng", "ĐVT", "Số lượng", "Đơn giá",
            "Tỷ lệ CK(%)", "Thành tiền", "Thuế suất", "Tiền thuế", "Tổng cộng",
        };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value           = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var line in Items)
        {
            worksheet.Cell(row, 1).Value  = line.AccountingDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 2).Value  = line.DocumentNumber;
            worksheet.Cell(row, 3).Value  = line.CustomerName;
            worksheet.Cell(row, 4).Value  = line.EmployeeName;
            worksheet.Cell(row, 5).Value  = line.ProductCode;
            worksheet.Cell(row, 6).Value  = line.ProductName;
            worksheet.Cell(row, 7).Value  = line.Unit;
            worksheet.Cell(row, 8).Value  = line.Quantity;
            worksheet.Cell(row, 9).Value  = line.UnitPrice;
            worksheet.Cell(row, 10).Value = line.DiscountRate;
            worksheet.Cell(row, 11).Value = line.Amount;
            worksheet.Cell(row, 12).Value = line.TaxRate;
            worksheet.Cell(row, 13).Value = line.TaxAmount;
            worksheet.Cell(row, 14).Value = line.GrandTotal;
            row++;
        }

        worksheet.Cell(row, 6).Value            = "Tổng cộng";
        worksheet.Cell(row, 6).Style.Font.Bold  = true;
        worksheet.Cell(row, 8).Value             = TotalQuantity;
        worksheet.Cell(row, 8).Style.Font.Bold   = true;
        worksheet.Cell(row, 11).Value             = TotalAmount;
        worksheet.Cell(row, 11).Style.Font.Bold   = true;
        worksheet.Cell(row, 13).Value             = TotalTaxAmount;
        worksheet.Cell(row, 13).Style.Font.Bold   = true;
        worksheet.Cell(row, 14).Value             = TotalGrandTotal;
        worksheet.Cell(row, 14).Style.Font.Bold   = true;

        worksheet.Columns().AdjustToContents();
        return workbook;
    }

    private FlowDocument BuildReportDocument()
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 10,
            PagePadding = new Thickness(20),
        };

        doc.Blocks.Add(new Paragraph(new Bold(new Run("SỔ CHI TIẾT BÁN HÀNG")) { FontSize = 18 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 4),
        });
        doc.Blocks.Add(new Paragraph(new Run(Title)) { TextAlignment = TextAlignment.Center, FontSize = 13, Margin = new Thickness(0, 0, 0, 2) });
        doc.Blocks.Add(new Paragraph(new Run(FilterSummary))
        {
            TextAlignment = TextAlignment.Center,
            FontSize      = 11,
            Margin        = new Thickness(0, 0, 0, 12),
        });

        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 4, 0, 4) };
        foreach (var width in new[] { 65, 70, 100, 90, 90, 55, 45, 65, 55, 70, 55, 65, 70 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow(
            "Ngày HT", "Số chứng từ", "Khách hàng", "Nhân viên", "Tên hàng",
            "ĐVT", "SL", "Đơn giá", "CK(%)", "Thành tiền", "Thuế suất", "Tiền thuế", "Tổng cộng"));

        foreach (var line in Items)
        {
            rowGroup.Rows.Add(DataRow(
                line.AccountingDate.ToString("dd/MM/yyyy"),
                line.DocumentNumber,
                line.CustomerName,
                line.EmployeeName,
                line.ProductName,
                line.Unit,
                line.Quantity.ToString(),
                FormatMoney(line.UnitPrice),
                line.DiscountRate.ToString("0.##"),
                FormatMoney(line.Amount),
                $"{line.TaxRate:0}%",
                FormatMoney(line.TaxAmount),
                FormatMoney(line.GrandTotal)));
        }

        rowGroup.Rows.Add(TotalsRow());
        table.RowGroups.Add(rowGroup);
        doc.Blocks.Add(table);

        return doc;
    }

    private TableRow TotalsRow()
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        row.Cells.Add(BoldCell("Tổng cộng", 6));
        row.Cells.Add(BoldCell(TotalQuantity.ToString()));
        row.Cells.Add(BoldCell(""));
        row.Cells.Add(BoldCell(""));
        row.Cells.Add(BoldCell(FormatMoney(TotalAmount)));
        row.Cells.Add(BoldCell(""));
        row.Cells.Add(BoldCell(FormatMoney(TotalTaxAmount)));
        row.Cells.Add(BoldCell(FormatMoney(TotalGrandTotal)));
        return row;
    }

    private static TableCell BoldCell(string text, int columnSpan = 1)
        => new(new Paragraph(new Bold(new Run(text))) { TextAlignment = TextAlignment.Center })
        {
            Padding         = new Thickness(3),
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            ColumnSpan      = columnSpan,
        };

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        foreach (var h in headers)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(h))) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(3),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static TableRow DataRow(params string[] values)
    {
        var row = new TableRow();
        foreach (var v in values)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(v)) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(3),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
}
