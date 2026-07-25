// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using DesktopLamour.Features.HomePage.Categories.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductFormViewModel : ViewModelBase
{
    private readonly ICreateProductUseCase   _createUseCase;
    private readonly IUpdateProductUseCase   _updateUseCase;
    private readonly IGetCategoriesUseCase   _getCategories;
    private readonly Func<CategoryFormWindow> _categoryFormWindowFactory;
    private readonly ILogger<ProductFormViewModel> _logger;

    private bool _isEditMode;
    private int  _editingId;
    private int  _pendingCategoryId;

    [ObservableProperty] private string  _windowTitle  = "Thêm sản phẩm";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;

    // Form fields
    [ObservableProperty] private string  _code          = string.Empty;
    [ObservableProperty] private string  _name          = string.Empty;
    [ObservableProperty] private ISearchableItem? _selectedCategory;
    [ObservableProperty] private string  _unit          = string.Empty;
    [ObservableProperty] private decimal _costPrice;
    [ObservableProperty] private decimal _sellingPrice;
    [ObservableProperty] private int     _stockQuantity;
    [ObservableProperty] private bool    _isActive      = true;

    // Tax fields
    [ObservableProperty] private VatRateType?       _vatRate;
    [ObservableProperty] private TaxReductionStatus? _taxReductionType;
    [ObservableProperty] private decimal?     _importTaxRate;
    [ObservableProperty] private decimal?     _exportTaxRate;
    [ObservableProperty] private string?      _exciseTaxGroup;

    public static IReadOnlyList<VatRateType?> VatRateOptions { get; } =
        new List<VatRateType?> { null }
            .Concat(Enum.GetValues<VatRateType>().Cast<VatRateType?>())
            .ToList();

    public static IReadOnlyList<TaxReductionStatus?> TaxReductionStatusOptions { get; } =
        new List<TaxReductionStatus?> { null }
            .Concat(Enum.GetValues<TaxReductionStatus>().Cast<TaxReductionStatus?>())
            .ToList();

    public static IReadOnlyList<string?> ExciseTaxGroupOptions { get; } =
        new List<string?> { null, "Nhóm 1", "Nhóm 2", "Nhóm 3", "Nhóm 4", "Nhóm 5" };

    public bool IsAddMode => !_isEditMode;

    public IReadOnlyList<ISearchableItem> Categories { get; private set; } = Array.Empty<ISearchableItem>();

    public event Action<bool>? RequestClose;

    public ProductFormViewModel(
        ICreateProductUseCase createUseCase,
        IUpdateProductUseCase updateUseCase,
        IGetCategoriesUseCase getCategories,
        Func<CategoryFormWindow> categoryFormWindowFactory,
        ILogger<ProductFormViewModel> logger)
    {
        _createUseCase             = createUseCase;
        _updateUseCase             = updateUseCase;
        _getCategories             = getCategories;
        _categoryFormWindowFactory = categoryFormWindowFactory;
        _logger                   = logger;
    }

    public void Initialize(Product? product)
    {
        ErrorMessage = string.Empty;

        if (product is null)
        {
            _isEditMode      = false;
            _editingId       = 0;
            _pendingCategoryId = 0;
            WindowTitle      = "Thêm sản phẩm";
            Code             = Name = Unit = string.Empty;
            SelectedCategory = null;
            CostPrice        = 0;
            SellingPrice     = 0;
            StockQuantity    = 0;
            IsActive         = true;
            VatRate          = null;
            TaxReductionType = TaxReductionStatus.CoGiamThue;
            ImportTaxRate    = null;
            ExportTaxRate    = null;
            ExciseTaxGroup   = null;
        }
        else
        {
            _isEditMode      = true;
            _editingId       = product.Id;
            _pendingCategoryId = product.CategoryId;
            WindowTitle      = "Sửa sản phẩm";
            Code             = product.Code;
            Name             = product.Name;
            Unit             = product.Unit;
            CostPrice        = product.CostPrice;
            SellingPrice     = product.SellingPrice;
            StockQuantity    = product.StockQuantity;
            IsActive         = product.IsActive;
            VatRate          = product.VatRate;
            TaxReductionType = product.TaxReductionType;
            ImportTaxRate    = product.ImportTaxRate;
            ExportTaxRate    = product.ExportTaxRate;
            ExciseTaxGroup   = product.ExciseTaxGroup;
        }

        OnPropertyChanged(nameof(IsAddMode));
        BeginDirtyTracking();

        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var categories = await _getCategories.ExecuteAsync(ct);
            Categories = categories.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Categories));
            if (_pendingCategoryId > 0)
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _pendingCategoryId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load categories for product form");
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync(CancellationToken ct = default)
    {
        var before = Categories.Select(c => c.Id).ToHashSet();
        var window = _categoryFormWindowFactory();
        window.Initialize();
        if (window.ShowDialog() != true) return;
        try
        {
            var categories = await _getCategories.ExecuteAsync(ct);
            Categories = categories.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Categories));
            var newItem = Categories.FirstOrDefault(c => !before.Contains(c.Id));
            if (newItem is not null) SelectedCategory = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload categories after add"); }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        ErrorMessage = string.Empty;

        if (SelectedCategory is null)
        {
            ErrorMessage = "Vui lòng chọn danh mục.";
            return;
        }

        IsLoading = true;
        try
        {
            if (!_isEditMode)
            {
                var input = new CreateProductInput(
                    Code.Trim(), Name.Trim(), SelectedCategory.Id, Unit.Trim(),
                    CostPrice, SellingPrice, StockQuantity, IsActive,
                    VatRate, TaxReductionType, ImportTaxRate, ExportTaxRate, ExciseTaxGroup);
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateProductInput(
                    _editingId, Code.Trim(), Name.Trim(), SelectedCategory.Id, Unit.Trim(),
                    CostPrice, SellingPrice, StockQuantity, IsActive,
                    VatRate, TaxReductionType, ImportTaxRate, ExportTaxRate, ExciseTaxGroup);
                await _updateUseCase.ExecuteAsync(input, ct);
            }
            StopDirtyTracking();
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
    private void Cancel()
    {
        if (IsDirty)
        {
            var r = MessageBox.Show(
                "Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;
        }
        StopDirtyTracking();
        RequestClose?.Invoke(false);
    }
}
