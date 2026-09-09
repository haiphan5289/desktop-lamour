// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

public class SalesReturnResponseDto
{
    [JsonPropertyName("id")]              public int     Id             { get; set; }
    [JsonPropertyName("document_number")] public string  DocumentNumber { get; set; } = "";
    [JsonPropertyName("accounting_date")] public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]   public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("customer_id")]     public int     CustomerId     { get; set; }
    [JsonPropertyName("customer_name")]   public string  CustomerName   { get; set; } = "";
    [JsonPropertyName("employee_id")]     public int?    EmployeeId     { get; set; }
    [JsonPropertyName("employee_name")]   public string? EmployeeName   { get; set; }
    [JsonPropertyName("description")]     public string? Description    { get; set; }
    [JsonPropertyName("reference")]       public string? Reference      { get; set; }
    [JsonPropertyName("return_type")]     public int     ReturnType     { get; set; }
    [JsonPropertyName("total_amount")]    public decimal TotalAmount    { get; set; }
    [JsonPropertyName("total_discount")]  public decimal TotalDiscount  { get; set; }
    [JsonPropertyName("total_payment")]   public decimal TotalPayment   { get; set; }
    [JsonPropertyName("created_at")]      public DateTime CreatedAt     { get; set; }
    // Luôn "Confirmed" — vòng đời Nháp → Ghi sổ đã bỏ (2026-09-07). Giữ field để không phá
    // hợp đồng JSON; cột "Trạng thái" màn danh sách vẫn hiển thị "Đã ghi sổ" từ giá trị này.
    [JsonPropertyName("status")]          public string  Status         { get; set; } = "Confirmed";
    [JsonPropertyName("confirmed_at")]    public DateTime? ConfirmedAt  { get; set; }
    [JsonPropertyName("lines")]           public List<SalesReturnLineDto> Lines { get; set; } = new();
}
