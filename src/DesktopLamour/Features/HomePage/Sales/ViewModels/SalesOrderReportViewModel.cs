// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderReportViewModel : ViewModelBase, INavigationParameterAware
{
    private readonly IGetSalesOrderSummaryReportUseCase _getSummaryReport;
    private readonly INavigationService                 _navigationService;
    private readonly Func<SalesOrderReportFilterWindow>  _reportFilterWindowFactory;

    [ObservableProperty] private bool                    _isLoading;
    [ObservableProperty] private bool                    _hasError;
    [ObservableProperty] private string                  _errorMessage = string.Empty;
    [ObservableProperty] private bool                    _hasLines;
    [ObservableProperty] private SalesOrderReportFilter?  _currentFilter;

    [ObservableProperty] private int     _rowCount;
    [ObservableProperty] private int     _totalQuantitySold;
    [ObservableProperty] private decimal _totalSalesAmount;
    [ObservableProperty] private decimal _totalDiscountAmount;
    [ObservableProperty] private int     _totalReturnQuantity;
    [ObservableProperty] private decimal _totalReturnValue;
    [ObservableProperty] private decimal _totalNetRevenue;

    [ObservableProperty] private bool _isUnitColumnVisible;

    public ObservableCollection<SalesOrderSummaryLineItem> Items { get; } = new();

    private List<ReportDisplayRow> _displayRows = new();

    [ObservableProperty]
    private ICollectionView _rowsView = CollectionViewSource.GetDefaultView(new List<ReportDisplayRow>());

    public string ReportTitle =>
        $"TỔNG HỢP BÁN HÀNG THEO {(CurrentFilter?.ReportType ?? SalesOrderReportTypes.ByProduct).ToUpperInvariant()}";

    partial void OnCurrentFilterChanged(SalesOrderReportFilter? value) => OnPropertyChanged(nameof(ReportTitle));

    private static readonly Dictionary<string, (SummaryDimension Field, Func<SalesOrderSummaryLineItem, string> Key, string Label)[]> GroupingsByType = new()
    {
        [SalesOrderReportTypes.ByProduct] =
            new[] { (Field: SummaryDimension.Product, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.ProductName), Label: "Mặt hàng") },
        [SalesOrderReportTypes.ByProductThenCustomer] =
            new[]
            {
                (Field: SummaryDimension.Product,  Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.ProductName),  Label: "Mặt hàng"),
                (Field: SummaryDimension.Customer, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.CustomerName), Label: "Khách hàng"),
            },
        [SalesOrderReportTypes.ByProductThenEmployee] =
            new[]
            {
                (Field: SummaryDimension.Product,  Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.ProductName),  Label: "Mặt hàng"),
                (Field: SummaryDimension.Employee, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.EmployeeName), Label: "Nhân viên"),
            },
        [SalesOrderReportTypes.ByCustomer] =
            new[] { (Field: SummaryDimension.Customer, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.CustomerName), Label: "Khách hàng") },
        [SalesOrderReportTypes.ByEmployee] =
            new[] { (Field: SummaryDimension.Employee, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.EmployeeName), Label: "Nhân viên") },
        [SalesOrderReportTypes.ByCustomerThenEmployee] =
            new[]
            {
                (Field: SummaryDimension.Customer, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.CustomerName), Label: "Khách hàng"),
                (Field: SummaryDimension.Employee, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.EmployeeName), Label: "Nhân viên"),
            },
        [SalesOrderReportTypes.ByCustomerThenProduct] =
            new[]
            {
                (Field: SummaryDimension.Customer, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.CustomerName), Label: "Khách hàng"),
                (Field: SummaryDimension.Product,  Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.ProductName),  Label: "Mặt hàng"),
            },
    };

    public SalesOrderReportViewModel(
        IGetSalesOrderSummaryReportUseCase getSummaryReport,
        INavigationService                 navigationService,
        Func<SalesOrderReportFilterWindow>  reportFilterWindowFactory)
    {
        _getSummaryReport          = getSummaryReport;
        _navigationService         = navigationService;
        _reportFilterWindowFactory = reportFilterWindowFactory;
    }

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is not SalesOrderReportFilter filter) return;
        CurrentFilter = filter;
        _ = LoadAsync();
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var filter = CurrentFilter;
            var items = await _getSummaryReport.ExecuteAsync(
                filter?.ProductIds,
                filter?.EmployeeId,
                filter?.CustomerId,
                filter?.Unit,
                filter?.Category,
                filter?.FromDate,
                filter?.ToDate,
                ct);

            Items.Clear();
            foreach (var dto in items)
                Items.Add(SalesOrderSummaryLineItem.FromDto(dto));

            HasLines = Items.Count > 0;
            RecalculateTotals();
            RebuildDisplayRows();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    private void RecalculateTotals()
    {
        TotalQuantitySold   = Items.Sum(i => i.QuantitySold);
        TotalSalesAmount    = Items.Sum(i => i.SalesAmount);
        TotalDiscountAmount = Items.Sum(i => i.DiscountAmount);
        TotalReturnQuantity = Items.Sum(i => i.ReturnQuantity);
        TotalReturnValue    = Items.Sum(i => i.ReturnValue);
        TotalNetRevenue     = Items.Sum(i => i.NetRevenue);
    }

    private static string ColumnLabelFor(SummaryDimension d) => d switch
    {
        SummaryDimension.Product  => "Tên hàng",
        SummaryDimension.Customer => "Khách hàng",
        SummaryDimension.Employee => "Nhân viên",
        _ => "",
    };

    private void RebuildDisplayRows()
    {
        var reportType = CurrentFilter?.ReportType ?? SalesOrderReportTypes.ByProduct;
        if (!GroupingsByType.TryGetValue(reportType, out var dimensions))
            dimensions = GroupingsByType[SalesOrderReportTypes.ByProduct];

        // Leaf rows are grouped by ALL active dimensions combined (2 dims → one row per pair,
        // 1 dim → one row per value) — every row shown is a full aggregate, never a raw line.
        var leafGroups = dimensions.Length == 1
            ? Items.GroupBy(dimensions[0].Key)
            : Items.GroupBy(i => dimensions[0].Key(i) + "␟" + dimensions[1].Key(i));

        var isNested   = dimensions.Length == 2;
        var innerField = dimensions[^1].Field;
        var allFields  = dimensions.Select(d => d.Field).ToHashSet();
        var showUnit   = allFields.Contains(SummaryDimension.Product);

        IsUnitColumnVisible = showUnit;

        var rows = new List<ReportDisplayRow>();
        foreach (var group in leafGroups)
        {
            var groupItems = group.ToList();
            var row = ReportDisplayRow.Aggregate(groupItems, innerField, showUnit);
            if (isNested)
            {
                row.GroupKey   = dimensions[0].Key(groupItems[0]);
                row.GroupLabel = ColumnLabelFor(dimensions[0].Field);
            }
            rows.Add(row);
        }

        // Aggregate() always writes the row's own identity into ProductName, regardless of which
        // dimension it actually represents — so sorting by ProductName works uniformly here.
        var orderedRows = dimensions.Length == 2
            ? rows.OrderBy(r => r.GroupKey, StringComparer.CurrentCultureIgnoreCase)
                  .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                  .ToList()
            : rows.OrderBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                  .ToList();

        _displayRows = orderedRows;

        var view = new ListCollectionView(_displayRows);
        if (dimensions.Length == 2)
        {
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ReportDisplayRow.GroupKey)));
            view.SortDescriptions.Add(new SortDescription(nameof(ReportDisplayRow.GroupKey), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(ReportDisplayRow.ProductName), ListSortDirection.Ascending));
        }
        else
        {
            view.SortDescriptions.Add(new SortDescription(nameof(ReportDisplayRow.ProductName), ListSortDirection.Ascending));
        }

        RowsView  = view;
        RowCount  = _displayRows.Count;
    }

    private List<ReportDisplayRow> BuildExportRows()
    {
        var result = new List<ReportDisplayRow>();
        string? lastKey = null;
        foreach (var row in _displayRows)
        {
            if (row.GroupKey is not null && row.GroupKey != lastKey)
            {
                var groupRows = _displayRows.Where(r => r.GroupKey == row.GroupKey).ToList();
                result.Add(new ReportDisplayRow
                {
                    IsGroupHeader  = true,
                    ProductName    = $"{row.GroupLabel} : {row.GroupKey} ({groupRows.Count})",
                    QuantitySold   = groupRows.Sum(r => r.QuantitySold),
                    SalesAmount    = groupRows.Sum(r => r.SalesAmount),
                    DiscountAmount = groupRows.Sum(r => r.DiscountAmount),
                    ReturnQuantity = groupRows.Sum(r => r.ReturnQuantity),
                    ReturnValue    = groupRows.Sum(r => r.ReturnValue),
                    NetRevenue     = groupRows.Sum(r => r.NetRevenue),
                });
                lastKey = row.GroupKey;
            }
            result.Add(row);
        }
        return result;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void ChooseParameters()
    {
        var window = _reportFilterWindowFactory();
        if (window.ShowDialog() == true)
        {
            CurrentFilter = window.BuildFilter();
            _ = LoadAsync();
        }
    }

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
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, "Báo cáo bán hàng");
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"TongHopBanHang_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet       = workbook.Worksheets.Add("Báo cáo");

            string[] headers =
            {
                "Mã hàng", "Tên hàng", "ĐVT",
                "Số lượng bán", "Doanh số bán", "Chiết khấu",
                "Số lượng trả lại", "Giá trị trả lại", "Giá trị giảm giá", "Doanh thu thuần",
            };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var line in BuildExportRows())
            {
                worksheet.Cell(row, 1).Value = line.ProductCode;
                worksheet.Cell(row, 2).Value = line.ProductName;
                worksheet.Cell(row, 3).Value = line.Unit;
                worksheet.Cell(row, 4).Value = line.QuantitySold;
                worksheet.Cell(row, 5).Value = line.SalesAmount;
                worksheet.Cell(row, 6).Value = line.DiscountAmount;
                worksheet.Cell(row, 7).Value = line.ReturnQuantity;
                worksheet.Cell(row, 8).Value = line.ReturnValue;
                worksheet.Cell(row, 9).Value = line.DiscountValue;
                worksheet.Cell(row, 10).Value = line.NetRevenue;
                if (line.IsGroupHeader)
                    worksheet.Range(row, 1, row, 10).Style.Font.Bold = true;
                row++;
            }

            worksheet.Cell(row, 2).Value           = "Tổng cộng";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 4).Value            = TotalQuantitySold;
            worksheet.Cell(row, 4).Style.Font.Bold  = true;
            worksheet.Cell(row, 5).Value             = TotalSalesAmount;
            worksheet.Cell(row, 5).Style.Font.Bold   = true;
            worksheet.Cell(row, 6).Value             = TotalDiscountAmount;
            worksheet.Cell(row, 6).Style.Font.Bold   = true;
            worksheet.Cell(row, 7).Value              = TotalReturnQuantity;
            worksheet.Cell(row, 7).Style.Font.Bold    = true;
            worksheet.Cell(row, 8).Value              = TotalReturnValue;
            worksheet.Cell(row, 8).Style.Font.Bold    = true;
            worksheet.Cell(row, 10).Value              = TotalNetRevenue;
            worksheet.Cell(row, 10).Style.Font.Bold    = true;

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

    private FlowDocument BuildReportDocument()
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 11,
            PagePadding = new Thickness(20),
        };

        doc.Blocks.Add(new Paragraph(new Bold(new Run(ReportTitle)) { FontSize = 18 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 4),
        });

        doc.Blocks.Add(new Paragraph(new Run(CurrentFilter?.Summary ?? "Tất cả chứng từ"))
        {
            TextAlignment = TextAlignment.Center,
            FontSize      = 12,
            Margin        = new Thickness(0, 0, 0, 12),
        });

        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 4, 0, 4) };
        foreach (var width in new[] { 70, 150, 50, 70, 90, 80, 70, 80, 80, 90 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow(
            "Mã hàng", "Tên hàng", "ĐVT",
            "SL bán", "Doanh số bán", "Chiết khấu",
            "SL trả lại", "GT trả lại", "GT giảm giá", "Doanh thu thuần"));

        foreach (var line in BuildExportRows())
        {
            rowGroup.Rows.Add(DataRow(
                line.IsGroupHeader,
                line.ProductCode,
                line.ProductName,
                line.Unit,
                line.QuantitySold.ToString(),
                FormatMoney(line.SalesAmount),
                FormatMoney(line.DiscountAmount),
                line.ReturnQuantity.ToString(),
                FormatMoney(line.ReturnValue),
                FormatMoney(line.DiscountValue),
                FormatMoney(line.NetRevenue)));
        }

        rowGroup.Rows.Add(TotalsRow());
        table.RowGroups.Add(rowGroup);
        doc.Blocks.Add(table);

        return doc;
    }

    private TableRow TotalsRow()
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        row.Cells.Add(BoldCell("Tổng cộng", 3));
        row.Cells.Add(BoldCell(TotalQuantitySold.ToString()));
        row.Cells.Add(BoldCell(FormatMoney(TotalSalesAmount)));
        row.Cells.Add(BoldCell(FormatMoney(TotalDiscountAmount)));
        row.Cells.Add(BoldCell(TotalReturnQuantity.ToString()));
        row.Cells.Add(BoldCell(FormatMoney(TotalReturnValue)));
        row.Cells.Add(BoldCell(string.Empty));
        row.Cells.Add(BoldCell(FormatMoney(TotalNetRevenue)));
        return row;
    }

    private static TableCell BoldCell(string text, int columnSpan = 1)
    {
        var cell = new TableCell(new Paragraph(new Bold(new Run(text))) { TextAlignment = TextAlignment.Center })
        {
            Padding         = new Thickness(4),
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            ColumnSpan      = columnSpan,
        };
        return cell;
    }

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        foreach (var h in headers)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(h))) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(4),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static TableRow DataRow(bool bold, params string[] values)
    {
        var row = new TableRow { Background = bold ? Brushes.WhiteSmoke : Brushes.Transparent };
        foreach (var v in values)
        {
            Inline content = bold ? new Bold(new Run(v)) : new Run(v);
            row.Cells.Add(new TableCell(new Paragraph(content) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(4),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
}
