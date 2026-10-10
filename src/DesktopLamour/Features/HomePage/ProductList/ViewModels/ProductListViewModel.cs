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
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Views;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly INavigationService         _navigationService;
    private readonly IGetProductsUseCase        _getProducts;
    private readonly IGetCategoriesUseCase      _getCategories;
    private readonly IDeleteProductUseCase      _deleteProduct;
    private readonly IDuplicateProductUseCase   _duplicateProduct;
    private readonly IImportExcelProductsUseCase _importExcel;
    private readonly Func<ProductFormWindow>    _formWindowFactory;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasProducts;
    [ObservableProperty] private Product? _selectedProduct;

    // ── Filter theo từng cột — nhúng ngay trong header lưới (khớp UI MISA), AND với nhau và với
    // ô Tìm kiếm chung + dropdown "Nhóm vật tư, hàng hóa, dịch vụ".
    [ObservableProperty] private string _filterCode         = string.Empty;
    [ObservableProperty] private string _filterName         = string.Empty;
    [ObservableProperty] private string _filterNature       = string.Empty;
    [ObservableProperty] private string _filterCategory     = string.Empty;
    [ObservableProperty] private string _filterUnit         = string.Empty;
    [ObservableProperty] private string _filterTaxReduction = string.Empty;
    // null = không lọc (checkbox 3 trạng thái), true = chỉ dòng ngừng theo dõi, false = chỉ dòng đang theo dõi.
    [ObservableProperty] private bool?  _filterStopTracking;

    public NumericColumnFilter StockQuantityFilter { get; } = new();
    public NumericColumnFilter StockValueFilter    { get; } = new();

    partial void OnFilterCodeChanged(string value)         => ProductsView.Refresh();
    partial void OnFilterNameChanged(string value)         => ProductsView.Refresh();
    partial void OnFilterNatureChanged(string value)       => ProductsView.Refresh();
    partial void OnFilterCategoryChanged(string value)     => ProductsView.Refresh();
    partial void OnFilterUnitChanged(string value)         => ProductsView.Refresh();
    partial void OnFilterTaxReductionChanged(string value) => ProductsView.Refresh();
    partial void OnFilterStopTrackingChanged(bool? value)  => ProductsView.Refresh();

    // Dropdown "Nhóm vật tư, hàng hóa, dịch vụ" phía trên lưới — mục đầu AllCategories = không lọc.
    private const string AllCategories = "Tất cả";
    public ObservableCollection<string> CategoryOptions { get; } = new() { AllCategories };
    [ObservableProperty] private string _selectedCategoryOption = AllCategories;

    partial void OnSelectedCategoryOptionChanged(string value) => ProductsView.Refresh();

    public ObservableCollection<Product> Products { get; } = new();

    // View lọc live theo các Filter* per-cột — DataGrid bind vào đây thay vì Products trực tiếp;
    // Products vẫn là nguồn dữ liệu thật (Add/Remove/Clear ở Load/Duplicate/Delete không đổi).
    public ICollectionView ProductsView { get; }

    // Footer — tính trên các dòng đang hiển thị sau lọc.
    public string RowCountText           => $"Số dòng = {VisibleProducts.Count()}";
    public string TotalStockQuantityText => MoneyFormat.Format(VisibleProducts.Sum(p => (decimal)p.StockQuantity), "N0");
    public string TotalStockValueText    => MoneyFormat.Format(VisibleProducts.Sum(p => p.StockValue), "N0");

    private IEnumerable<Product> VisibleProducts => ProductsView.Cast<Product>();

    private bool HasSelection => SelectedProduct is not null;

    public ProductListViewModel(
        INavigationService       navigationService,
        IGetProductsUseCase      getProducts,
        IGetCategoriesUseCase    getCategories,
        IDeleteProductUseCase    deleteProduct,
        IDuplicateProductUseCase duplicateProduct,
        IImportExcelProductsUseCase importExcel,
        Func<ProductFormWindow>  formWindowFactory)
    {
        _navigationService = navigationService;
        _getProducts       = getProducts;
        _getCategories     = getCategories;
        _deleteProduct     = deleteProduct;
        _duplicateProduct  = duplicateProduct;
        _importExcel       = importExcel;
        _formWindowFactory = formWindowFactory;

        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProduct;
        // ICollectionView tự bắn CollectionChanged (Reset) sau mỗi Refresh() — dùng chung 1 chỗ để
        // cập nhật footer, khỏi phải gọi tay ở từng OnFilterXChanged/Load/Duplicate/Delete.
        ProductsView.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(RowCountText));
            OnPropertyChanged(nameof(TotalStockQuantityText));
            OnPropertyChanged(nameof(TotalStockValueText));
        };

        StockQuantityFilter.Changed = ProductsView.Refresh;
        StockValueFilter.Changed    = ProductsView.Refresh;
    }

    private bool FilterProduct(object obj)
    {
        if (obj is not Product p) return false;

        if (!string.IsNullOrEmpty(SelectedCategoryOption) && SelectedCategoryOption != AllCategories
            && !string.Equals(p.CategoryName, SelectedCategoryOption, StringComparison.OrdinalIgnoreCase))
            return false;

        if (FilterStopTracking is { } stopTracking && p.IsStopTracking != stopTracking)
            return false;

        return Matches(p.Code, FilterCode)
            && Matches(p.Name, FilterName)
            && Matches(NatureText(p.Nature), FilterNature)
            && Matches(p.CategoryName, FilterCategory)
            && Matches(p.Unit, FilterUnit)
            && Matches(TaxReductionText(p.TaxReductionType), FilterTaxReduction)
            && StockQuantityFilter.Matches(p.StockQuantity)
            && StockValueFilter.Matches(p.StockValue);
    }

    // Chữ hiển thị trên lưới — phải khớp ProductNatureDisplayConverter/TaxReductionStatusDisplayConverter
    // để ô lọc so đúng với chữ user đang nhìn thấy.
    private static string NatureText(ProductNature nature) => nature switch
    {
        ProductNature.VatTuHangHoa => "Vật tư hàng hóa",
        ProductNature.DichVu       => "Dịch vụ",
        _                          => nature.ToString(),
    };

    private static string TaxReductionText(TaxReductionStatus? status) => status switch
    {
        TaxReductionStatus.CoGiamThue   => "Có giảm thuế",
        TaxReductionStatus.ChuaGiamThue => "Không giảm thuế",
        TaxReductionStatus.ChuaXacDinh  => "Chưa xác định",
        _                               => string.Empty,
    };

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

            var categories = await _getCategories.ExecuteAsync(ct);
            var selected   = SelectedCategoryOption;
            // Giữ nguyên mục "Tất cả" ở đầu — ComboBox tự set SelectedItem = null khi mục đang chọn bị gỡ.
            while (CategoryOptions.Count > 1) CategoryOptions.RemoveAt(CategoryOptions.Count - 1);
            foreach (var c in categories) CategoryOptions.Add(c.Name);
            SelectedCategoryOption = CategoryOptions.Contains(selected) ? selected : AllCategories;
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
