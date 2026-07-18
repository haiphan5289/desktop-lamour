// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

public class SalesOrderSummaryLineDto
{
    [JsonPropertyName("product_id")]       public int      ProductId      { get; set; }
    [JsonPropertyName("product_code")]     public string   ProductCode    { get; set; } = "";
    [JsonPropertyName("product_name")]     public string   ProductName    { get; set; } = "";
    [JsonPropertyName("unit")]             public string   Unit           { get; set; } = "";
    [JsonPropertyName("customer_id")]      public int      CustomerId     { get; set; }
    [JsonPropertyName("customer_code")]    public string   CustomerCode   { get; set; } = "";
    [JsonPropertyName("customer_name")]    public string   CustomerName   { get; set; } = "";
    [JsonPropertyName("employee_id")]      public int?     EmployeeId     { get; set; }
    [JsonPropertyName("employee_code")]    public string?  EmployeeCode   { get; set; }
    [JsonPropertyName("employee_name")]    public string?  EmployeeName   { get; set; }
    [JsonPropertyName("quantity_sold")]    public int      QuantitySold   { get; set; }
    [JsonPropertyName("sales_amount")]     public decimal  SalesAmount    { get; set; }
    [JsonPropertyName("discount_amount")]  public decimal  DiscountAmount { get; set; }
    [JsonPropertyName("return_quantity")]  public int      ReturnQuantity { get; set; }
    [JsonPropertyName("return_value")]     public decimal  ReturnValue    { get; set; }
    [JsonPropertyName("net_revenue")]      public decimal  NetRevenue     { get; set; }
}
