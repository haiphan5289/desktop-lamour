// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductUnits.Views;

namespace DesktopLamour.Features.HomePage.ProductUnits.ViewModels;

public partial class ProductUnitListViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetProductUnitsUseCase     _getUnits;
    private readonly IDeleteProductUnitUseCase   _deleteUnit;
    private readonly Func<ProductUnitFormWindow> _formWindowFactory;

    [ObservableProperty] private bool         _isLoading;
    [ObservableProperty] private bool         _hasError;
    [ObservableProperty] private string       _errorMessage = string.Empty;
    [ObservableProperty] private bool         _hasUnits;
    [ObservableProperty] private ProductUnit? _selectedUnit;

    public ObservableCollection<ProductUnit> Units { get; } = new();

    private bool HasSelection => SelectedUnit is not null;

    public ProductUnitListViewModel(
        INavigationService          navigationService,
        IGetProductUnitsUseCase     getUnits,
        IDeleteProductUnitUseCase   deleteUnit,
        Func<ProductUnitFormWindow> formWindowFactory)
    {
        _navigationService = navigationService;
        _getUnits           = getUnits;
        _deleteUnit         = deleteUnit;
        _formWindowFactory  = formWindowFactory;
    }

    partial void OnSelectedUnitChanged(ProductUnit? value)
    {
        EditUnitCommand.NotifyCanExecuteChanged();
        DeleteUnitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadUnitsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getUnits.ExecuteAsync(ct);
            Units.Clear();
            foreach (var u in items) Units.Add(u);
            HasUnits = Units.Count > 0;
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
    private async Task AddUnitAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadUnitsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditUnitAsync(CancellationToken ct = default)
    {
        if (SelectedUnit is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedUnit);
        if (window.ShowDialog() == true)
            await LoadUnitsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteUnitAsync(CancellationToken ct = default)
    {
        if (SelectedUnit is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa đơn vị tính '{SelectedUnit.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteUnit.ExecuteAsync(SelectedUnit.Id, ct);
            Units.Remove(SelectedUnit);
            SelectedUnit = null;
            HasUnits      = Units.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
