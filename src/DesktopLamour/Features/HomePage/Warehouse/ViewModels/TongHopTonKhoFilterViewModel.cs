// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class TongHopTonKhoFilterViewModel : ViewModelBase
{
    public event Action<bool>? RequestClose;

    private readonly IGetWarehouseSettingsUseCase _getWarehouses;
    private readonly IGetCategoriesUseCase        _getCategories;
    private readonly IGetProductUnitsUseCase      _getProductUnits;
    private readonly ILogger<TongHopTonKhoFilterViewModel> _logger;

    [ObservableProperty] private bool             _isLoading;
    [ObservableProperty] private string            _periodPreset = "Tùy chọn";
    [ObservableProperty] private DateTime          _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime          _toDate   = DateTime.Today;
    [ObservableProperty] private ISearchableItem?  _selectedCategory;
    [ObservableProperty] private ISearchableItem?  _selectedProductUnit;

    public IReadOnlyList<string> PeriodPresetOptions { get; } = new[] { "Tháng này", "Quý này", "Năm này", "Tùy chọn" };
    public IReadOnlyList<ISearchableItem> Categories   { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> ProductUnits { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<WarehouseCheckItem> WarehouseItems { get; } = new();

    public InventoryFilter Result { get; private set; } = new();

    public TongHopTonKhoFilterViewModel(
        IGetWarehouseSettingsUseCase getWarehouses,
        IGetCategoriesUseCase        getCategories,
        IGetProductUnitsUseCase      getProductUnits,
        ILogger<TongHopTonKhoFilterViewModel> logger)
    {
        _getWarehouses   = getWarehouses;
        _getCategories   = getCategories;
        _getProductUnits = getProductUnits;
        _logger          = logger;
    }

    public async Task InitializeAsync(InventoryFilter current, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            var warehouseTask = _getWarehouses.ExecuteAsync(ct);
            var categoryTask  = _getCategories.ExecuteAsync(ct);
            var unitTask      = _getProductUnits.ExecuteAsync(ct);
            await Task.WhenAll(warehouseTask, categoryTask, unitTask);

            Categories = categoryTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Categories));
            ProductUnits = unitTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(ProductUnits));

            WarehouseItems.Clear();
            foreach (var w in warehouseTask.Result.Cast<ISearchableItem>())
                WarehouseItems.Add(new WarehouseCheckItem(w) { IsSelected = current.WarehouseIds.Contains(w.Id) });

            FromDate             = current.FromDate;
            ToDate               = current.ToDate;
            SelectedCategory     = Categories.FirstOrDefault(c => c.Id == current.CategoryId);
            SelectedProductUnit  = ProductUnits.FirstOrDefault(u => u.Id == current.ProductUnitId);
            PeriodPreset         = "Tùy chọn";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load filter lookups for TongHopTonKho popup");
        }
        finally { IsLoading = false; }
    }

    // Preset chỉ tính lại From/To client-side — không đổi cách gọi BE.
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
        }
    }

    [RelayCommand]
    private void Apply()
    {
        Result = new InventoryFilter
        {
            FromDate      = FromDate,
            ToDate        = ToDate,
            CategoryId    = SelectedCategory?.Id,
            ProductUnitId = SelectedProductUnit?.Id,
            WarehouseIds  = WarehouseItems.Where(w => w.IsSelected).Select(w => w.Id).ToList(),
        };
        RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);

    [RelayCommand]
    private void ClearConditions()
    {
        PeriodPreset        = "Tùy chọn";
        FromDate            = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        ToDate              = DateTime.Today;
        SelectedCategory    = null;
        SelectedProductUnit = null;
        foreach (var w in WarehouseItems) w.IsSelected = false;
    }
}
