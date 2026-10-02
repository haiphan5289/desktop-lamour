// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class ReceiptEntryDto
{
    [JsonPropertyName("id")]             public int     Id            { get; set; }
    [JsonPropertyName("description")]    public string  Description   { get; set; } = "";
    [JsonPropertyName("debit_account_id")]           public int     DebitAccountId           { get; set; }
    [JsonPropertyName("debit_account_code")]         public string? DebitAccountCode         { get; set; }
    [JsonPropertyName("debit_account_description")]  public string? DebitAccountDescription  { get; set; }
    [JsonPropertyName("credit_account_id")]          public int     CreditAccountId          { get; set; }
    [JsonPropertyName("credit_account_code")]        public string? CreditAccountCode        { get; set; }
    [JsonPropertyName("credit_account_description")] public string? CreditAccountDescription { get; set; }
    // Chỉ Phiếu thu hàng loạt GỬI LÊN 2 field này ("Cash111" | "Bank112" | "Receivable131") — màn đó chọn
    // "Tiền mặt / Tiền gửi" chứ không có danh mục TK; BE tự tra sang danh mục khi *_account_id = 0.
    // Response không trả về (luôn null) — đọc *_account_code.
    [JsonPropertyName("debit_account")]  public string? DebitAccount  { get; set; }
    [JsonPropertyName("credit_account")] public string? CreditAccount { get; set; }
    [JsonPropertyName("amount")]         public decimal Amount        { get; set; }
    [JsonPropertyName("subject_code")]   public string? SubjectCode   { get; set; }
    [JsonPropertyName("subject_name")]   public string? SubjectName   { get; set; }
    [JsonPropertyName("bank_account")]   public string? BankAccount   { get; set; }
    [JsonPropertyName("sales_order_id")] public int?    SalesOrderId  { get; set; }
}
