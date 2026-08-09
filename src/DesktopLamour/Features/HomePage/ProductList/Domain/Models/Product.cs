// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.Models;

public class Product : ISearchableItem
{
    public int          Id               { get; set; }
    public string       Code             { get; set; } = string.Empty;
    public string       Name             { get; set; } = string.Empty;
    public int          CategoryId       { get; set; }
    public string       CategoryName     { get; set; } = string.Empty;
    public string       Unit             { get; set; } = string.Empty;
    public decimal      CostPrice        { get; set; }
    public decimal      SellingPrice     { get; set; }
    public int          StockQuantity    { get; set; }
    public bool         IsActive         { get; set; } = true;

    // Tax fields
    public VatRateType? VatRate          { get; set; }
    public TaxReductionStatus? TaxReductionType { get; set; }
    public decimal?     ImportTaxRate    { get; set; }
    public decimal?     ExportTaxRate    { get; set; }
    public string?      ExciseTaxGroup   { get; set; }

    // Header — "Sửa Vật tư, hàng hoá, dịch vụ"
    public ProductNature Nature              { get; set; } = ProductNature.VatTuHangHoa;
    public string?        Description         { get; set; }
    public int?            ProductUnitId       { get; set; }
    public string?         ProductUnitName     { get; set; }
    public string?         WarrantyPeriod      { get; set; }
    public int             MinStockQuantity    { get; set; }
    public string?         Origin              { get; set; }
    public string?         PurchaseDescription { get; set; }
    public string?         SaleDescription     { get; set; }

    // Tab "Ngầm định"
    public int?    DefaultWarehouseId        { get; set; }
    public string? DefaultWarehouseName      { get; set; }
    public int?    StockAccountId            { get; set; }
    public string? StockAccountCode          { get; set; }
    public int?    RevenueAccountId          { get; set; }
    public string? RevenueAccountCode        { get; set; }
    public int?    DiscountAccountId         { get; set; }
    public string? DiscountAccountCode       { get; set; }
    public int?    PriceReductionAccountId   { get; set; }
    public string? PriceReductionAccountCode { get; set; }
    public int?    ReturnAccountId           { get; set; }
    public string? ReturnAccountCode         { get; set; }
    public int?    CostAccountId             { get; set; }
    public string? CostAccountCode           { get; set; }
    public decimal TradeDiscountRate         { get; set; }
    public string? SpecialGoodsType          { get; set; }
    public decimal LatestPurchasePrice       { get; set; }
    public bool    IsPromotionalGood         { get; set; }

    public string DisplayText => $"{Code} — {Name}";
}
