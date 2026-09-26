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

    // 2026-09-18: khi drill từ report 1 chiều "Nhân viên", MISA đổi hẳn tiêu đề cửa sổ thành "SỔ CHI
    // TIẾT BÁN HÀNG THEO NHÂN VIÊN" (ảnh mẫu xác nhận) thay vì tiêu đề chung. Các biến thể khác
    // (Khách hàng/Mặt hàng 1 chiều, mọi report 2-3 chiều) CHƯA có ảnh mẫu xác nhận nên vẫn giữ tiêu
    // đề chung — tránh đoán mò lặp lại sai lầm đã gặp ở SalesOrderReportViewModel.
    [ObservableProperty] private string _reportTitle = "SỔ CHI TIẾT BÁN HÀNG";

    // 2026-09-18: khớp mẫu MISA — 1 dòng subtitle duy nhất "{Title}; {FilterSummary}" thay vì 2
    // label riêng như trước.
    public string Subtitle => string.IsNullOrWhiteSpace(Title) ? FilterSummary : $"{Title}; {FilterSummary}";
    partial void OnTitleChanged(string value)         => OnPropertyChanged(nameof(Subtitle));
    partial void OnFilterSummaryChanged(string value) => OnPropertyChanged(nameof(Subtitle));

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
    [ObservableProperty] private string _filterDescription    = string.Empty;
    [ObservableProperty] private string _filterCustomerCode   = string.Empty;
    [ObservableProperty] private string _filterCustomerName   = string.Empty;
    [ObservableProperty] private string _filterProductCode    = string.Empty;
    [ObservableProperty] private string _filterProductName    = string.Empty;
    [ObservableProperty] private string _filterUnit           = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value) => ApplyFilters();
    partial void OnFilterDescriptionChanged(string value)    => ApplyFilters();
    partial void OnFilterCustomerCodeChanged(string value)   => ApplyFilters();
    partial void OnFilterCustomerNameChanged(string value)   => ApplyFilters();
    partial void OnFilterProductCodeChanged(string value)    => ApplyFilters();
    partial void OnFilterProductNameChanged(string value)    => ApplyFilters();
    partial void OnFilterUnitChanged(string value)           => ApplyFilters();

    public DateColumnFilter AccountingDateFilter { get; } = new();
    public DateColumnFilter DocumentDateFilter   { get; } = new();

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
        DocumentDateFilter.Changed   = ApplyFilters;
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
        FilterDocumentNumber = FilterDescription = FilterCustomerCode = FilterCustomerName =
            FilterProductCode = FilterProductName = FilterUnit = string.Empty;

        AccountingDateFilter.Operator = FilterOperator.Equal;
        AccountingDateFilter.Value    = null;
        DocumentDateFilter.Operator   = FilterOperator.Equal;
        DocumentDateFilter.Value      = null;

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
        ReportTitle    = filter.SourceReportType == SalesOrderReportTypes.ByEmployee
            ? "SỔ CHI TIẾT BÁN HÀNG THEO NHÂN VIÊN"
            : "SỔ CHI TIẾT BÁN HÀNG";
        _ = LoadAsync();
    }

    // 2026-09-18: gộp "Từ ngày X" + "đến ngày Y" thành 1 cụm liền mạch (khớp mẫu MISA "Từ ngày
    // 01/9/2026 đến ngày 14/9/2026") thay vì 2 phần nối bằng "·" như trước — "·" chỉ còn dùng để
    // ngăn cách cụm ngày với các filter khác (ĐVT/Nhóm VTHH) khi có.
    private static string BuildFilterSummary(SalesOrderDetailFilter filter)
    {
        var parts = new List<string>();
        if (filter.FromDate.HasValue && filter.ToDate.HasValue)
            parts.Add($"Từ ngày {filter.FromDate.Value:dd/M/yyyy} đến ngày {filter.ToDate.Value:dd/M/yyyy}");
        else if (filter.FromDate.HasValue)
            parts.Add($"Từ ngày {filter.FromDate.Value:dd/M/yyyy}");
        else if (filter.ToDate.HasValue)
            parts.Add($"đến ngày {filter.ToDate.Value:dd/M/yyyy}");
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
        && DocumentDateFilter.Matches(item.DocumentDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterDescription, item.Description ?? "")
        && Matches(FilterCustomerCode, item.CustomerCode)
        && Matches(FilterCustomerName, item.CustomerName)
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

        // 2026-09-18: khớp bộ cột mới theo mẫu MISA — thêm Ngày chứng từ/Diễn giải chung/Diễn giải/
        // Mã khách hàng, bỏ Nhân viên (không có trong mẫu), đổi tên Khách hàng→Tên khách hàng và
        // Số lượng→Tổng số lượng bán (xem DataGrid.Columns trong View để khớp đúng thứ tự).
        string[] headers =
        {
            "Ngày hạch toán", "Ngày chứng từ", "Số chứng từ", "Diễn giải chung", "Diễn giải",
            "Mã khách hàng", "Tên khách hàng", "Mã hàng", "Tên hàng", "ĐVT",
            "Tổng số lượng bán", "Đơn giá", "Tỷ lệ CK(%)", "Thành tiền", "Thuế suất", "Tiền thuế", "Tổng cộng",
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
            worksheet.Cell(row, 2).Value  = line.DocumentDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 3).Value  = line.DocumentNumber;
            worksheet.Cell(row, 4).Value  = line.Description ?? "";
            worksheet.Cell(row, 5).Value  = line.ProductName;
            worksheet.Cell(row, 6).Value  = line.CustomerCode;
            worksheet.Cell(row, 7).Value  = line.CustomerName;
            worksheet.Cell(row, 8).Value  = line.ProductCode;
            worksheet.Cell(row, 9).Value  = line.ProductName;
            worksheet.Cell(row, 10).Value = line.Unit;
            worksheet.Cell(row, 11).Value = line.Quantity;
            worksheet.Cell(row, 12).Value = line.UnitPrice;
            worksheet.Cell(row, 13).Value = line.DiscountRate;
            worksheet.Cell(row, 14).Value = line.Amount;
            worksheet.Cell(row, 15).Value = line.TaxRate;
            worksheet.Cell(row, 16).Value = line.TaxAmount;
            worksheet.Cell(row, 17).Value = line.GrandTotal;
            row++;
        }

        worksheet.Cell(row, 9).Value             = "Tổng cộng";
        worksheet.Cell(row, 9).Style.Font.Bold   = true;
        worksheet.Cell(row, 11).Value             = TotalQuantity;
        worksheet.Cell(row, 11).Style.Font.Bold   = true;
        worksheet.Cell(row, 14).Value             = TotalAmount;
        worksheet.Cell(row, 14).Style.Font.Bold   = true;
        worksheet.Cell(row, 16).Value             = TotalTaxAmount;
        worksheet.Cell(row, 16).Style.Font.Bold   = true;
        worksheet.Cell(row, 17).Value             = TotalGrandTotal;
        worksheet.Cell(row, 17).Style.Font.Bold   = true;

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

        doc.Blocks.Add(new Paragraph(new Bold(new Run(ReportTitle)) { FontSize = 18 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 4),
        });
        doc.Blocks.Add(new Paragraph(new Run(Subtitle))
        {
            TextAlignment = TextAlignment.Center,
            FontSize      = 12,
            Margin        = new Thickness(0, 0, 0, 12),
        });

        // 2026-09-18: khớp bộ cột mới theo mẫu MISA — thêm Ngày CT/Diễn giải chung/Diễn giải/Mã KH,
        // bỏ Nhân viên. "Mã hàng" vẫn không có trong bản in (giữ nguyên như trước, chỉ Excel export
        // mới liệt kê đủ cả mã lẫn tên hàng) để bảng in không quá chật.
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 4, 0, 4) };
        foreach (var width in new[] { 50, 50, 60, 70, 70, 45, 75, 75, 35, 35, 55, 40, 55, 45, 50, 55 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow(
            "Ngày HT", "Ngày CT", "Số chứng từ", "Diễn giải chung", "Diễn giải",
            "Mã KH", "Khách hàng", "Tên hàng", "ĐVT", "SL", "Đơn giá", "CK(%)",
            "Thành tiền", "Thuế suất", "Tiền thuế", "Tổng cộng"));

        foreach (var line in Items)
        {
            rowGroup.Rows.Add(DataRow(
                line.AccountingDate.ToString("dd/MM/yyyy"),
                line.DocumentDate.ToString("dd/MM/yyyy"),
                line.DocumentNumber,
                line.Description ?? "",
                line.ProductName,
                line.CustomerCode,
                line.CustomerName,
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
        row.Cells.Add(BoldCell("Tổng cộng", 9));
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

    private static string FormatMoney(decimal value) => MoneyFormat.Format(value);
}
