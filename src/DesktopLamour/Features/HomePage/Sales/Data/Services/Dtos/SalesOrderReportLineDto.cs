// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

public class SalesOrderReportLineDto
{
    [JsonPropertyName("order_id")]          public int      OrderId          { get; set; }
    [JsonPropertyName("document_number")]   public string   DocumentNumber   { get; set; } = "";
    [JsonPropertyName("accounting_date")]   public DateTime AccountingDate   { get; set; }
    [JsonPropertyName("customer_id")]       public int      CustomerId       { get; set; }
    [JsonPropertyName("customer_name")]     public string   CustomerName     { get; set; } = "";
    [JsonPropertyName("employee_id")]       public int?     EmployeeId       { get; set; }
    [JsonPropertyName("employee_name")]     public string?  EmployeeName     { get; set; }
    [JsonPropertyName("product_id")]        public int      ProductId        { get; set; }
    [JsonPropertyName("product_code")]      public string   ProductCode      { get; set; } = "";
    [JsonPropertyName("product_name")]      public string   ProductName      { get; set; } = "";
    [JsonPropertyName("unit")]              public string   Unit             { get; set; } = "";
    [JsonPropertyName("category")]          public string?  Category         { get; set; }
    [JsonPropertyName("quantity")]          public int      Quantity         { get; set; }
    [JsonPropertyName("unit_price")]        public decimal  UnitPrice        { get; set; }
    [JsonPropertyName("discount_rate")]     public decimal  DiscountRate     { get; set; }
    [JsonPropertyName("amount")]            public decimal  Amount           { get; set; }
    [JsonPropertyName("tax_rate")]          public decimal  TaxRate          { get; set; }
    [JsonPropertyName("tax_amount")]        public decimal  TaxAmount        { get; set; }
}
