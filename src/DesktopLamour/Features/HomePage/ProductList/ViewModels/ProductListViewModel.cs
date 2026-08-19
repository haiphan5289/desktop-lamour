// Copyright © 2026 DesktopLamour. All rights reserved.
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Views;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly INavigationService         _navigationService;
    private readonly IGetProductsUseCase        _getProducts;
    private readonly IDeleteProductUseCase      _deleteProduct;
    private readonly IDuplicateProductUseCase   _duplicateProduct;
    private readonly IImportExcelProductsUseCase _importExcel;
    private readonly Func<ProductFormWindow>    _formWindowFactory;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasProducts;
    [ObservableProperty] private Product? _selectedProduct;

    // 1 ô tìm kiếm chung — khớp OR trên các trường text chính, không phân biệt hoa/thường.
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<Product> Products { get; } = new();

    // View lọc live theo các Filter* per-cột — DataGrid bind vào đây thay vì Products trực tiếp;
    // Products vẫn là nguồn dữ liệu thật (Add/Remove/Clear ở Load/Duplicate/Delete không đổi).
    public ICollectionView ProductsView { get; }

    private bool HasSelection => SelectedProduct is not null;

    public ProductListViewModel(
        INavigationService       navigationService,
        IGetProductsUseCase      getProducts,
        IDeleteProductUseCase    deleteProduct,
        IDuplicateProductUseCase duplicateProduct,
        IImportExcelProductsUseCase importExcel,
        Func<ProductFormWindow>  formWindowFactory)
    {
        _navigationService = navigationService;
        _getProducts       = getProducts;
        _deleteProduct     = deleteProduct;
        _duplicateProduct  = duplicateProduct;
        _importExcel       = importExcel;
        _formWindowFactory = formWindowFactory;

        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProduct;
    }

    partial void OnSearchTextChanged(string value) => ProductsView.Refresh();

    private bool FilterProduct(object obj)
    {
        if (obj is not Product p) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        return Matches(p.Code, SearchText)
            || Matches(p.Name, SearchText)
            || Matches(p.CategoryName, SearchText)
            || Matches(p.Unit, SearchText);
    }

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));

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
    private void NavigateToCategories()
        => _navigationService.NavigateTo(NavigationRoutes.Categories.List);

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

    [RelayCommand]
    private async Task ImportExcelAsync(CancellationToken ct = default)
    {
        var dialog = new OpenFileDialog
        {
            Title  = "Chọn file Excel sản phẩm",
            Filter = "Excel files (*.xlsx)|*.xlsx",
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            await using var stream = File.OpenRead(dialog.FileName);
            var result = await _importExcel.ExecuteAsync(stream, Path.GetFileName(dialog.FileName), ct);

            await LoadProductsCommand.ExecuteAsync(null);

            var message = $"Import hoàn tất!\n\nĐã import: {result.Imported}/{result.Total} sản phẩm.";
            if (result.Errors.Count > 0)
            {
                var errorLines = result.Errors
                    .Take(10)
                    .Select(e => $"  Dòng {e.Row}: {e.Reason}");
                message += $"\n\nDòng lỗi ({result.Skipped}):\n{string.Join("\n", errorLines)}";
                if (result.Errors.Count > 10)
                    message += $"\n  ... và {result.Errors.Count - 10} dòng lỗi khác";
            }

            MessageBox.Show(message, "Kết quả Import", MessageBoxButton.OK,
                result.Errors.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Import thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"SanPham_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sản phẩm");

            string[] headers = { "Mã sản phẩm", "Tên sản phẩm", "Danh mục", "Đơn vị", "Giá nhập", "Giá bán", "Tồn kho" };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var p in ProductsView.Cast<Product>())
            {
                worksheet.Cell(row, 1).Value = p.Code;
                worksheet.Cell(row, 2).Value = p.Name;
                worksheet.Cell(row, 3).Value = p.CategoryName;
                worksheet.Cell(row, 4).Value = p.Unit;
                worksheet.Cell(row, 5).Value = p.CostPrice;
                worksheet.Cell(row, 6).Value = p.SellingPrice;
                worksheet.Cell(row, 7).Value = p.StockQuantity;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
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
