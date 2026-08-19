// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;

public class UpdateEmployeeRequestDto
{
    [JsonPropertyName("name")]                public string  Name              { get; set; } = string.Empty;
    [JsonPropertyName("gender")]              public string  Gender            { get; set; } = "Nam";
    [JsonPropertyName("phone")]               public string  Phone             { get; set; } = string.Empty;
    [JsonPropertyName("role")]                public string  Role              { get; set; } = "Cashier";
    [JsonPropertyName("unit")]                public string  Unit              { get; set; } = "Tiệm spa";
    [JsonPropertyName("job_title")]           public string  JobTitle          { get; set; } = "Khac";
    [JsonPropertyName("bank_account_number")] public string? BankAccountNumber { get; set; }
    [JsonPropertyName("bank_name")]           public string? BankName          { get; set; }
    [JsonPropertyName("password")]            public string? Password          { get; set; }
    [JsonPropertyName("is_active")]           public bool    IsActive          { get; set; } = true;
}
