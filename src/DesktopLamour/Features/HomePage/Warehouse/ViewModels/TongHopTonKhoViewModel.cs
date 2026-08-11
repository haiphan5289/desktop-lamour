// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class TongHopTonKhoViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetInventorySummaryUseCase _getSummary;
    private readonly Func<TongHopTonKhoFilterWindow> _filterWindowFactory;

    private InventoryFilter _filter = new();

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;
    [ObservableProperty] private string _filterSummary = "Chưa lọc — nhấn 🔍 Lọc để chọn kỳ báo cáo và điều kiện.";

    public ObservableCollection<InventorySummaryItem> Items { get; } = new();

    public TongHopTonKhoViewModel(
        INavigationService              navigationService,
        IGetInventorySummaryUseCase     getSummary,
        Func<TongHopTonKhoFilterWindow> filterWindowFactory)
    {
        _navigationService   = navigationService;
        _getSummary          = getSummary;
        _filterWindowFactory = filterWindowFactory;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private async Task OpenFilterAsync(CancellationToken ct = default)
    {
        var window = _filterWindowFactory();
        await window.InitializeAsync(_filter, ct);
        if (window.ShowDialog() != true) return;

        _filter = window.Result;
        UpdateFilterSummary();
        await LoadAsync(ct);
    }

    private void UpdateFilterSummary()
    {
        var range = $"{_filter.FromDate:dd/MM/yyyy} → {_filter.ToDate:dd/MM/yyyy}";
        var extras = new List<string>();
        if (_filter.WarehouseIds.Count > 0) extras.Add($"{_filter.WarehouseIds.Count} kho");
        if (_filter.CategoryId is not null) extras.Add("1 nhóm VTHH");
        if (_filter.ProductUnitId is not null) extras.Add("1 đơn vị tính");
        FilterSummary = extras.Count > 0
            ? $"{range} · {string.Join(", ", extras)}"
            : range;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var from = DateOnly.FromDateTime(_filter.FromDate);
            var to   = DateOnly.FromDateTime(_filter.ToDate);
            var data = await _getSummary.ExecuteAsync(
                from, to, _filter.WarehouseIds, _filter.CategoryId, _filter.ProductUnitId, ct);

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
