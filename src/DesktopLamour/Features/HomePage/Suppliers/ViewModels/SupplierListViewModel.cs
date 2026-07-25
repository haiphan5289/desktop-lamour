// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Suppliers.Views;

namespace DesktopLamour.Features.HomePage.Suppliers.ViewModels;

public partial class SupplierListViewModel : ViewModelBase
{
    private readonly INavigationService        _navigationService;
    private readonly IGetSuppliersUseCase      _getSuppliers;
    private readonly IDeleteSupplierUseCase    _deleteSupplier;
    private readonly IDuplicateSupplierUseCase _duplicateSupplier;
    private readonly Func<SupplierFormWindow>  _formWindowFactory;

    [ObservableProperty] private bool      _isLoading;
    [ObservableProperty] private bool      _hasError;
    [ObservableProperty] private string    _errorMessage = string.Empty;
    [ObservableProperty] private bool      _hasSuppliers;
    [ObservableProperty] private Supplier? _selectedSupplier;

    public ObservableCollection<Supplier> Suppliers { get; } = new();

    private bool HasSelection => SelectedSupplier is not null;

    public SupplierListViewModel(
        INavigationService navigationService,
        IGetSuppliersUseCase getSuppliers,
        IDeleteSupplierUseCase deleteSupplier,
        IDuplicateSupplierUseCase duplicateSupplier,
        Func<SupplierFormWindow> formWindowFactory)
    {
        _navigationService  = navigationService;
        _getSuppliers       = getSuppliers;
        _deleteSupplier     = deleteSupplier;
        _duplicateSupplier  = duplicateSupplier;
        _formWindowFactory  = formWindowFactory;
    }

    partial void OnSelectedSupplierChanged(Supplier? value)
    {
        DuplicateSupplierCommand.NotifyCanExecuteChanged();
        EditSupplierCommand.NotifyCanExecuteChanged();
        DeleteSupplierCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadSuppliersAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getSuppliers.ExecuteAsync(ct);
            Suppliers.Clear();
            foreach (var s in items) Suppliers.Add(s);
            HasSuppliers = Suppliers.Count > 0;
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
    private async Task AddSupplierAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadSuppliersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DuplicateSupplierAsync(CancellationToken ct = default)
    {
        if (SelectedSupplier is null) return;
        IsLoading = true;
        try
        {
            var copy = await _duplicateSupplier.ExecuteAsync(SelectedSupplier.Id, ct);
            Suppliers.Add(copy);
            HasSuppliers     = true;
            SelectedSupplier = copy;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Nhân bản thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditSupplierAsync(CancellationToken ct = default)
    {
        if (SelectedSupplier is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedSupplier);
        if (window.ShowDialog() == true)
            await LoadSuppliersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSupplierAsync(CancellationToken ct = default)
    {
        if (SelectedSupplier is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa nhà cung cấp '{SelectedSupplier.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteSupplier.ExecuteAsync(SelectedSupplier.Id, ct);
            Suppliers.Remove(SelectedSupplier);
            SelectedSupplier = null;
            HasSuppliers     = Suppliers.Count > 0;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
