// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

public class SalesReturnLineDto
{
    [JsonPropertyName("id")]                  public int     Id              { get; set; }
    [JsonPropertyName("product_id")]          public int     ProductId       { get; set; }
    [JsonPropertyName("warehouse_id")]         public int     WarehouseId     { get; set; }
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

    [JsonPropertyName("tax_rate")]            public decimal TaxRate         { get; set; }
    [JsonPropertyName("tax_amount")]          public decimal TaxAmount       { get; set; }
    [JsonPropertyName("tax_account")]         public string  TaxAccount      { get; set; } = "33311";

    [JsonPropertyName("cost_account")]        public string  CostAccount     { get; set; } = "1561";
    [JsonPropertyName("cogs_account")]        public string  CogsAccount     { get; set; } = "632";
    [JsonPropertyName("cost_price")]          public decimal CostPrice       { get; set; }
    [JsonPropertyName("cost_amount")]         public decimal CostAmount      { get; set; }

    [JsonPropertyName("department_id")]       public int?    DepartmentId    { get; set; }
    [JsonPropertyName("department_name")]     public string? DepartmentName  { get; set; }
}
