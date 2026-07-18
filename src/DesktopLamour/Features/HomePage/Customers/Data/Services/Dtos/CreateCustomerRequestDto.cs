// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;

public class CreateCustomerRequestDto
{
    [JsonPropertyName("name")]           public string Name          { get; set; } = string.Empty;
    [JsonPropertyName("address")]        public string Address       { get; set; } = string.Empty;
    [JsonPropertyName("province")]       public string Province      { get; set; } = string.Empty;
    [JsonPropertyName("customer_group")] public string CustomerGroup { get; set; } = string.Empty;
    [JsonPropertyName("tax_code")]       public string TaxCode       { get; set; } = string.Empty;
    [JsonPropertyName("phone")]          public string Phone         { get; set; } = string.Empty;
    [JsonPropertyName("sale_care_employee_id")] public int? SaleCareEmployeeId { get; set; }
}
