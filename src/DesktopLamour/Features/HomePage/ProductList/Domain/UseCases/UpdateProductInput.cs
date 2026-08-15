// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public sealed record UpdateProductInput
{
    public required int     Id            { get; init; }
    public required string  Code          { get; init; }
    public required string  Name          { get; init; }
    public int?             CategoryId    { get; init; }
    public required string  Unit          { get; init; }
    public required decimal CostPrice     { get; init; }
    public required decimal SellingPrice  { get; init; }
    public required int     StockQuantity { get; init; }
    public required bool    IsActive      { get; init; }

    public VatRateType?       VatRate          { get; init; }
    public TaxReductionStatus? TaxReductionType { get; init; }
    public decimal?           ImportTaxRate    { get; init; }
    public decimal?           ExportTaxRate    { get; init; }
    public string?            ExciseTaxGroup   { get; init; }

    // Header — "Sửa Vật tư, hàng hoá, dịch vụ"
    public ProductNature Nature              { get; init; } = ProductNature.VatTuHangHoa;
    public string?        Description         { get; init; }
    public int?            ProductUnitId       { get; init; }
    public string?         WarrantyPeriod      { get; init; }
    public int             MinStockQuantity    { get; init; }
    public string?         Origin              { get; init; }
    public string?         PurchaseDescription { get; init; }
    public string?         SaleDescription     { get; init; }

    // Tab "Ngầm định"
    public int?    DefaultWarehouseId      { get; init; }
    public int?    StockAccountId          { get; init; }
    public int?    RevenueAccountId        { get; init; }
    public int?    DiscountAccountId       { get; init; }
    public int?    PriceReductionAccountId { get; init; }
    public int?    ReturnAccountId         { get; init; }
    public int?    CostAccountId           { get; init; }
    public decimal TradeDiscountRate       { get; init; }
    public string? SpecialGoodsType        { get; init; }
    public decimal LatestPurchasePrice     { get; init; }
    public bool    IsPromotionalGood       { get; init; }
    public bool    IsDepositProduct        { get; init; }
}
