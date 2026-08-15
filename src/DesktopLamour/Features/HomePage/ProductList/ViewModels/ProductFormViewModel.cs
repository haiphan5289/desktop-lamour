// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using DesktopLamour.Features.HomePage.Categories.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductUnits.Views;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Views;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductFormViewModel : ViewModelBase
{
    private readonly ICreateProductUseCase          _createUseCase;
    private readonly IUpdateProductUseCase          _updateUseCase;
    private readonly IGetCategoriesUseCase          _getCategories;
    private readonly IGetProductUnitsUseCase        _getProductUnits;
    private readonly IGetWarehouseSettingsUseCase   _getWarehouses;
    private readonly IGetAccountSettingsUseCase     _getAccountSettings;
    private readonly Func<CategoryFormWindow>       _categoryFormWindowFactory;
    private readonly Func<ProductUnitFormWindow>    _productUnitFormWindowFactory;
    private readonly Func<WarehouseSettingFormWindow> _warehouseFormWindowFactory;
    private readonly ILogger<ProductFormViewModel>  _logger;

    private bool _isEditMode;
    private int  _editingId;

    // Add mode: "Diễn giải khi mua"/"Diễn giải khi bán" tự động theo "Tên" cho tới khi user
    // tự tay gõ khác đi — mỗi field ngừng theo dõi riêng biệt ngay khi giá trị lệch khỏi Tên.
    private bool _purchaseDescriptionFollowsName = true;
    private bool _saleDescriptionFollowsName     = true;

    private int  _pendingCategoryId;
    private int? _pendingProductUnitId;
    private int? _pendingWarehouseId;
    private int? _pendingStockAccountId;
    private int? _pendingRevenueAccountId;
    private int? _pendingDiscountAccountId;
    private int? _pendingPriceReductionAccountId;
    private int? _pendingReturnAccountId;
    private int? _pendingCostAccountId;

    // Mặc định cho vật tư hàng hoá mới — khớp cấu hình mẫu MISA (Kho ngầm định "HH",
    // TK kho 1561, TK doanh thu 5111, TK chiết khấu 5211, TK giảm giá 5213, TK trả lại 5212, TK chi phí 632).
    private const string DefaultWarehouseCode        = "HH";
    private const string DefaultStockAccountCode     = "1561";
    private const string DefaultRevenueAccountCode   = "5111";
    private const string DefaultDiscountAccountCode  = "5211";
    private const string DefaultPriceReductionAccountCode = "5213";
    private const string DefaultReturnAccountCode    = "5212";
    private const string DefaultCostAccountCode      = "632";

    [ObservableProperty] private string  _windowTitle  = "Thêm vật tư hàng hoá";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;

    // Header fields
    [ObservableProperty] private string  _code          = string.Empty;
    [ObservableProperty] private string  _name          = string.Empty;
    [ObservableProperty] private ProductNature _nature  = ProductNature.VatTuHangHoa;
    [ObservableProperty] private ISearchableItem? _selectedCategory;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private ISearchableItem? _selectedProductUnit;
    [ObservableProperty] private string  _unit          = string.Empty;
    [ObservableProperty] private string? _warrantyPeriod;
    [ObservableProperty] private int     _minStockQuantity;
    [ObservableProperty] private string? _origin;
    [ObservableProperty] private string? _purchaseDescription;
    [ObservableProperty] private string? _saleDescription;
    [ObservableProperty] private int     _stockQuantity;

    partial void OnNameChanged(string value)
    {
        if (_isEditMode) return;
        if (_purchaseDescriptionFollowsName) PurchaseDescription = value;
        if (_saleDescriptionFollowsName)     SaleDescription     = value;
    }

    partial void OnPurchaseDescriptionChanged(string? value)
    {
        if (!_isEditMode && value != Name) _purchaseDescriptionFollowsName = false;
    }

    partial void OnSaleDescriptionChanged(string? value)
    {
        if (!_isEditMode && value != Name) _saleDescriptionFollowsName = false;
    }

    // Tab "Ngầm định"
    [ObservableProperty] private ISearchableItem? _selectedDefaultWarehouse;
    [ObservableProperty] private ISearchableItem? _selectedStockAccount;
    [ObservableProperty] private ISearchableItem? _selectedRevenueAccount;
    [ObservableProperty] private ISearchableItem? _selectedDiscountAccount;
    [ObservableProperty] private ISearchableItem? _selectedPriceReductionAccount;
    [ObservableProperty] private ISearchableItem? _selectedReturnAccount;
    [ObservableProperty] private ISearchableItem? _selectedCostAccount;
    [ObservableProperty] private decimal _tradeDiscountRate;
    [ObservableProperty] private string? _specialGoodsType;
    [ObservableProperty] private decimal _costPrice;
    [ObservableProperty] private decimal _latestPurchasePrice;
    [ObservableProperty] private decimal _sellingPrice;
    [ObservableProperty] private bool    _isPromotionalGood;
    [ObservableProperty] private bool    _isDepositProduct;

    // Tab "Thuế"
    [ObservableProperty] private VatRateType?       _vatRate;
    [ObservableProperty] private TaxReductionStatus? _taxReductionType;
    [ObservableProperty] private decimal?     _importTaxRate;
    [ObservableProperty] private decimal?     _exportTaxRate;
    [ObservableProperty] private string?      _exciseTaxGroup;

    [ObservableProperty] private bool _isActive = true;

    public bool StopTracking
    {
        get => !IsActive;
        set { IsActive = !value; OnPropertyChanged(); }
    }

    public static IReadOnlyList<ProductNature> NatureOptions { get; } =
        Enum.GetValues<ProductNature>().ToList();

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

    public static IReadOnlyList<string?> WarrantyPeriodOptions { get; } =
        new List<string?> { null, "Không bảo hành", "6 tháng", "12 tháng", "24 tháng" };

    public bool IsAddMode => !_isEditMode;

    public IReadOnlyList<ISearchableItem> Categories       { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> ProductUnits     { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Warehouses       { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> AccountSettings  { get; private set; } = Array.Empty<ISearchableItem>();

    public event Action<bool>? RequestClose;

    public ProductFormViewModel(
        ICreateProductUseCase createUseCase,
        IUpdateProductUseCase updateUseCase,
        IGetCategoriesUseCase getCategories,
        IGetProductUnitsUseCase getProductUnits,
        IGetWarehouseSettingsUseCase getWarehouses,
        IGetAccountSettingsUseCase getAccountSettings,
        Func<CategoryFormWindow> categoryFormWindowFactory,
        Func<ProductUnitFormWindow> productUnitFormWindowFactory,
        Func<WarehouseSettingFormWindow> warehouseFormWindowFactory,
        ILogger<ProductFormViewModel> logger)
    {
        _createUseCase                = createUseCase;
        _updateUseCase                = updateUseCase;
        _getCategories                = getCategories;
        _getProductUnits              = getProductUnits;
        _getWarehouses                = getWarehouses;
        _getAccountSettings           = getAccountSettings;
        _categoryFormWindowFactory    = categoryFormWindowFactory;
        _productUnitFormWindowFactory = productUnitFormWindowFactory;
        _warehouseFormWindowFactory   = warehouseFormWindowFactory;
        _logger                       = logger;
    }

    public void Initialize(Product? product)
    {
        ErrorMessage = string.Empty;

        if (product is null)
        {
            _isEditMode                     = false;
            _editingId                      = 0;
            _purchaseDescriptionFollowsName = true;
            _saleDescriptionFollowsName     = true;
            _pendingCategoryId              = 0;
            _pendingProductUnitId           = null;
            _pendingWarehouseId             = null;
            _pendingStockAccountId          = null;
            _pendingRevenueAccountId        = null;
            _pendingDiscountAccountId       = null;
            _pendingPriceReductionAccountId = null;
            _pendingReturnAccountId         = null;
            _pendingCostAccountId           = null;

            WindowTitle          = "Thêm vật tư hàng hoá";
            Code                 = Name = Unit = string.Empty;
            Nature               = ProductNature.VatTuHangHoa;
            SelectedCategory     = null;
            Description          = null;
            SelectedProductUnit  = null;
            WarrantyPeriod       = null;
            MinStockQuantity     = 0;
            Origin               = null;
            // PurchaseDescription/SaleDescription không reset thủ công ở đây — dòng "Code = Name = Unit"
            // phía trên đã trigger OnNameChanged() tự điền 2 field này về "" (theo dõi Name). Set null
            // thủ công tại đây sẽ bị hiểu nhầm là user tự sửa (giá trị lệch khỏi Name) và tắt auto-sync.
            CostPrice            = 0;
            SellingPrice         = 0;
            StockQuantity        = 0;
            IsActive             = true;
            VatRate              = null;
            TaxReductionType     = TaxReductionStatus.CoGiamThue;
            ImportTaxRate        = null;
            ExportTaxRate        = null;
            ExciseTaxGroup       = null;

            SelectedDefaultWarehouse       = null;
            SelectedStockAccount           = null;
            SelectedRevenueAccount         = null;
            SelectedDiscountAccount        = null;
            SelectedPriceReductionAccount  = null;
            SelectedReturnAccount          = null;
            SelectedCostAccount            = null;
            TradeDiscountRate              = 0;
            SpecialGoodsType               = null;
            LatestPurchasePrice            = 0;
            IsPromotionalGood              = false;
            IsDepositProduct               = false;
        }
        else
        {
            _isEditMode                     = true;
            _editingId                      = product.Id;
            _purchaseDescriptionFollowsName = false;
            _saleDescriptionFollowsName     = false;
            _pendingCategoryId              = product.CategoryId ?? 0;
            _pendingProductUnitId           = product.ProductUnitId;
            _pendingWarehouseId             = product.DefaultWarehouseId;
            _pendingStockAccountId          = product.StockAccountId;
            _pendingRevenueAccountId        = product.RevenueAccountId;
            _pendingDiscountAccountId       = product.DiscountAccountId;
            _pendingPriceReductionAccountId = product.PriceReductionAccountId;
            _pendingReturnAccountId         = product.ReturnAccountId;
            _pendingCostAccountId           = product.CostAccountId;

            WindowTitle          = "Sửa vật tư hàng hoá";
            Code                 = product.Code;
            Name                 = product.Name;
            Nature               = product.Nature;
            Description          = product.Description;
            Unit                 = product.Unit;
            WarrantyPeriod       = product.WarrantyPeriod;
            MinStockQuantity     = product.MinStockQuantity;
            Origin               = product.Origin;
            PurchaseDescription  = product.PurchaseDescription;
            SaleDescription      = product.SaleDescription;
            CostPrice            = product.CostPrice;
            SellingPrice         = product.SellingPrice;
            StockQuantity        = product.StockQuantity;
            IsActive             = product.IsActive;
            VatRate              = product.VatRate;
            TaxReductionType     = product.TaxReductionType;
            ImportTaxRate        = product.ImportTaxRate;
            ExportTaxRate        = product.ExportTaxRate;
            ExciseTaxGroup       = product.ExciseTaxGroup;

            TradeDiscountRate    = product.TradeDiscountRate;
            SpecialGoodsType     = product.SpecialGoodsType;
            LatestPurchasePrice  = product.LatestPurchasePrice;
            IsPromotionalGood    = product.IsPromotionalGood;
            IsDepositProduct     = product.IsDepositProduct;
        }

        OnPropertyChanged(nameof(IsAddMode));
        OnPropertyChanged(nameof(StopTracking));
        BeginDirtyTracking();

        _ = LoadLookupsAsync();
    }

    private async Task LoadLookupsAsync(CancellationToken ct = default)
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

        try
        {
            var units = await _getProductUnits.ExecuteAsync(ct);
            ProductUnits = units.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(ProductUnits));
            if (_pendingProductUnitId is > 0)
                SelectedProductUnit = ProductUnits.FirstOrDefault(u => u.Id == _pendingProductUnitId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load product units for product form");
        }

        try
        {
            var warehouses = await _getWarehouses.ExecuteAsync(ct);
            Warehouses = warehouses.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Warehouses));
            if (_pendingWarehouseId is > 0)
                SelectedDefaultWarehouse = Warehouses.FirstOrDefault(w => w.Id == _pendingWarehouseId);
            else if (!_isEditMode)
                SelectedDefaultWarehouse = Warehouses.FirstOrDefault(w => w.Code == DefaultWarehouseCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load warehouses for product form");
        }

        try
        {
            var accounts = await _getAccountSettings.ExecuteAsync(ct);
            AccountSettings = accounts.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(AccountSettings));
            if (_pendingStockAccountId is > 0)
                SelectedStockAccount = AccountSettings.FirstOrDefault(a => a.Id == _pendingStockAccountId);
            else if (!_isEditMode)
                SelectedStockAccount = AccountSettings.FirstOrDefault(a => a.Code == DefaultStockAccountCode);

            if (_pendingRevenueAccountId is > 0)
                SelectedRevenueAccount = AccountSettings.FirstOrDefault(a => a.Id == _pendingRevenueAccountId);
            else if (!_isEditMode)
                SelectedRevenueAccount = AccountSettings.FirstOrDefault(a => a.Code == DefaultRevenueAccountCode);

            if (_pendingDiscountAccountId is > 0)
                SelectedDiscountAccount = AccountSettings.FirstOrDefault(a => a.Id == _pendingDiscountAccountId);
            else if (!_isEditMode)
                SelectedDiscountAccount = AccountSettings.FirstOrDefault(a => a.Code == DefaultDiscountAccountCode);

            if (_pendingPriceReductionAccountId is > 0)
                SelectedPriceReductionAccount = AccountSettings.FirstOrDefault(a => a.Id == _pendingPriceReductionAccountId);
            else if (!_isEditMode)
                SelectedPriceReductionAccount = AccountSettings.FirstOrDefault(a => a.Code == DefaultPriceReductionAccountCode);

            if (_pendingReturnAccountId is > 0)
                SelectedReturnAccount = AccountSettings.FirstOrDefault(a => a.Id == _pendingReturnAccountId);
            else if (!_isEditMode)
                SelectedReturnAccount = AccountSettings.FirstOrDefault(a => a.Code == DefaultReturnAccountCode);

            if (_pendingCostAccountId is > 0)
                SelectedCostAccount = AccountSettings.FirstOrDefault(a => a.Id == _pendingCostAccountId);
            else if (!_isEditMode)
                SelectedCostAccount = AccountSettings.FirstOrDefault(a => a.Code == DefaultCostAccountCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load account settings for product form");
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
    private async Task AddProductUnitAsync(CancellationToken ct = default)
    {
        var before = ProductUnits.Select(u => u.Id).ToHashSet();
        var window = _productUnitFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var units = await _getProductUnits.ExecuteAsync(ct);
            ProductUnits = units.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(ProductUnits));
            var newItem = ProductUnits.FirstOrDefault(u => !before.Contains(u.Id));
            if (newItem is not null) SelectedProductUnit = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload product units after add"); }
    }

    [RelayCommand]
    private async Task AddWarehouseAsync(CancellationToken ct = default)
    {
        var before = Warehouses.Select(w => w.Id).ToHashSet();
        var window = _warehouseFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var warehouses = await _getWarehouses.ExecuteAsync(ct);
            Warehouses = warehouses.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Warehouses));
            var newItem = Warehouses.FirstOrDefault(w => !before.Contains(w.Id));
            if (newItem is not null) SelectedDefaultWarehouse = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload warehouses after add"); }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        if (await PersistAsync(ct))
        {
            StopDirtyTracking();
            RequestClose?.Invoke(true);
        }
    }

    [RelayCommand]
    private async Task SaveAndAddNewAsync(CancellationToken ct = default)
    {
        if (await PersistAsync(ct))
        {
            StopDirtyTracking();
            Initialize(null);
        }
    }

    private async Task<bool> PersistAsync(CancellationToken ct)
    {
        ErrorMessage = string.Empty;

        IsLoading = true;
        try
        {
            if (!_isEditMode)
            {
                var input = new CreateProductInput
                {
                    Code             = Code.Trim(),
                    Name             = Name.Trim(),
                    CategoryId       = SelectedCategory?.Id,
                    Unit             = (SelectedProductUnit?.Name ?? Unit).Trim(),
                    CostPrice        = CostPrice,
                    SellingPrice     = SellingPrice,
                    StockQuantity    = StockQuantity,
                    IsActive         = IsActive,
                    VatRate          = VatRate,
                    TaxReductionType = TaxReductionType,
                    ImportTaxRate    = ImportTaxRate,
                    ExportTaxRate    = ExportTaxRate,
                    ExciseTaxGroup   = ExciseTaxGroup,

                    Nature              = Nature,
                    Description         = Description,
                    ProductUnitId       = SelectedProductUnit?.Id,
                    WarrantyPeriod      = WarrantyPeriod,
                    MinStockQuantity    = MinStockQuantity,
                    Origin              = Origin,
                    PurchaseDescription = PurchaseDescription,
                    SaleDescription     = SaleDescription,

                    DefaultWarehouseId      = SelectedDefaultWarehouse?.Id,
                    StockAccountId          = SelectedStockAccount?.Id,
                    RevenueAccountId        = SelectedRevenueAccount?.Id,
                    DiscountAccountId       = SelectedDiscountAccount?.Id,
                    PriceReductionAccountId = SelectedPriceReductionAccount?.Id,
                    ReturnAccountId         = SelectedReturnAccount?.Id,
                    CostAccountId           = SelectedCostAccount?.Id,
                    TradeDiscountRate       = TradeDiscountRate,
                    SpecialGoodsType        = SpecialGoodsType,
                    LatestPurchasePrice     = LatestPurchasePrice,
                    IsPromotionalGood       = IsPromotionalGood,
                    IsDepositProduct        = IsDepositProduct,
                };
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateProductInput
                {
                    Id               = _editingId,
                    Code             = Code.Trim(),
                    Name             = Name.Trim(),
                    CategoryId       = SelectedCategory?.Id,
                    Unit             = (SelectedProductUnit?.Name ?? Unit).Trim(),
                    CostPrice        = CostPrice,
                    SellingPrice     = SellingPrice,
                    StockQuantity    = StockQuantity,
                    IsActive         = IsActive,
                    VatRate          = VatRate,
                    TaxReductionType = TaxReductionType,
                    ImportTaxRate    = ImportTaxRate,
                    ExportTaxRate    = ExportTaxRate,
                    ExciseTaxGroup   = ExciseTaxGroup,

                    Nature              = Nature,
                    Description         = Description,
                    ProductUnitId       = SelectedProductUnit?.Id,
                    WarrantyPeriod      = WarrantyPeriod,
                    MinStockQuantity    = MinStockQuantity,
                    Origin              = Origin,
                    PurchaseDescription = PurchaseDescription,
                    SaleDescription     = SaleDescription,

                    DefaultWarehouseId      = SelectedDefaultWarehouse?.Id,
                    StockAccountId          = SelectedStockAccount?.Id,
                    RevenueAccountId        = SelectedRevenueAccount?.Id,
                    DiscountAccountId       = SelectedDiscountAccount?.Id,
                    PriceReductionAccountId = SelectedPriceReductionAccount?.Id,
                    ReturnAccountId         = SelectedReturnAccount?.Id,
                    CostAccountId           = SelectedCostAccount?.Id,
                    TradeDiscountRate       = TradeDiscountRate,
                    SpecialGoodsType        = SpecialGoodsType,
                    LatestPurchasePrice     = LatestPurchasePrice,
                    IsPromotionalGood       = IsPromotionalGood,
                    IsDepositProduct        = IsDepositProduct,
                };
                await _updateUseCase.ExecuteAsync(input, ct);
            }
            return true;
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lưu thất bại: {ex.Message}";
            return false;
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
