// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class UpdatePaymentRequestDto
{
    [JsonPropertyName("supplier_id")]           public int      SupplierId          { get; set; }
    [JsonPropertyName("payee_name")]            public string   PayeeName           { get; set; } = "";
    [JsonPropertyName("address")]               public string?  Address             { get; set; }
    [JsonPropertyName("payment_reason")]        public string   PaymentReason       { get; set; } = "ChiKhac";
    [JsonPropertyName("payment_employee_id")]   public int?     PaymentEmployeeId   { get; set; }
    [JsonPropertyName("attachment")]            public string?  Attachment          { get; set; }
    [JsonPropertyName("reference")]             public string?  Reference           { get; set; }
    [JsonPropertyName("accounting_date")]       public DateTime AccountingDate      { get; set; }
    [JsonPropertyName("document_date")]         public DateTime DocumentDate        { get; set; }
    [JsonPropertyName("document_number")]       public string   DocumentNumber      { get; set; } = "";
    [JsonPropertyName("entries")]               public List<PaymentEntryDto> Entries { get; set; } = new();
}
