// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

public class DepositDeductionResponseDto
{
    [JsonPropertyName("id")]                          public int      Id                       { get; set; }
    [JsonPropertyName("document_number")]             public string   DocumentNumber           { get; set; } = "";
    [JsonPropertyName("deposit_id")]                  public int      DepositId                { get; set; }
    [JsonPropertyName("deposit_document_number")]     public string   DepositDocumentNumber    { get; set; } = "";
    [JsonPropertyName("sales_order_id")]               public int      SalesOrderId             { get; set; }
    [JsonPropertyName("sales_order_document_number")] public string   SalesOrderDocumentNumber { get; set; } = "";
    [JsonPropertyName("customer_id")]                  public int      CustomerId               { get; set; }
    [JsonPropertyName("customer_name")]                public string   CustomerName             { get; set; } = "";
    [JsonPropertyName("employee_id")]                  public int?     EmployeeId               { get; set; }
    [JsonPropertyName("employee_name")]                public string?  EmployeeName             { get; set; }
    [JsonPropertyName("amount")]                       public decimal  Amount                   { get; set; }
    [JsonPropertyName("accounting_date")]              public DateTime AccountingDate           { get; set; }
    [JsonPropertyName("document_date")]                public DateTime DocumentDate             { get; set; }
    [JsonPropertyName("description")]                  public string?  Description              { get; set; }
    [JsonPropertyName("created_at")]                   public DateTime CreatedAt                { get; set; }
}
