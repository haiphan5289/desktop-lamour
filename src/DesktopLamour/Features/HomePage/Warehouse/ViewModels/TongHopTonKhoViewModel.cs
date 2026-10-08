// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
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
using Microsoft.Extensions.Logging;

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

    public ObservableCollection<InventorySummaryItem> Items { get; } = new();

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
            foreach (var w in warehouseTask.Result.Cast<ISearchableItem>())
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
            var groups = (await _getSummary.ExecuteAsync(
                from, to, warehouseIds, SelectedCategory?.Id, SelectedProductUnit?.Id, productIds, ct)).ToList();

            // Mỗi kho: 1 dòng tổng (tiêu đề nhóm, số liệu = tổng các mặt hàng của kho) rồi tới các mặt hàng.
            Items.Clear();
            foreach (var g in groups)
            {
                Items.Add(new InventorySummaryItem
                {
                    IsGroupHeader = true,
                    WarehouseId   = g.WarehouseId,
                    WarehouseCode = g.WarehouseCode,
                    WarehouseName = g.WarehouseName,
                    Name          = $"Tên kho : {g.WarehouseName} ({g.Items.Count})",
                    OpeningQty    = g.Items.Sum(i => i.OpeningQty),
                    OpeningValue  = g.Items.Sum(i => i.OpeningValue),
                    ImportQty     = g.Items.Sum(i => i.ImportQty),
                    ImportValue   = g.Items.Sum(i => i.ImportValue),
                    ExportQty     = g.Items.Sum(i => i.ExportQty),
                    ExportValue   = g.Items.Sum(i => i.ExportValue),
                    ClosingQty    = g.Items.Sum(i => i.ClosingQty),
                    ClosingValue  = g.Items.Sum(i => i.ClosingValue),
                });
                foreach (var item in g.Items) Items.Add(item);
            }

            var dataRows    = Items.Where(i => !i.IsGroupHeader).ToList();
            RowCount        = dataRows.Count;
            TotalOpeningQty = dataRows.Sum(i => i.OpeningQty);
            TotalImportQty  = dataRows.Sum(i => i.ImportQty);
            TotalExportQty  = dataRows.Sum(i => i.ExportQty);
            TotalClosingQty = dataRows.Sum(i => i.ClosingQty);
            ReportSubtitle  = $"Từ ngày {FromDate:dd/MM/yyyy} đến ngày {ToDate:dd/MM/yyyy}";
            HasItems        = Items.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
