// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class PaymentEntryDto
{
    [JsonPropertyName("id")]                  public int     Id                    { get; set; }
    [JsonPropertyName("description")]         public string  Description           { get; set; } = "";
    [JsonPropertyName("debit_account_id")]    public int     DebitAccountId         { get; set; }
    [JsonPropertyName("debit_account_code")]  public string? DebitAccountCode       { get; set; }
    [JsonPropertyName("debit_account_description")]  public string? DebitAccountDescription  { get; set; }
    [JsonPropertyName("credit_account_id")]   public int     CreditAccountId        { get; set; }
    [JsonPropertyName("credit_account_code")] public string? CreditAccountCode      { get; set; }
    [JsonPropertyName("credit_account_description")] public string? CreditAccountDescription { get; set; }
    [JsonPropertyName("amount")]              public decimal Amount                { get; set; }
    [JsonPropertyName("subject_code")]        public string? SubjectCode            { get; set; }
    [JsonPropertyName("subject_name")]        public string? SubjectName            { get; set; }
    [JsonPropertyName("bank_account")]        public string? BankAccount            { get; set; }
    [JsonPropertyName("expense_category_id")]   public int?    ExpenseCategoryId   { get; set; }
    [JsonPropertyName("expense_category_name")] public string? ExpenseCategoryName { get; set; }
}
