// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

public class CreateSalesOrderRequestDto
{
    [JsonPropertyName("document_number")]   public string   DocumentNumber { get; set; } = "";
    [JsonPropertyName("accounting_date")]   public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]     public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("customer_id")]       public int      CustomerId     { get; set; }
    [JsonPropertyName("customer_name_override")] public string? CustomerNameOverride { get; set; }
    [JsonPropertyName("customer_address_override")] public string? CustomerAddressOverride { get; set; }
    [JsonPropertyName("employee_id")]       public int?     EmployeeId     { get; set; }
    [JsonPropertyName("description")]       public string?  Description    { get; set; }
    [JsonPropertyName("reference")]         public string?  Reference      { get; set; }
    [JsonPropertyName("payment_terms")]     public string?  PaymentTerms   { get; set; }
    [JsonPropertyName("payment_due_days")]  public int?     PaymentDueDays { get; set; }
    [JsonPropertyName("payment_due_date")]  public DateTime? PaymentDueDate { get; set; }
    [JsonPropertyName("notes")]             public string?  Notes          { get; set; }
    [JsonPropertyName("delivery_method")]   public string?  DeliveryMethod { get; set; }
    [JsonPropertyName("payment_method")]    public string?  PaymentMethod  { get; set; }
    [JsonPropertyName("lines")]             public List<SalesOrderLineDto> Lines { get; set; } = new();
}
