// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class CreatePaymentReceiptRequestDto
{
    [JsonPropertyName("customer_id")]     public int      CustomerId     { get; set; }
    [JsonPropertyName("employee_id")]     public int?     EmployeeId     { get; set; }
    [JsonPropertyName("collection_date")] public DateTime CollectionDate { get; set; }
    [JsonPropertyName("description")]     public string?  Description    { get; set; }
    [JsonPropertyName("total_amount")]    public decimal  TotalAmount    { get; set; }
    [JsonPropertyName("payment_method")]  public string   PaymentMethod  { get; set; } = "Cash";
    [JsonPropertyName("currency")]        public string   Currency       { get; set; } = "VND";
    [JsonPropertyName("exchange_rate")]   public decimal  ExchangeRate   { get; set; } = 1m;
    [JsonPropertyName("lines")]           public List<CreatePaymentReceiptLineDtoRequest> Lines { get; set; } = new();
}

public class CreatePaymentReceiptLineDtoRequest
{
    [JsonPropertyName("document_date")]   public DateTime  DocumentDate   { get; set; }
    [JsonPropertyName("document_number")] public string    DocumentNumber { get; set; } = "";
    [JsonPropertyName("invoice_number")]  public string    InvoiceNumber  { get; set; } = "";
    [JsonPropertyName("description")]     public string    Description    { get; set; } = "";
    [JsonPropertyName("due_date")]        public DateTime? DueDate        { get; set; }
    [JsonPropertyName("amount_due")]      public decimal   AmountDue      { get; set; }
    [JsonPropertyName("amount_paid")]     public decimal   AmountPaid     { get; set; }
}
