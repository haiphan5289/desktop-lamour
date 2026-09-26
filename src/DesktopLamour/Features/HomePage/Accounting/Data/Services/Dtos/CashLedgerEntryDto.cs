// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class CashLedgerEntryDto
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
    [JsonPropertyName("amount")]          public decimal  Amount         { get; set; }
    [JsonPropertyName("balance")]         public decimal  Balance        { get; set; }
    [JsonPropertyName("person_name")]     public string?  PersonName     { get; set; }
    [JsonPropertyName("payment_reason")]  public string?  PaymentReason  { get; set; }
    [JsonPropertyName("document_type")]   public string   DocumentType   { get; set; } = "";
    [JsonPropertyName("status")]          public string   Status         { get; set; } = "Confirmed";

    // 2026-09-26: dòng chưa ghi sổ ("Treo", hoặc "Draft" dữ liệu cũ) — lưới Quỹ tô màu giống danh
    // sách Chứng từ bán hàng (SalesOrderListItem.IsHeld). Không gửi/nhận qua API.
    [JsonIgnore] public bool IsHeld => Status != "Confirmed";
    // 2026-09-26: id phiếu gốc (BE tra theo số chứng từ) — để Ghi sổ/Bỏ ghi/Xóa/Sửa thẳng từ màn Quỹ.
    [JsonPropertyName("receipt_id")]      public int?     ReceiptId      { get; set; }
    [JsonPropertyName("payment_id")]      public int?     PaymentId      { get; set; }
    // true = phiếu thu hàng loạt → màn Quỹ mở BulkCustomerReceiptWindow thay vì ReceiptWindow.
    [JsonPropertyName("is_bulk_receipt")] public bool IsBulkReceipt { get; set; }
}
