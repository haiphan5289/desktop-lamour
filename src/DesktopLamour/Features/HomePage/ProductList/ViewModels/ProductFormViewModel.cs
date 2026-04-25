// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductFormViewModel : ViewModelBase
{
    private readonly ICreateProductUseCase _createUseCase;
    private readonly IUpdateProductUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string  _windowTitle  = "Thêm sản phẩm";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;

    // Form fields
    [ObservableProperty] private string  _code          = string.Empty;
    [ObservableProperty] private string  _name          = string.Empty;
    [ObservableProperty] private string  _category      = string.Empty;
    [ObservableProperty] private string  _unit          = string.Empty;
    [ObservableProperty] private decimal _costPrice;
    [ObservableProperty] private decimal _sellingPrice;
    [ObservableProperty] private int     _stockQuantity;
    [ObservableProperty] private bool    _isActive      = true;

    public bool IsAddMode => !_isEditMode;

    public event Action<bool>? RequestClose;

    public ProductFormViewModel(
        ICreateProductUseCase createUseCase,
        IUpdateProductUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(Product? product)
    {
        ErrorMessage = string.Empty;

        if (product is null)
        {
            _isEditMode   = false;
            _editingId    = 0;
            WindowTitle   = "Thêm sản phẩm";
            Code          = Name = Category = Unit = string.Empty;
            CostPrice     = 0;
            SellingPrice  = 0;
            StockQuantity = 0;
            IsActive      = true;
        }
        else
        {
            _isEditMode   = true;
            _editingId    = product.Id;
            WindowTitle   = "Sửa sản phẩm";
            Code          = product.Code;
            Name          = product.Name;
            Category      = product.Category;
            Unit          = product.Unit;
            CostPrice     = product.CostPrice;
            SellingPrice  = product.SellingPrice;
            StockQuantity = product.StockQuantity;
            IsActive      = product.IsActive;
        }

        OnPropertyChanged(nameof(IsAddMode));
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        ErrorMessage = string.Empty;
        IsLoading    = true;
        try
        {
            if (!_isEditMode)
            {
                var input = new CreateProductInput(
                    Code.Trim(), Name.Trim(), Category.Trim(), Unit.Trim(),
                    CostPrice, SellingPrice, StockQuantity, IsActive);
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateProductInput(
                    _editingId, Code.Trim(), Name.Trim(), Category.Trim(), Unit.Trim(),
                    CostPrice, SellingPrice, StockQuantity, IsActive);
                await _updateUseCase.ExecuteAsync(input, ct);
            }
            RequestClose?.Invoke(true);
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Lưu thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);
}
