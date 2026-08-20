// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

public class WarehouseTransactionResponseDto
{
    [JsonPropertyName("id")]                   public int      Id                 { get; set; }
    [JsonPropertyName("transaction_type")]     public string   TransactionType    { get; set; } = "";
    [JsonPropertyName("document_number")]      public string   DocumentNumber     { get; set; } = "";
    [JsonPropertyName("accounting_date")]      public DateTime AccountingDate     { get; set; }
    [JsonPropertyName("document_date")]        public DateTime DocumentDate       { get; set; }
    [JsonPropertyName("description")]          public string?  Description        { get; set; }
    [JsonPropertyName("total_amount")]         public decimal  TotalAmount        { get; set; }
    [JsonPropertyName("delivery_or_receiver")]  public string?  DeliveryOrReceiver { get; set; }
    [JsonPropertyName("object_name")]          public string?  ObjectName         { get; set; }
    [JsonPropertyName("has_sales_order")]      public bool     HasSalesOrder      { get; set; }
    [JsonPropertyName("is_held")]              public bool     IsHeld             { get; set; }
    [JsonPropertyName("ledger_date")]          public DateTime LedgerDate         { get; set; }
    [JsonPropertyName("document_type_label")]  public string   DocumentTypeLabel  { get; set; } = "";
    [JsonPropertyName("lines")]                public List<WarehouseTransactionLineDto> Lines { get; set; } = new();
}

public class WarehouseTransactionLineDto
{
    [JsonPropertyName("product_code")]   public string  ProductCode   { get; set; } = "";
    [JsonPropertyName("product_name")]   public string  ProductName   { get; set; } = "";
    [JsonPropertyName("warehouse_name")] public string  WarehouseName { get; set; } = "";
    [JsonPropertyName("debit_account")]  public string  DebitAccount  { get; set; } = "";
    [JsonPropertyName("credit_account")] public string  CreditAccount { get; set; } = "";
    [JsonPropertyName("unit")]           public string  Unit          { get; set; } = "";
    [JsonPropertyName("quantity")]       public decimal Quantity      { get; set; }
    [JsonPropertyName("unit_price")]     public decimal UnitPrice     { get; set; }
    [JsonPropertyName("amount")]         public decimal Amount        { get; set; }
}
