// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Views;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly INavigationService         _navigationService;
    private readonly IGetProductsUseCase        _getProducts;
    private readonly IDeleteProductUseCase      _deleteProduct;
    private readonly IDuplicateProductUseCase   _duplicateProduct;
    private readonly Func<ProductFormWindow>    _formWindowFactory;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasProducts;
    [ObservableProperty] private Product? _selectedProduct;

    public ObservableCollection<Product> Products { get; } = new();

    private bool HasSelection => SelectedProduct is not null;

    public ProductListViewModel(
        INavigationService       navigationService,
        IGetProductsUseCase      getProducts,
        IDeleteProductUseCase    deleteProduct,
        IDuplicateProductUseCase duplicateProduct,
        Func<ProductFormWindow>  formWindowFactory)
    {
        _navigationService = navigationService;
        _getProducts       = getProducts;
        _deleteProduct     = deleteProduct;
        _duplicateProduct  = duplicateProduct;
        _formWindowFactory = formWindowFactory;
    }

    partial void OnSelectedProductChanged(Product? value)
    {
        DuplicateProductCommand.NotifyCanExecuteChanged();
        EditProductCommand.NotifyCanExecuteChanged();
        DeleteProductCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadProductsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getProducts.ExecuteAsync(ct);
            Products.Clear();
            foreach (var p in items) Products.Add(p);
            HasProducts = Products.Count > 0;
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
    private async Task AddProductAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadProductsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DuplicateProductAsync(CancellationToken ct = default)
    {
        if (SelectedProduct is null) return;
        IsLoading = true;
        try
        {
            var copy = await _duplicateProduct.ExecuteAsync(SelectedProduct.Id, ct);
            Products.Add(copy);
            HasProducts     = true;
            SelectedProduct = copy;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Nhân bản thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditProductAsync(CancellationToken ct = default)
    {
        if (SelectedProduct is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedProduct);
        if (window.ShowDialog() == true)
            await LoadProductsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteProductAsync(CancellationToken ct = default)
    {
        if (SelectedProduct is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa sản phẩm '{SelectedProduct.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteProduct.ExecuteAsync(SelectedProduct.Id, ct);
            Products.Remove(SelectedProduct);
            SelectedProduct = null;
            HasProducts     = Products.Count > 0;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
