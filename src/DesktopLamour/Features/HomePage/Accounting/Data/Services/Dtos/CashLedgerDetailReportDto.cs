// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

// Báo cáo "Sổ kế toán chi tiết quỹ tiền mặt" — mỗi dòng là 1 dòng hạch toán (BE: reports/cash-ledger-detail).
public class CashLedgerDetailReportDto
{
    [JsonPropertyName("opening_balance")] public decimal OpeningBalance { get; set; }
    [JsonPropertyName("closing_balance")] public decimal ClosingBalance { get; set; }
    [JsonPropertyName("total_debit")]     public decimal TotalDebit     { get; set; }
    [JsonPropertyName("total_credit")]    public decimal TotalCredit    { get; set; }
    [JsonPropertyName("rows")]            public List<CashLedgerDetailRowDto> Rows { get; set; } = new();
}

public class CashLedgerDetailRowDto
{
    [JsonPropertyName("accounting_date")] public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]   public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("receipt_number")]  public string?  ReceiptNumber  { get; set; }
    [JsonPropertyName("payment_number")]  public string?  PaymentNumber  { get; set; }
    [JsonPropertyName("description")]     public string   Description    { get; set; } = "";
    [JsonPropertyName("account")]         public string   Account        { get; set; } = "";
    [JsonPropertyName("counter_account")] public string   CounterAccount { get; set; } = "";
    [JsonPropertyName("debit_amount")]    public decimal  DebitAmount    { get; set; }
    [JsonPropertyName("credit_amount")]   public decimal  CreditAmount   { get; set; }
    [JsonPropertyName("balance")]         public decimal  Balance        { get; set; }
    [JsonPropertyName("person_name")]     public string?  PersonName     { get; set; }
    [JsonPropertyName("category_code")]   public string?  CategoryCode   { get; set; }
    [JsonPropertyName("category_name")]   public string?  CategoryName   { get; set; }
    // null = dòng sổ quỹ cũ không còn phiếu gốc → số chứng từ không bấm được.
    [JsonPropertyName("receipt_id")]      public int?     ReceiptId      { get; set; }
    [JsonPropertyName("payment_id")]      public int?     PaymentId      { get; set; }
    [JsonPropertyName("is_bulk_receipt")] public bool     IsBulkReceipt  { get; set; }
}
