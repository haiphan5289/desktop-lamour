// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

public class CreateDepositDeductionRequestDto
{
    [JsonPropertyName("sales_order_id")]   public int      SalesOrderId   { get; set; }
    [JsonPropertyName("amount")]           public decimal  Amount         { get; set; }
    [JsonPropertyName("accounting_date")]  public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]    public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("description")]      public string?  Description    { get; set; }
}
