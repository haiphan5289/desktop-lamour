// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

public class WarehouseReceiptLineDto
{
    [JsonPropertyName("id")]               public int     Id            { get; set; }
    [JsonPropertyName("product_id")]       public int     ProductId     { get; set; }
    [JsonPropertyName("product_code")]     public string  ProductCode   { get; set; } = "";
    [JsonPropertyName("product_name")]     public string  ProductName   { get; set; } = "";
    [JsonPropertyName("warehouse_id")]     public int     WarehouseId   { get; set; }
    [JsonPropertyName("warehouse_name")]   public string  WarehouseName { get; set; } = "";
    [JsonPropertyName("quantity")]         public decimal Quantity      { get; set; }
    [JsonPropertyName("unit_price")]       public decimal UnitPrice     { get; set; }
    [JsonPropertyName("amount")]           public decimal Amount        { get; set; }
    [JsonPropertyName("debit_account")]    public string  DebitAccount  { get; set; } = "156";
    [JsonPropertyName("credit_account")]   public string  CreditAccount { get; set; } = "331";
    [JsonPropertyName("cost_item")]              public string? CostItem              { get; set; }
    [JsonPropertyName("cost_object")]            public string? CostObject            { get; set; }
    [JsonPropertyName("project")]                public string? Project               { get; set; }
    [JsonPropertyName("purchase_order_number")]  public string? PurchaseOrderNumber   { get; set; }
    [JsonPropertyName("sales_contract_number")]  public string? SalesContractNumber   { get; set; }
    [JsonPropertyName("loan_contract_number")]   public string? LoanContractNumber    { get; set; }
    [JsonPropertyName("statistics_code")]        public string? StatisticsCode        { get; set; }
}

public class WarehouseReceiptResponseDto
{
    [JsonPropertyName("id")]               public int      Id             { get; set; }
    [JsonPropertyName("receipt_number")]   public string   ReceiptNumber  { get; set; } = "";
    [JsonPropertyName("receipt_type")]     public int      ReceiptType    { get; set; }
    [JsonPropertyName("status")]           public string   Status         { get; set; } = "Draft";
    [JsonPropertyName("customer_id")]      public int?     CustomerId     { get; set; }
    [JsonPropertyName("customer_name")]    public string?  CustomerName   { get; set; }
    [JsonPropertyName("supplier_id")]      public int?     SupplierId     { get; set; }
    [JsonPropertyName("supplier_name")]    public string?  SupplierName   { get; set; }
    [JsonPropertyName("employee_id")]      public int?     EmployeeId     { get; set; }
    [JsonPropertyName("employee_name")]    public string?  EmployeeName   { get; set; }
    [JsonPropertyName("accounting_date")]  public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]    public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("description")]      public string?  Description    { get; set; }
    [JsonPropertyName("delivery_person")]  public string?  DeliveryPerson { get; set; }
    [JsonPropertyName("reference")]        public string?  Reference      { get; set; }
    [JsonPropertyName("total_amount")]     public decimal  TotalAmount    { get; set; }
    [JsonPropertyName("created_at")]       public DateTime CreatedAt      { get; set; }
    [JsonPropertyName("confirmed_at")]     public DateTime? ConfirmedAt   { get; set; }
    [JsonPropertyName("lines")]            public List<WarehouseReceiptLineDto> Lines { get; set; } = new();
}

public class CreateWarehouseReceiptLineDto
{
    [JsonPropertyName("product_id")]       public int     ProductId     { get; set; }
    [JsonPropertyName("warehouse_id")]     public int     WarehouseId   { get; set; }
    [JsonPropertyName("quantity")]         public decimal Quantity      { get; set; }
    [JsonPropertyName("unit_price")]       public decimal UnitPrice     { get; set; }
    [JsonPropertyName("amount")]           public decimal Amount        { get; set; }
    [JsonPropertyName("debit_account")]    public string  DebitAccount  { get; set; } = "156";
    [JsonPropertyName("credit_account")]   public string  CreditAccount { get; set; } = "331";
    [JsonPropertyName("cost_item")]              public string? CostItem              { get; set; }
    [JsonPropertyName("cost_object")]            public string? CostObject            { get; set; }
    [JsonPropertyName("project")]                public string? Project               { get; set; }
    [JsonPropertyName("purchase_order_number")]  public string? PurchaseOrderNumber   { get; set; }
    [JsonPropertyName("sales_contract_number")]  public string? SalesContractNumber   { get; set; }
    [JsonPropertyName("loan_contract_number")]   public string? LoanContractNumber    { get; set; }
    [JsonPropertyName("statistics_code")]        public string? StatisticsCode        { get; set; }
}

public class CreateWarehouseReceiptRequestDto
{
    [JsonPropertyName("receipt_type")]     public int      ReceiptType    { get; set; }
    [JsonPropertyName("customer_id")]      public int?     CustomerId     { get; set; }
    [JsonPropertyName("supplier_id")]      public int?     SupplierId     { get; set; }
    [JsonPropertyName("employee_id")]      public int?     EmployeeId     { get; set; }
    [JsonPropertyName("accounting_date")]  public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]    public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("description")]      public string?  Description    { get; set; }
    [JsonPropertyName("delivery_person")]  public string?  DeliveryPerson { get; set; }
    [JsonPropertyName("reference")]        public string?  Reference      { get; set; }
    [JsonPropertyName("lines")]            public List<CreateWarehouseReceiptLineDto> Lines { get; set; } = new();
}
