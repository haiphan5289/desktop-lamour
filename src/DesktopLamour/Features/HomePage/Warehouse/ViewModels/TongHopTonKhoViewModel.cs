// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

// Khớp MISA (2026-10-08): mở màn là hiện ngay hộp "Chọn tham số" (kỳ báo cáo, từ/đến, ĐVT, nhóm VTHH,
// tick kho HH/TB) → Đồng ý mới ra báo cáo; nút "Chọn tham số..." mở lại hộp. Báo cáo chia theo từng
// kho: mỗi kho là 1 dòng tổng "Tên kho : Hàng Hóa (66)" rồi tới các mặt hàng với số liệu riêng kho đó.
// (Trước đó bộ lọc nằm sẵn trên màn hình sau khi popup cũ TongHopTonKhoFilterWindow bị gỡ.)
public partial class TongHopTonKhoViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetInventorySummaryByWarehouseUseCase _getSummary;
    private readonly Func<TongHopTonKhoParamsWindow> _paramsWindowFactory;
    private readonly IGetWarehouseSettingsUseCase _getWarehouses;
    private readonly IGetCategoriesUseCase        _getCategories;
    private readonly IGetProductUnitsUseCase      _getProductUnits;
    private readonly IGetProductsUseCase          _getProducts;
    private readonly ILogger<TongHopTonKhoViewModel> _logger;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;

    // Dòng phụ dưới tiêu đề báo cáo: "Từ ngày … đến ngày …" của lần nạp gần nhất.
    [ObservableProperty] private string _reportSubtitle = string.Empty;

    // Chân bảng: chỉ cộng các dòng mặt hàng (không cộng dòng tổng nhóm kho để khỏi tính đôi).
    [ObservableProperty] private int     _rowCount;
    [ObservableProperty] private decimal _totalOpeningQty;
    [ObservableProperty] private decimal _totalImportQty;
    [ObservableProperty] private decimal _totalExportQty;
    [ObservableProperty] private decimal _totalClosingQty;

    [ObservableProperty] private string           _periodPreset = "Tháng này";
    [ObservableProperty] private DateTime          _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime          _toDate   = DateTime.Today;
    [ObservableProperty] private ISearchableItem?  _selectedCategory;
    [ObservableProperty] private ISearchableItem?  _selectedProductUnit;

    public IReadOnlyList<string> PeriodPresetOptions { get; } = new[] { "Tháng này", "Quý này", "Năm này", "Tùy chọn" };
    public IReadOnlyList<ISearchableItem> Categories   { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> ProductUnits { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<WarehouseCheckItem> WarehouseItems { get; } = new();
    public ObservableCollection<ProductCheckItem>   ProductItems   { get; } = new();

    // Dòng đang hiển thị = các nhóm kho đã nạp, sau khi áp hàng ô lọc theo cột (Filters).
    public ObservableCollection<InventorySummaryItem> Items { get; } = new();

    // Hàng ô lọc dưới tiêu đề cột (khớp MISA) — lọc ngay trên dữ liệu đã nạp, không gọi lại BE. Cùng
    // mẫu với InventoryDetailViewModel/SalesOrderReportDetailViewModel (Shared/Models/ColumnFilterModels.cs):
    // cột chữ lọc "chứa chuỗi", cột số có toán tử (mặc định ≤) + giá trị.
    [ObservableProperty] private string _filterCode = string.Empty;
    [ObservableProperty] private string _filterName = string.Empty;
    [ObservableProperty] private string _filterUnit = string.Empty;
    [ObservableProperty] private string _filterDate = string.Empty;

    partial void OnFilterCodeChanged(string value) => ApplyFilters();
    partial void OnFilterNameChanged(string value) => ApplyFilters();
    partial void OnFilterUnitChanged(string value) => ApplyFilters();
    partial void OnFilterDateChanged(string value) => ApplyFilters();

    public NumericColumnFilter OpeningQtyFilter   { get; } = new();
    public NumericColumnFilter OpeningValueFilter { get; } = new();
    public NumericColumnFilter ImportQtyFilter    { get; } = new();
    public NumericColumnFilter ImportValueFilter  { get; } = new();
    public NumericColumnFilter ExportQtyFilter    { get; } = new();
    public NumericColumnFilter ExportValueFilter  { get; } = new();
    public NumericColumnFilter ClosingQtyFilter   { get; } = new();
    public NumericColumnFilter ClosingValueFilter { get; } = new();

    private const string FilePrefix = "TongHopTonKho";
    private List<InventoryWarehouseGroup> _groups = new();

    public string ReportTitle => "TỔNG HỢP TỒN KHO";

    public TongHopTonKhoViewModel(
        INavigationService              navigationService,
        IGetInventorySummaryByWarehouseUseCase getSummary,
        Func<TongHopTonKhoParamsWindow> paramsWindowFactory,
        IGetWarehouseSettingsUseCase    getWarehouses,
        IGetCategoriesUseCase           getCategories,
        IGetProductUnitsUseCase         getProductUnits,
        IGetProductsUseCase             getProducts,
        ILogger<TongHopTonKhoViewModel> logger)
    {
        _navigationService = navigationService;
        _getSummary        = getSummary;
        _paramsWindowFactory = paramsWindowFactory;
        _getWarehouses     = getWarehouses;
        _getCategories     = getCategories;
        _getProductUnits   = getProductUnits;
        _getProducts       = getProducts;
        _logger            = logger;
        foreach (var filter in new[]
                 {
                     OpeningQtyFilter, OpeningValueFilter, ImportQtyFilter, ImportValueFilter,
                     ExportQtyFilter, ExportValueFilter, ClosingQtyFilter, ClosingValueFilter,
                 })
            filter.Changed = ApplyFilters;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    // Double-click 1 dòng sản phẩm → "Sổ chi tiết vật tư hàng hóa" cho riêng sản phẩm đó, kế thừa
    // đúng khoảng ngày/kho đang lọc ở màn này.
    [RelayCommand]
    private void DrillDown(InventorySummaryItem? item)
    {
        if (item is null) return;

        if (item.IsGroupHeader) return; // dòng tổng "Tên kho : …" không phải mặt hàng

        // Sổ chi tiết mở đúng kho của dòng đang chọn (báo cáo đã chia theo kho) — như MISA
        // ("Kho: Hàng Hóa; Mặt hàng: Meso Fills").
        var filter = new InventoryDetailFilter
        {
            ProductId      = item.ProductId,
            ProductLabel   = $"{item.Code} — {item.Name}",
            FromDate       = FromDate,
            ToDate         = ToDate,
            WarehouseIds   = new List<int> { item.WarehouseId },
            WarehouseLabel = item.WarehouseName,
        };

        _navigationService.NavigateTo(NavigationRoutes.Warehouse.InventoryDetail, filter);
    }

    // Preset chỉ tính lại From/To client-side — không tự gọi BE, người dùng vẫn bấm "Lọc".
    partial void OnPeriodPresetChanged(string value)
    {
        var today = DateTime.Today;
        switch (value)
        {
            case "Tháng này":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate   = today;
                break;
            case "Quý này":
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                FromDate = new DateTime(today.Year, quarterStartMonth, 1);
                ToDate   = today;
                break;
            case "Năm này":
                FromDate = new DateTime(today.Year, 1, 1);
                ToDate   = today;
                break;
            case "Tùy chọn":
            default:
                break;
        }
    }

    [RelayCommand]
    private void ClearConditions()
    {
        PeriodPreset         = "Tháng này";
        SelectedCategory     = null;
        SelectedProductUnit  = null;
        foreach (var w in WarehouseItems) w.IsSelected = false;
        foreach (var p in ProductItems)   p.IsSelected = false;
        OnPropertyChanged(nameof(AreAllWarehousesSelected));
    }

    // Ô tick ở đầu cột trong danh sách kho của hộp tham số: tick/bỏ tick tất cả kho.
    public bool AreAllWarehousesSelected
    {
        get => WarehouseItems.Count > 0 && WarehouseItems.All(w => w.IsSelected);
        set
        {
            foreach (var w in WarehouseItems) w.IsSelected = value;
            OnPropertyChanged();
        }
    }

    private void OnWarehouseItemChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WarehouseCheckItem.IsSelected))
            OnPropertyChanged(nameof(AreAllWarehousesSelected));
    }

    // ── Hộp "Chọn tham số" ──────────────────────────────────────────────────
    private sealed record ParamsSnapshot(
        string Preset, DateTime From, DateTime To, ISearchableItem? Category, ISearchableItem? Unit, IReadOnlyList<int> WarehouseIds);

    private ParamsSnapshot CaptureParams() => new(
        PeriodPreset, FromDate, ToDate, SelectedCategory, SelectedProductUnit,
        WarehouseItems.Where(w => w.IsSelected).Select(w => w.Id).ToList());

    // Hủy bỏ: trả mọi tham số về đúng lúc mở hộp (đặt Preset trước vì đổi Preset tự tính lại Từ/Đến).
    private void RestoreParams(ParamsSnapshot snap)
    {
        PeriodPreset        = snap.Preset;
        FromDate            = snap.From;
        ToDate              = snap.To;
        SelectedCategory    = snap.Category;
        SelectedProductUnit = snap.Unit;
        foreach (var w in WarehouseItems) w.IsSelected = snap.WarehouseIds.Contains(w.Id);
        OnPropertyChanged(nameof(AreAllWarehousesSelected));
    }

    // true = bấm Đồng ý (tham số đã giữ lại), false = Hủy bỏ/đóng hộp (tham số trả về như cũ).
    private bool ShowParamsDialog()
    {
        var snapshot = CaptureParams();
        var window   = _paramsWindowFactory();
        window.DataContext = this;
        window.Owner       = System.Windows.Application.Current.MainWindow;
        if (window.ShowDialog() == true) return true;

        RestoreParams(snapshot);
        return false;
    }

    // Nút "Chọn tham số..." trên báo cáo: mở lại hộp, Đồng ý thì nạp lại báo cáo.
    [RelayCommand]
    private async Task ChooseParamsAsync(CancellationToken ct = default)
    {
        if (ShowParamsDialog())
            await LoadAsync(ct);
    }

    // Gọi 1 lần khi mở màn (Loaded): nạp danh mục → hộp tham số hiện ngay → Đồng ý mới ra báo cáo;
    // Hủy bỏ ở lần mở đầu thì quay lại màn trước (như MISA đóng hộp là không vào báo cáo).
    [RelayCommand]
    private async Task StartAsync(CancellationToken ct = default)
    {
        await InitializeFiltersAsync(ct);
        if (ShowParamsDialog())
            await LoadAsync(ct);
        else
            _navigationService.GoBack();
    }

    // Tải danh mục kho/nhóm VTHH/đơn vị tính cho các dropdown lọc — gọi 1 lần khi màn hình mở,
    // trước lần LoadAsync đầu tiên.
    [RelayCommand]
    private async Task InitializeFiltersAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            var warehouseTask = _getWarehouses.ExecuteAsync(ct);
            var categoryTask  = _getCategories.ExecuteAsync(ct);
            var unitTask      = _getProductUnits.ExecuteAsync(ct);
            var productTask   = _getProducts.ExecuteAsync(ct);
            await Task.WhenAll(warehouseTask, categoryTask, unitTask, productTask);

            Categories = categoryTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Categories));
            ProductUnits = unitTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(ProductUnits));

            foreach (var old in WarehouseItems) old.PropertyChanged -= OnWarehouseItemChanged;
            WarehouseItems.Clear();
            // Chỉ liệt kê kho đang hoạt động — kho đã ngưng không được lên hộp tham số/báo cáo.
            foreach (var w in warehouseTask.Result.Where(w => w.IsActive).Cast<ISearchableItem>())
            {
                // Mặc định tick tất cả kho (HH, TB) như hộp tham số MISA — bỏ tick bớt kho để lọc.
                var item = new WarehouseCheckItem(w) { IsSelected = true };
                item.PropertyChanged += OnWarehouseItemChanged;
                WarehouseItems.Add(item);
            }
            OnPropertyChanged(nameof(AreAllWarehousesSelected));

            ProductItems.Clear();
            foreach (var p in productTask.Result.Cast<ISearchableItem>().OrderBy(p => p.Code))
                ProductItems.Add(new ProductCheckItem(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load filter lookups for TongHopTonKho");
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var from          = DateOnly.FromDateTime(FromDate);
            var to            = DateOnly.FromDateTime(ToDate);
            var warehouseIds  = WarehouseItems.Where(w => w.IsSelected).Select(w => w.Id).ToList();
            var productIds    = ProductItems.Where(p => p.IsSelected).Select(p => p.Id).ToList();
            _groups = (await _getSummary.ExecuteAsync(
                from, to, warehouseIds, SelectedCategory?.Id, SelectedProductUnit?.Id, productIds, ct)).ToList();

            ReportSubtitle = $"Từ ngày {FromDate:dd/MM/yyyy} đến ngày {ToDate:dd/MM/yyyy}";
            HasItems       = _groups.Count > 0;
            ApplyFilters();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // ── Hàng lọc theo cột ─────────────────────────────────────────────────────────────────────────
    // Mỗi kho: 1 dòng tổng (tiêu đề nhóm, số liệu = tổng các mặt hàng CÒN HIỂN THỊ của kho) rồi tới các
    // mặt hàng khớp bộ lọc; kho không còn mặt hàng nào khớp thì ẩn luôn dòng tổng.
    private void ApplyFilters()
    {
        Items.Clear();
        foreach (var g in _groups)
        {
            var visible = g.Items.Where(MatchesFilters).ToList();
            if (visible.Count == 0) continue;

            Items.Add(new InventorySummaryItem
            {
                IsGroupHeader = true,
                WarehouseId   = g.WarehouseId,
                WarehouseCode = g.WarehouseCode,
                WarehouseName = g.WarehouseName,
                Name          = $"Tên kho : {g.WarehouseName} ({visible.Count})",
                OpeningQty    = visible.Sum(i => i.OpeningQty),
                OpeningValue  = visible.Sum(i => i.OpeningValue),
                ImportQty     = visible.Sum(i => i.ImportQty),
                ImportValue   = visible.Sum(i => i.ImportValue),
                ExportQty     = visible.Sum(i => i.ExportQty),
                ExportValue   = visible.Sum(i => i.ExportValue),
                ClosingQty    = visible.Sum(i => i.ClosingQty),
                ClosingValue  = visible.Sum(i => i.ClosingValue),
            });
            foreach (var item in visible) Items.Add(item);
        }

        // Chân bảng: chỉ cộng các dòng mặt hàng (không cộng dòng tổng nhóm kho để khỏi tính đôi).
        var dataRows    = Items.Where(i => !i.IsGroupHeader).ToList();
        RowCount        = dataRows.Count;
        TotalOpeningQty = dataRows.Sum(i => i.OpeningQty);
        TotalImportQty  = dataRows.Sum(i => i.ImportQty);
        TotalExportQty  = dataRows.Sum(i => i.ExportQty);
        TotalClosingQty = dataRows.Sum(i => i.ClosingQty);
    }

    private bool MatchesFilters(InventorySummaryItem i) =>
        TextMatches(i.Code, FilterCode)
        && TextMatches(i.Name, FilterName)
        && TextMatches(i.Unit, FilterUnit)
        && TextMatches(DateText(i), FilterDate)
        && OpeningQtyFilter.Matches(i.OpeningQty)
        && OpeningValueFilter.Matches(i.OpeningValue)
        && ImportQtyFilter.Matches(i.ImportQty)
        && ImportValueFilter.Matches(i.ImportValue)
        && ExportQtyFilter.Matches(i.ExportQty)
        && ExportValueFilter.Matches(i.ExportValue)
        && ClosingQtyFilter.Matches(i.ClosingQty)
        && ClosingValueFilter.Matches(i.ClosingValue);

    // Cùng định dạng cột "Ngày HT" trên lưới (yyyyMMddHHmm).
    private static string DateText(InventorySummaryItem i) =>
        i.LatestAccountingDate?.ToString("yyyyMMddHHmm") ?? string.Empty;

    private static bool TextMatches(string? text, string filter) =>
        string.IsNullOrWhiteSpace(filter)
        || (text ?? string.Empty).Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    // ── Xuất khẩu / Gửi / In ─ làm trên đúng các dòng đang hiển thị (đã áp bộ lọc), như MISA ───────
    private bool EnsureHasData()
    {
        if (HasItems) return true;
        MessageBox.Show("Chưa có dữ liệu báo cáo. Vui lòng bấm \"Chọn tham số...\" để xem báo cáo trước.",
            "Tổng hợp tồn kho", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    [RelayCommand]
    private void ExportExcel()
    {
        if (!EnsureHasData()) return;
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"{FilePrefix}_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = BuildWorkbook();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void SendEmail()
    {
        if (!EnsureHasData()) return;
        try
        {
            using var workbook = BuildWorkbook();
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, FilePrefix);
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
        if (!EnsureHasData()) return;
        try
        {
            using var workbook = BuildWorkbook();
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, FilePrefix);
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

    [RelayCommand]
    private void Print()
    {
        if (!EnsureHasData()) return;

        var printDialog = new System.Windows.Controls.PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        var document = BuildReportDocument();
        document.PageHeight  = printDialog.PrintableAreaHeight;
        document.PageWidth   = printDialog.PrintableAreaWidth;
        document.PagePadding = new Thickness(30);
        document.ColumnWidth = printDialog.PrintableAreaWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, "Tổng hợp tồn kho");
    }

    // 12 cột (A–L) giống lưới: Mã hàng, Tên hàng, ĐVT, 4 nhóm × (Số lượng, Giá trị), Ngày HT.
    private const int ExcelLastColumn = 12;

    private ClosedXML.Excel.XLWorkbook BuildWorkbook()
    {
        var workbook  = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Tổng hợp tồn kho");
        var blue      = ClosedXML.Excel.XLColor.FromHtml("#BDD7EE");
        var center    = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

        worksheet.Range(1, 1, 1, ExcelLastColumn).Merge();
        worksheet.Cell(1, 1).Value = ReportTitle;
        worksheet.Cell(1, 1).Style.Font.Bold     = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = center;

        worksheet.Range(2, 1, 2, ExcelLastColumn).Merge();
        worksheet.Cell(2, 1).Value = ReportSubtitle;
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Cell(2, 1).Style.Alignment.Horizontal = center;

        // Tiêu đề 2 tầng: Mã hàng/Tên hàng/ĐVT/Ngày HT gộp dọc, mỗi nhóm số liệu gộp ngang 2 ô.
        const int headerRow = 4;
        void Merged(int row1, int col1, int row2, int col2, string text)
        {
            var range = worksheet.Range(row1, col1, row2, col2);
            if (row1 != row2 || col1 != col2) range.Merge();
            worksheet.Cell(row1, col1).Value = text;
        }
        Merged(headerRow, 1, headerRow + 1, 1, "Mã hàng");
        Merged(headerRow, 2, headerRow + 1, 2, "Tên hàng");
        Merged(headerRow, 3, headerRow + 1, 3, "ĐVT");
        var groupTitles = new[] { "Đầu kỳ", "Nhập kho", "Xuất kho", "Cuối kỳ" };
        for (var g = 0; g < groupTitles.Length; g++)
        {
            var col = 4 + g * 2;
            Merged(headerRow, col, headerRow, col + 1, groupTitles[g]);
            worksheet.Cell(headerRow + 1, col).Value     = "Số lượng";
            worksheet.Cell(headerRow + 1, col + 1).Value = "Giá trị";
        }
        Merged(headerRow, ExcelLastColumn, headerRow + 1, ExcelLastColumn, "Ngày HT");

        var headerRange = worksheet.Range(headerRow, 1, headerRow + 1, ExcelLastColumn);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = blue;
        headerRange.Style.Alignment.Horizontal = center;
        headerRange.Style.Alignment.Vertical   = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

        var r = headerRow + 2;
        foreach (var item in Items)
        {
            // Mã hàng là chữ (vd "01") — ép kiểu text để Excel không đổi thành số.
            worksheet.Cell(r, 1).SetValue(item.Code);
            worksheet.Cell(r, 2).Value = item.Name;
            worksheet.Cell(r, 3).Value = item.Unit;
            worksheet.Cell(r, 4).Value  = item.OpeningQty;
            worksheet.Cell(r, 5).Value  = item.OpeningValue;
            worksheet.Cell(r, 6).Value  = item.ImportQty;
            worksheet.Cell(r, 7).Value  = item.ImportValue;
            worksheet.Cell(r, 8).Value  = item.ExportQty;
            worksheet.Cell(r, 9).Value  = item.ExportValue;
            worksheet.Cell(r, 10).Value = item.ClosingQty;
            worksheet.Cell(r, 11).Value = item.ClosingValue;
            worksheet.Cell(r, 12).SetValue(DateText(item));
            if (item.IsGroupHeader)
            {
                var groupRow = worksheet.Range(r, 1, r, ExcelLastColumn);
                groupRow.Style.Font.Bold = true;
                groupRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FFF2CC");
            }
            r++;
        }

        // Chân bảng: Số dòng + tổng các dòng mặt hàng (không cộng dòng tổng nhóm kho).
        var dataRows = Items.Where(i => !i.IsGroupHeader).ToList();
        worksheet.Cell(r, 2).Value  = $"Số dòng = {dataRows.Count}";
        worksheet.Cell(r, 4).Value  = dataRows.Sum(i => i.OpeningQty);
        worksheet.Cell(r, 5).Value  = dataRows.Sum(i => i.OpeningValue);
        worksheet.Cell(r, 6).Value  = dataRows.Sum(i => i.ImportQty);
        worksheet.Cell(r, 7).Value  = dataRows.Sum(i => i.ImportValue);
        worksheet.Cell(r, 8).Value  = dataRows.Sum(i => i.ExportQty);
        worksheet.Cell(r, 9).Value  = dataRows.Sum(i => i.ExportValue);
        worksheet.Cell(r, 10).Value = dataRows.Sum(i => i.ClosingQty);
        worksheet.Cell(r, 11).Value = dataRows.Sum(i => i.ClosingValue);
        worksheet.Range(r, 1, r, ExcelLastColumn).Style.Font.Bold = true;

        worksheet.Range(headerRow + 2, 4, r, 11).Style.NumberFormat.Format = "#,##0";
        var table = worksheet.Range(headerRow, 1, r, ExcelLastColumn);
        table.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        table.Style.Border.InsideBorder  = ClosedXML.Excel.XLBorderStyleValues.Thin;

        // Chỉ đo độ rộng theo bảng — tiêu đề báo cáo ở dòng 1–2 đã gộp ô nên không được làm cột A phình ra.
        worksheet.Columns(1, ExcelLastColumn).AdjustToContents(headerRow, r);
        return workbook;
    }

    private FlowDocument BuildReportDocument()
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 9,
            PagePadding = new Thickness(20),
        };

        doc.Blocks.Add(new Paragraph(new Bold(new Run(ReportTitle)) { FontSize = 16 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 4),
        });
        doc.Blocks.Add(new Paragraph(new Italic(new Run(ReportSubtitle)))
        {
            TextAlignment = TextAlignment.Center,
            FontSize      = 11,
            Margin        = new Thickness(0, 0, 0, 12),
        });

        // Bản in bỏ cột Ngày HT cho vừa khổ giấy dọc (vẫn có trong lưới và file Excel).
        var table = new Table { CellSpacing = 0 };
        foreach (var width in new[] { 50, 150, 40, 48, 70, 48, 70, 48, 70, 48, 70 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var group = new TableRowGroup();
        var top = new TableRow { Background = Brushes.WhiteSmoke };
        top.Cells.Add(PrintCell("Mã hàng", true, TextAlignment.Center, rowSpan: 2));
        top.Cells.Add(PrintCell("Tên hàng", true, TextAlignment.Center, rowSpan: 2));
        top.Cells.Add(PrintCell("ĐVT",      true, TextAlignment.Center, rowSpan: 2));
        foreach (var title in new[] { "Đầu kỳ", "Nhập kho", "Xuất kho", "Cuối kỳ" })
            top.Cells.Add(PrintCell(title, true, TextAlignment.Center, columnSpan: 2));
        group.Rows.Add(top);

        var sub = new TableRow { Background = Brushes.WhiteSmoke };
        for (var i = 0; i < 4; i++)
        {
            sub.Cells.Add(PrintCell("Số lượng", true, TextAlignment.Center));
            sub.Cells.Add(PrintCell("Giá trị",  true, TextAlignment.Center));
        }
        group.Rows.Add(sub);

        foreach (var item in Items)
            group.Rows.Add(PrintDataRow(item.IsGroupHeader, item.Code, item.Name, item.Unit,
                item.OpeningQty, item.OpeningValue, item.ImportQty, item.ImportValue,
                item.ExportQty, item.ExportValue, item.ClosingQty, item.ClosingValue));

        var dataRows = Items.Where(i => !i.IsGroupHeader).ToList();
        group.Rows.Add(PrintDataRow(true, string.Empty, $"Số dòng = {dataRows.Count}", string.Empty,
            dataRows.Sum(i => i.OpeningQty), dataRows.Sum(i => i.OpeningValue),
            dataRows.Sum(i => i.ImportQty),  dataRows.Sum(i => i.ImportValue),
            dataRows.Sum(i => i.ExportQty),  dataRows.Sum(i => i.ExportValue),
            dataRows.Sum(i => i.ClosingQty), dataRows.Sum(i => i.ClosingValue)));

        table.RowGroups.Add(group);
        doc.Blocks.Add(table);
        return doc;
    }

    private static TableRow PrintDataRow(bool bold, string code, string name, string unit, params decimal[] numbers)
    {
        var row = new TableRow { Background = bold ? Brushes.WhiteSmoke : Brushes.Transparent };
        row.Cells.Add(PrintCell(code, bold, TextAlignment.Left));
        row.Cells.Add(PrintCell(name, bold, TextAlignment.Left));
        row.Cells.Add(PrintCell(unit, bold, TextAlignment.Left));
        foreach (var n in numbers)
            row.Cells.Add(PrintCell(MoneyFormat.Format(n), bold, TextAlignment.Right));
        return row;
    }

    private static TableCell PrintCell(
        string text, bool bold, TextAlignment alignment, int rowSpan = 1, int columnSpan = 1)
    {
        Inline content = bold ? new Bold(new Run(text)) : new Run(text);
        return new TableCell(new Paragraph(content) { TextAlignment = alignment })
        {
            RowSpan         = rowSpan,
            ColumnSpan      = columnSpan,
            Padding         = new Thickness(3),
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
        };
    }
}
