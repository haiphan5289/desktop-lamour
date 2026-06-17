// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

public class SalesReturnLineDto
{
    [JsonPropertyName("id")]                  public int     Id              { get; set; }
    [JsonPropertyName("product_id")]          public int     ProductId       { get; set; }
    [JsonPropertyName("product_code")]        public string  ProductCode     { get; set; } = "";
    [JsonPropertyName("product_name")]        public string  ProductName     { get; set; } = "";
    [JsonPropertyName("return_account")]      public string  ReturnAccount   { get; set; } = "5212";
    [JsonPropertyName("debt_account")]        public string  DebtAccount     { get; set; } = "131";
    [JsonPropertyName("discount_account")]    public string  DiscountAccount { get; set; } = "5211";
    [JsonPropertyName("unit")]                public string  Unit            { get; set; } = "";
    [JsonPropertyName("quantity")]            public int     Quantity        { get; set; }
    [JsonPropertyName("unit_price")]          public decimal UnitPrice       { get; set; }
    [JsonPropertyName("amount")]              public decimal Amount          { get; set; }
    [JsonPropertyName("discount_rate")]       public decimal DiscountRate    { get; set; }
    [JsonPropertyName("discount_amount")]     public decimal DiscountAmount  { get; set; }
    [JsonPropertyName("sales_order_number")]  public string? SalesOrderNumber { get; set; }
}
