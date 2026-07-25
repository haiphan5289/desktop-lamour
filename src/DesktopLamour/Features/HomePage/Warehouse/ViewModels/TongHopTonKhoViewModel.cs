// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class TongHopTonKhoViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetInventorySummaryUseCase _getSummary;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;
    [ObservableProperty] private DateTime _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime _toDate   = DateTime.Today;

    public ObservableCollection<InventorySummaryItem> Items { get; } = new();

    public TongHopTonKhoViewModel(
        INavigationService          navigationService,
        IGetInventorySummaryUseCase getSummary)
    {
        _navigationService = navigationService;
        _getSummary        = getSummary;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var from = DateOnly.FromDateTime(FromDate);
            var to   = DateOnly.FromDateTime(ToDate);
            var data = await _getSummary.ExecuteAsync(from, to, ct);

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
