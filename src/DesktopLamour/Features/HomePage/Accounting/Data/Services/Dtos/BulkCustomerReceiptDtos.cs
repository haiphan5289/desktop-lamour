// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class OutstandingSalesOrderDto
{
    [JsonPropertyName("sales_order_id")]   public int      SalesOrderId    { get; set; }
    [JsonPropertyName("document_number")]  public string   DocumentNumber  { get; set; } = "";
    [JsonPropertyName("accounting_date")]  public DateTime AccountingDate  { get; set; }
    [JsonPropertyName("document_date")]    public DateTime DocumentDate    { get; set; }
    [JsonPropertyName("customer_id")]      public int      CustomerId      { get; set; }
    [JsonPropertyName("customer_code")]    public string   CustomerCode    { get; set; } = "";
    [JsonPropertyName("customer_name")]    public string   CustomerName    { get; set; } = "";
    [JsonPropertyName("description")]      public string?  Description     { get; set; }
    [JsonPropertyName("remaining_amount")] public decimal  RemainingAmount { get; set; }
}

public class BulkReceiptLineRequestDto
{
    [JsonPropertyName("sales_order_id")] public int     SalesOrderId { get; set; }
    [JsonPropertyName("amount")]         public decimal Amount       { get; set; }
}

public class CreateBulkCustomerReceiptRequestDto
{
    [JsonPropertyName("accounting_date")]       public DateTime AccountingDate      { get; set; }
    [JsonPropertyName("document_date")]         public DateTime DocumentDate        { get; set; }
    [JsonPropertyName("debit_account")]         public string   DebitAccount        { get; set; } = "Cash111";
    [JsonPropertyName("bank_account")]          public string?  BankAccount         { get; set; }
    [JsonPropertyName("collector_employee_id")] public int?     CollectorEmployeeId { get; set; }
    [JsonPropertyName("lines")]                 public List<BulkReceiptLineRequestDto> Lines { get; set; } = new();
}

public class CreateBulkCustomerReceiptResponseDto
{
    [JsonPropertyName("receipts")] public List<ReceiptResponseDto> Receipts { get; set; } = new();
}
