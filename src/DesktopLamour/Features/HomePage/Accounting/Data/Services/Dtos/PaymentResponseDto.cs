// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class PaymentResponseDto
{
    [JsonPropertyName("id")]                      public int      Id                    { get; set; }
    [JsonPropertyName("partner_type")]            public string   PartnerType           { get; set; } = "";
    [JsonPropertyName("partner_id")]              public int      PartnerId             { get; set; }
    [JsonPropertyName("partner_name")]            public string   PartnerName           { get; set; } = "";
    [JsonPropertyName("payee_name")]              public string   PayeeName             { get; set; } = "";
    [JsonPropertyName("address")]                 public string?  Address               { get; set; }
    [JsonPropertyName("payment_reason")]          public string   PaymentReason         { get; set; } = "";
    [JsonPropertyName("reason_detail")]            public string?  ReasonDetail          { get; set; }
    [JsonPropertyName("payment_employee_id")]     public int?     PaymentEmployeeId     { get; set; }
    [JsonPropertyName("payment_employee_name")]   public string?  PaymentEmployeeName   { get; set; }
    [JsonPropertyName("attachment")]              public string?  Attachment            { get; set; }
    [JsonPropertyName("reference")]               public string?  Reference             { get; set; }
    [JsonPropertyName("accounting_date")]         public DateTime AccountingDate        { get; set; }
    [JsonPropertyName("document_date")]           public DateTime DocumentDate          { get; set; }
    [JsonPropertyName("document_number")]         public string   DocumentNumber        { get; set; } = "";
    [JsonPropertyName("status")]                  public string   Status                { get; set; } = "";
    [JsonPropertyName("created_at")]              public DateTime CreatedAt             { get; set; }
    [JsonPropertyName("confirmed_at")]             public DateTime? ConfirmedAt          { get; set; }
    [JsonPropertyName("entries")]                 public List<PaymentEntryDto> Entries  { get; set; } = new();
}
