// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class UpdateReceiptRequestDto
{
    // "Customer" | "Employee". Bỏ trống cả 2 = phiếu thu hàng loạt (BE chỉ nhận với ThuKhachHangHangLoat).
    [JsonPropertyName("partner_type")]          public string?  PartnerType         { get; set; }
    [JsonPropertyName("partner_id")]            public int?     PartnerId           { get; set; }
    [JsonPropertyName("payer_name")]            public string   PayerName           { get; set; } = "";
    [JsonPropertyName("address")]               public string?  Address             { get; set; }
    [JsonPropertyName("payment_reason")]        public string   PaymentReason       { get; set; } = "ThuKhac";
    [JsonPropertyName("reason_detail")]         public string?  ReasonDetail        { get; set; }
    [JsonPropertyName("collector_employee_id")] public int?     CollectorEmployeeId { get; set; }
    [JsonPropertyName("attachment")]            public string?  Attachment          { get; set; }
    [JsonPropertyName("reference")]             public string?  Reference           { get; set; }
    [JsonPropertyName("accounting_date")]       public DateTime AccountingDate      { get; set; }
    [JsonPropertyName("document_date")]         public DateTime DocumentDate        { get; set; }
    [JsonPropertyName("document_number")]       public string   DocumentNumber      { get; set; } = "";
    [JsonPropertyName("entries")]               public List<ReceiptEntryDto> Entries { get; set; } = new();
}
