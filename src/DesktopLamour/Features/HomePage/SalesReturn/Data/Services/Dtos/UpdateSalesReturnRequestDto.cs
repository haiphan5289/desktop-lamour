// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

public class UpdateSalesReturnRequestDto
{
    [JsonPropertyName("document_number")] public string   DocumentNumber { get; set; } = "";
    [JsonPropertyName("accounting_date")] public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]   public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("customer_id")]     public int      CustomerId     { get; set; }
    [JsonPropertyName("employee_id")]     public int?     EmployeeId     { get; set; }
    [JsonPropertyName("description")]     public string?  Description    { get; set; }
    [JsonPropertyName("reference")]       public string?  Reference      { get; set; }
    [JsonPropertyName("return_type")]     public int      ReturnType     { get; set; }
    [JsonPropertyName("lines")]           public List<SalesReturnLineDto> Lines { get; set; } = new();
}
