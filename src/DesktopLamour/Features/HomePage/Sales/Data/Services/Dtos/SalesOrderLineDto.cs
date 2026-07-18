// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

public class SalesOrderLineDto
{
    [JsonPropertyName("id")]                  public int     Id                { get; set; }
    [JsonPropertyName("product_id")]          public int     ProductId         { get; set; }
    [JsonPropertyName("product_code")]        public string  ProductCode       { get; set; } = "";
    [JsonPropertyName("product_name")]        public string  ProductName       { get; set; } = "";
    [JsonPropertyName("is_promotion")]        public bool    IsPromotion       { get; set; }
    [JsonPropertyName("unit")]                public string  Unit              { get; set; } = "";
    [JsonPropertyName("quantity")]            public int     Quantity          { get; set; }
    [JsonPropertyName("unit_price")]           public decimal UnitPrice         { get; set; }
    [JsonPropertyName("discount_rate")]        public decimal DiscountRate      { get; set; }
    [JsonPropertyName("amount")]               public decimal Amount            { get; set; }
    [JsonPropertyName("tax_rate")]             public decimal TaxRate           { get; set; }
    [JsonPropertyName("tax_amount")]           public decimal TaxAmount         { get; set; }
    [JsonPropertyName("receivable_account")]  public string  ReceivableAccount { get; set; } = "131";
    [JsonPropertyName("revenue_account")]     public string  RevenueAccount    { get; set; } = "511";
}
