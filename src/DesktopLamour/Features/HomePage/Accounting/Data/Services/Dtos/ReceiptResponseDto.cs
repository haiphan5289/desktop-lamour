// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class ReceiptResponseDto
{
    [JsonPropertyName("id")]                      public int      Id                    { get; set; }
    [JsonPropertyName("customer_id")]             public int?     CustomerId            { get; set; }
    [JsonPropertyName("customer_name")]           public string   CustomerName          { get; set; } = "";
    [JsonPropertyName("payer_name")]              public string   PayerName             { get; set; } = "";
    [JsonPropertyName("address")]                 public string?  Address               { get; set; }
    [JsonPropertyName("payment_reason")]          public string   PaymentReason         { get; set; } = "";
    [JsonPropertyName("collector_employee_id")]   public int?     CollectorEmployeeId   { get; set; }
    [JsonPropertyName("collector_employee_name")] public string?  CollectorEmployeeName { get; set; }
    [JsonPropertyName("attachment")]              public string?  Attachment            { get; set; }
    [JsonPropertyName("reference")]               public string?  Reference             { get; set; }
    [JsonPropertyName("accounting_date")]         public DateTime AccountingDate        { get; set; }
    [JsonPropertyName("document_date")]           public DateTime DocumentDate          { get; set; }
    [JsonPropertyName("document_number")]         public string   DocumentNumber        { get; set; } = "";
    [JsonPropertyName("created_at")]              public DateTime CreatedAt             { get; set; }
    [JsonPropertyName("entries")]                 public List<ReceiptEntryDto> Entries  { get; set; } = new();
}
