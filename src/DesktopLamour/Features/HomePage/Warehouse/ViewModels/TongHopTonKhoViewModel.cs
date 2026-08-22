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
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

// Bộ lọc hiển thị NGAY trên màn hình (không còn popup riêng — TongHopTonKhoFilterWindow đã bị
// gỡ bỏ) để người dùng thấy và đổi điều kiện lọc trực tiếp, giống hàng lọc của các màn danh sách
// khác trong app.
public partial class TongHopTonKhoViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetInventorySummaryUseCase _getSummary;
    private readonly IGetWarehouseSettingsUseCase _getWarehouses;
    private readonly IGetCategoriesUseCase        _getCategories;
    private readonly IGetProductUnitsUseCase      _getProductUnits;
    private readonly IGetProductsUseCase          _getProducts;
    private readonly ILogger<TongHopTonKhoViewModel> _logger;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;

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
        IGetInventorySummaryUseCase     getSummary,
        IGetWarehouseSettingsUseCase    getWarehouses,
        IGetCategoriesUseCase           getCategories,
        IGetProductUnitsUseCase         getProductUnits,
        IGetProductsUseCase             getProducts,
        ILogger<TongHopTonKhoViewModel> logger)
    {
        _navigationService = navigationService;
        _getSummary        = getSummary;
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

        var selectedWarehouses = WarehouseItems.Where(w => w.IsSelected).ToList();
        var filter = new InventoryDetailFilter
        {
            ProductId      = item.ProductId,
            ProductLabel   = $"{item.Code} — {item.Name}",
            FromDate       = FromDate,
            ToDate         = ToDate,
            WarehouseIds   = selectedWarehouses.Count > 0 ? selectedWarehouses.Select(w => w.Id).ToList() : null,
            WarehouseLabel = selectedWarehouses.Count > 0 ? string.Join(", ", selectedWarehouses.Select(w => w.Name)) : null,
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

            WarehouseItems.Clear();
            foreach (var w in warehouseTask.Result.Cast<ISearchableItem>())
                WarehouseItems.Add(new WarehouseCheckItem(w));

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
            var data = await _getSummary.ExecuteAsync(
                from, to, warehouseIds, SelectedCategory?.Id, SelectedProductUnit?.Id, productIds, ct);

            Items.Clear();
            foreach (var item in data) Items.Add(item);
            HasItems = Items.Count > 0;
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
