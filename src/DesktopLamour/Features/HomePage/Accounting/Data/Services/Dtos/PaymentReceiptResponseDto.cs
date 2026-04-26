// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class PaymentReceiptResponseDto
{
    [JsonPropertyName("id")]              public int      Id             { get; set; }
    [JsonPropertyName("receipt_number")]  public string   ReceiptNumber  { get; set; } = "";
    [JsonPropertyName("customer_id")]     public int      CustomerId     { get; set; }
    [JsonPropertyName("customer_name")]   public string   CustomerName   { get; set; } = "";
    [JsonPropertyName("employee_id")]     public int?     EmployeeId     { get; set; }
    [JsonPropertyName("employee_name")]   public string?  EmployeeName   { get; set; }
    [JsonPropertyName("collection_date")] public DateTime CollectionDate { get; set; }
    [JsonPropertyName("total_amount")]    public decimal  TotalAmount    { get; set; }
    [JsonPropertyName("payment_method")]  public string   PaymentMethod  { get; set; } = "Cash";
    [JsonPropertyName("currency")]        public string   Currency       { get; set; } = "VND";
    [JsonPropertyName("exchange_rate")]   public decimal  ExchangeRate   { get; set; } = 1m;
    [JsonPropertyName("created_at")]      public DateTime CreatedAt      { get; set; }
}
