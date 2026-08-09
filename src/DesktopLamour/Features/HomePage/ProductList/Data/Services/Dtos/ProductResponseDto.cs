// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;

public class ProductResponseDto
{
    [JsonPropertyName("id")]                 public int      Id               { get; set; }
    [JsonPropertyName("code")]               public string   Code             { get; set; } = string.Empty;
    [JsonPropertyName("name")]               public string   Name             { get; set; } = string.Empty;
    [JsonPropertyName("category_id")]        public int      CategoryId       { get; set; }
    [JsonPropertyName("category_name")]      public string   CategoryName     { get; set; } = string.Empty;
    [JsonPropertyName("unit")]               public string   Unit             { get; set; } = string.Empty;
    [JsonPropertyName("cost_price")]         public decimal  CostPrice        { get; set; }
    [JsonPropertyName("selling_price")]      public decimal  SellingPrice     { get; set; }
    [JsonPropertyName("stock_quantity")]     public int      StockQuantity    { get; set; }
    [JsonPropertyName("is_active")]          public bool     IsActive         { get; set; }
    [JsonPropertyName("vat_rate")]           public string?  VatRate          { get; set; }
    [JsonPropertyName("tax_reduction_type")] public string?  TaxReductionType { get; set; }
    [JsonPropertyName("import_tax_rate")]    public decimal? ImportTaxRate    { get; set; }
    [JsonPropertyName("export_tax_rate")]    public decimal? ExportTaxRate    { get; set; }
    [JsonPropertyName("excise_tax_group")]   public string?  ExciseTaxGroup   { get; set; }

    // Header — "Sửa Vật tư, hàng hoá, dịch vụ"
    [JsonPropertyName("nature")]               public string  Nature              { get; set; } = string.Empty;
    [JsonPropertyName("description")]          public string? Description         { get; set; }
    [JsonPropertyName("product_unit_id")]      public int?    ProductUnitId       { get; set; }
    [JsonPropertyName("product_unit_name")]    public string? ProductUnitName     { get; set; }
    [JsonPropertyName("warranty_period")]      public string? WarrantyPeriod      { get; set; }
    [JsonPropertyName("min_stock_quantity")]   public int     MinStockQuantity    { get; set; }
    [JsonPropertyName("origin")]               public string? Origin              { get; set; }
    [JsonPropertyName("purchase_description")] public string? PurchaseDescription { get; set; }
    [JsonPropertyName("sale_description")]     public string? SaleDescription     { get; set; }

    // Tab "Ngầm định"
    [JsonPropertyName("default_warehouse_id")]         public int?    DefaultWarehouseId        { get; set; }
    [JsonPropertyName("default_warehouse_name")]       public string? DefaultWarehouseName      { get; set; }
    [JsonPropertyName("stock_account_id")]             public int?    StockAccountId            { get; set; }
    [JsonPropertyName("stock_account_code")]           public string? StockAccountCode          { get; set; }
    [JsonPropertyName("revenue_account_id")]           public int?    RevenueAccountId          { get; set; }
    [JsonPropertyName("revenue_account_code")]         public string? RevenueAccountCode        { get; set; }
    [JsonPropertyName("discount_account_id")]          public int?    DiscountAccountId         { get; set; }
    [JsonPropertyName("discount_account_code")]        public string? DiscountAccountCode       { get; set; }
    [JsonPropertyName("price_reduction_account_id")]   public int?    PriceReductionAccountId   { get; set; }
    [JsonPropertyName("price_reduction_account_code")] public string? PriceReductionAccountCode { get; set; }
    [JsonPropertyName("return_account_id")]            public int?    ReturnAccountId           { get; set; }
    [JsonPropertyName("return_account_code")]          public string? ReturnAccountCode         { get; set; }
    [JsonPropertyName("cost_account_id")]              public int?    CostAccountId             { get; set; }
    [JsonPropertyName("cost_account_code")]            public string? CostAccountCode           { get; set; }
    [JsonPropertyName("trade_discount_rate")]          public decimal TradeDiscountRate         { get; set; }
    [JsonPropertyName("special_goods_type")]           public string? SpecialGoodsType          { get; set; }
    [JsonPropertyName("latest_purchase_price")]        public decimal LatestPurchasePrice       { get; set; }
    [JsonPropertyName("is_promotional_good")]          public bool    IsPromotionalGood         { get; set; }
}
