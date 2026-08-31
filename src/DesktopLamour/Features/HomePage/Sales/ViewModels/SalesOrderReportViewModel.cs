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
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
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
    [ObservableProperty] private decimal _totalCostAmount;
    [ObservableProperty] private decimal _totalGrossProfit;

    [ObservableProperty] private bool _isUnitColumnVisible;

    // "Tên nhóm KH" chỉ có ý nghĩa khi report gồm dimension Khách hàng — ẩn hẳn ở các report
    // không liên quan tới khách hàng (vd. "Mặt hàng" đơn thuần) thay vì hiện cột luôn rỗng.
    [ObservableProperty] private bool _isCustomerGroupColumnVisible;

    // Cột "Mã hàng"/"Tên hàng" trong XAML là header tĩnh nhưng dữ liệu thực chất là danh tính của
    // dimension TRONG (inner) của report type đang chọn — trước đây luôn hard-code "Mã hàng"/"Tên
    // hàng" dù đang hiển thị Mã/Tên nhân viên hoặc khách hàng. 2 property này được code-behind
    // (SalesOrderReportView.xaml.cs) gán trực tiếp vào Header của 2 cột đó mỗi khi đổi report type.
    [ObservableProperty] private string _innerCodeLabel = "Mã hàng";
    [ObservableProperty] private string _innerNameLabel = "Tên hàng";

    // Report 2 chiều hiện dạng bảng phẳng kiểu MISA — dimension NGOÀI (vd. Nhân viên trong "Nhân
    // viên & khách hàng") cũng là 2 cột thật trên MỌI dòng, không co gọn theo Expander/group-header
    // nữa. Rỗng + ẩn cột khi report chỉ có 1 dimension.
    [ObservableProperty] private string _outerCodeLabel = "";
    [ObservableProperty] private string _outerNameLabel = "";
    [ObservableProperty] private bool   _isOuterColumnVisible;

    // ── Filter theo từng cột — nhúng ngay trong header lưới (khớp UI MISA), cùng pattern đã dùng
    // ở SalesOrderReportDetailView/WarehouseTransactionListView: cột text dùng Contains-match string,
    // cột số dùng NumericColumnFilter (operator =, ≤, ≥... + giá trị). AND tất cả với nhau.
    [ObservableProperty] private string _filterOuterCode         = string.Empty;
    [ObservableProperty] private string _filterOuterName         = string.Empty;
    [ObservableProperty] private string _filterInnerCode         = string.Empty;
    [ObservableProperty] private string _filterInnerName         = string.Empty;
    [ObservableProperty] private string _filterUnit              = string.Empty;
    [ObservableProperty] private string _filterCustomerGroupName = string.Empty;

    partial void OnFilterOuterCodeChanged(string value)         => RowsView.Refresh();
    partial void OnFilterOuterNameChanged(string value)         => RowsView.Refresh();
    partial void OnFilterInnerCodeChanged(string value)         => RowsView.Refresh();
    partial void OnFilterInnerNameChanged(string value)         => RowsView.Refresh();
    partial void OnFilterUnitChanged(string value)              => RowsView.Refresh();
    partial void OnFilterCustomerGroupNameChanged(string value) => RowsView.Refresh();

    public NumericColumnFilter QuantitySoldFilter    { get; } = new();
    public NumericColumnFilter SalesAmountFilter     { get; } = new();
    public NumericColumnFilter DiscountAmountFilter  { get; } = new();
    public NumericColumnFilter ReturnQuantityFilter  { get; } = new();
    public NumericColumnFilter ReturnValueFilter     { get; } = new();
    public NumericColumnFilter DiscountValueFilter   { get; } = new();
    public NumericColumnFilter NetRevenueFilter      { get; } = new();
    public NumericColumnFilter CostAmountFilter      { get; } = new();
    public NumericColumnFilter GrossProfitFilter     { get; } = new();
    public NumericColumnFilter GrossProfitRateFilter { get; } = new();

    private bool MatchesAllFilters(object obj)
    {
        if (obj is not ReportDisplayRow row) return false;

        return Matches(FilterOuterCode, row.OuterCode)
            && Matches(FilterOuterName, row.OuterName)
            && Matches(FilterInnerCode, row.ProductCode)
            && Matches(FilterInnerName, row.ProductName)
            && Matches(FilterUnit, row.Unit)
            && QuantitySoldFilter.Matches(row.QuantitySold)
            && SalesAmountFilter.Matches(row.SalesAmount)
            && DiscountAmountFilter.Matches(row.DiscountAmount)
            && ReturnQuantityFilter.Matches(row.ReturnQuantity)
            && ReturnValueFilter.Matches(row.ReturnValue)
            && DiscountValueFilter.Matches(row.DiscountValue)
            && NetRevenueFilter.Matches(row.NetRevenue)
            && CostAmountFilter.Matches(row.CostAmount)
            && GrossProfitFilter.Matches(row.GrossProfit)
            && GrossProfitRateFilter.Matches(row.GrossProfitRate)
            && Matches(FilterCustomerGroupName, row.CustomerGroupName);
    }

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || (!string.IsNullOrEmpty(cellText) && cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase));

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
        [SalesOrderReportTypes.ByEmployeeThenCustomer] =
            new[]
            {
                (Field: SummaryDimension.Employee, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.EmployeeName), Label: "Nhân viên"),
                (Field: SummaryDimension.Customer, Key: (Func<SalesOrderSummaryLineItem, string>)(i => i.CustomerName), Label: "Khách hàng"),
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

        QuantitySoldFilter.Changed    = () => RowsView.Refresh();
        SalesAmountFilter.Changed     = () => RowsView.Refresh();
        DiscountAmountFilter.Changed  = () => RowsView.Refresh();
        ReturnQuantityFilter.Changed  = () => RowsView.Refresh();
        ReturnValueFilter.Changed     = () => RowsView.Refresh();
        DiscountValueFilter.Changed   = () => RowsView.Refresh();
        NetRevenueFilter.Changed      = () => RowsView.Refresh();
        CostAmountFilter.Changed      = () => RowsView.Refresh();
        GrossProfitFilter.Changed     = () => RowsView.Refresh();
        GrossProfitRateFilter.Changed = () => RowsView.Refresh();
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
            // Tổng footer (TotalSalesAmount...) giờ tính lại trong RebuildDisplayRows/
            // UpdateFilteredTotals — theo đúng tập dòng ĐANG HIỂN THỊ sau lọc, không phải toàn bộ
            // Items như RecalculateTotals() cũ.
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

    private static string ColumnLabelFor(SummaryDimension d) => d switch
    {
        SummaryDimension.Product  => "Tên hàng",
        SummaryDimension.Customer => "Khách hàng",
        SummaryDimension.Employee => "Nhân viên",
        _ => "",
    };

    private static (string Code, string Name) CodeNameLabelFor(SummaryDimension d) => d switch
    {
        SummaryDimension.Product  => ("Mã hàng", "Tên hàng"),
        SummaryDimension.Customer => ("Mã khách hàng", "Tên khách hàng"),
        SummaryDimension.Employee => ("Mã nhân viên", "Tên nhân viên"),
        _ => ("Mã", "Tên"),
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

        IsUnitColumnVisible          = showUnit;
        IsCustomerGroupColumnVisible = allFields.Contains(SummaryDimension.Customer);
        (InnerCodeLabel, InnerNameLabel) = CodeNameLabelFor(innerField);

        IsOuterColumnVisible = isNested;
        (OuterCodeLabel, OuterNameLabel) = isNested ? CodeNameLabelFor(dimensions[0].Field) : ("", "");

        var rows = new List<ReportDisplayRow>();
        foreach (var group in leafGroups)
        {
            var groupItems = group.ToList();
            var row = ReportDisplayRow.Aggregate(groupItems, innerField, showUnit);
            if (isNested)
            {
                row.GroupKey   = dimensions[0].Key(groupItems[0]);
                row.GroupLabel = ColumnLabelFor(dimensions[0].Field);
                row.SetOuterId(dimensions[0].Field, groupItems[0]);
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

        // Bảng phẳng kiểu MISA — không group/collapse theo Expander nữa (row đã tự mang đủ cả 2
        // dimension làm cột thật khi isNested), chỉ cần sort đúng thứ tự Outer rồi Inner.
        var view = new ListCollectionView(_displayRows);
        if (dimensions.Length == 2)
        {
            view.SortDescriptions.Add(new SortDescription(nameof(ReportDisplayRow.GroupKey), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(ReportDisplayRow.ProductName), ListSortDirection.Ascending));
        }
        else
        {
            view.SortDescriptions.Add(new SortDescription(nameof(ReportDisplayRow.ProductName), ListSortDirection.Ascending));
        }

        // Filter theo cột (MatchesAllFilters) áp ngay khi gán — RowCount/tổng footer phải đếm
        // đúng số dòng ĐANG HIỂN THỊ sau lọc, không phải toàn bộ _displayRows. CollectionChanged
        // tự bắn Reset sau mỗi Refresh() nên chỉ cần gắn 1 lần cho view mới này.
        view.Filter = MatchesAllFilters;
        // CollectionChanged là explicit interface implementation của ICollectionView trên
        // CollectionView (không public qua kiểu cụ thể ListCollectionView) — phải ép kiểu về
        // ICollectionView mới gắn handler được, khớp cách CustomerListViewModel đã làm.
        ((ICollectionView)view).CollectionChanged += (_, _) => UpdateFilteredTotals(view);

        RowsView = view;
        UpdateFilteredTotals(view);
    }

    private void UpdateFilteredTotals(ICollectionView view)
    {
        var rows = view.Cast<ReportDisplayRow>().ToList();
        RowCount            = rows.Count;
        TotalQuantitySold   = rows.Sum(r => r.QuantitySold);
        TotalSalesAmount    = rows.Sum(r => r.SalesAmount);
        TotalDiscountAmount = rows.Sum(r => r.DiscountAmount);
        TotalReturnQuantity = rows.Sum(r => r.ReturnQuantity);
        TotalReturnValue    = rows.Sum(r => r.ReturnValue);
        TotalNetRevenue     = rows.Sum(r => r.NetRevenue);
        TotalCostAmount     = rows.Sum(r => r.CostAmount);
        TotalGrossProfit    = TotalNetRevenue - TotalCostAmount;
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
    private void DismissError() => HasError = false;

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

    // Double-click a summary row → drill down into the raw transactions behind it,
    // narrowed to whichever dimension(s) that row represents (product/customer/employee).
    [RelayCommand]
    private void DrillDown(ReportDisplayRow? row)
    {
        if (row is null) return;

        var reportType = CurrentFilter?.ReportType ?? SalesOrderReportTypes.ByProduct;
        if (!GroupingsByType.TryGetValue(reportType, out var dimensions))
            dimensions = GroupingsByType[SalesOrderReportTypes.ByProduct];

        var innerLabel = ColumnLabelFor(dimensions[^1].Field);
        var title = row.GroupLabel is not null
            ? $"{row.GroupLabel}: {row.GroupKey}  —  {innerLabel}: {row.ProductName}"
            : $"{innerLabel}: {row.ProductName}";

        var filter = CurrentFilter;
        var detailFilter = new SalesOrderDetailFilter
        {
            Title      = title,
            ProductId  = row.ProductId,
            CustomerId = row.CustomerId,
            EmployeeId = row.EmployeeId,
            Unit       = filter?.Unit,
            Category   = filter?.Category,
            FromDate   = filter?.FromDate,
            ToDate     = filter?.ToDate,
        };

        _navigationService.NavigateTo(NavigationRoutes.SalesOrders.ReportDetail, detailFilter);
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
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, "TongHopBanHang");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenMailClient(
                ReportTitle,
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
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, "TongHopBanHang");
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
        var worksheet = workbook.Worksheets.Add("Báo cáo");

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
        return workbook;
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
