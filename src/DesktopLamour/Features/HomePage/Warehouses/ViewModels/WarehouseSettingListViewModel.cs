// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Views;

namespace DesktopLamour.Features.HomePage.Warehouses.ViewModels;

public partial class WarehouseSettingListViewModel : ViewModelBase
{
    private readonly INavigationService               _navigationService;
    private readonly IGetWarehouseSettingsUseCase      _getWarehouses;
    private readonly IDeleteWarehouseSettingUseCase    _deleteWarehouse;
    private readonly Func<WarehouseSettingFormWindow>  _formWindowFactory;

    [ObservableProperty] private bool             _isLoading;
    [ObservableProperty] private bool             _hasError;
    [ObservableProperty] private string           _errorMessage = string.Empty;
    [ObservableProperty] private bool             _hasWarehouses;
    [ObservableProperty] private WarehouseSetting? _selectedWarehouse;

    public ObservableCollection<WarehouseSetting> Warehouses { get; } = new();

    private bool HasSelection => SelectedWarehouse is not null;

    public WarehouseSettingListViewModel(
        INavigationService               navigationService,
        IGetWarehouseSettingsUseCase      getWarehouses,
        IDeleteWarehouseSettingUseCase    deleteWarehouse,
        Func<WarehouseSettingFormWindow>  formWindowFactory)
    {
        _navigationService  = navigationService;
        _getWarehouses      = getWarehouses;
        _deleteWarehouse    = deleteWarehouse;
        _formWindowFactory  = formWindowFactory;
    }

    partial void OnSelectedWarehouseChanged(WarehouseSetting? value)
    {
        EditWarehouseCommand.NotifyCanExecuteChanged();
        DeleteWarehouseCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadWarehousesAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getWarehouses.ExecuteAsync(ct);
            Warehouses.Clear();
            foreach (var w in items) Warehouses.Add(w);
            HasWarehouses = Warehouses.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AddWarehouseAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadWarehousesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditWarehouseAsync(CancellationToken ct = default)
    {
        if (SelectedWarehouse is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedWarehouse);
        if (window.ShowDialog() == true)
            await LoadWarehousesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteWarehouseAsync(CancellationToken ct = default)
    {
        if (SelectedWarehouse is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa kho '{SelectedWarehouse.Code} — {SelectedWarehouse.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteWarehouse.ExecuteAsync(SelectedWarehouse.Id, ct);
            Warehouses.Remove(SelectedWarehouse);
            SelectedWarehouse = null;
            HasWarehouses      = Warehouses.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
