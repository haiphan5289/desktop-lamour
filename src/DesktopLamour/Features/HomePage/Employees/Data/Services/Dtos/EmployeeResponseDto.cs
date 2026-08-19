// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;

public class EmployeeResponseDto
{
    [JsonPropertyName("id")]                   public int     Id                { get; set; }
    [JsonPropertyName("code")]                 public string  Code              { get; set; } = string.Empty;
    [JsonPropertyName("name")]                 public string  Name              { get; set; } = string.Empty;
    [JsonPropertyName("gender")]               public string  Gender            { get; set; } = string.Empty;
    [JsonPropertyName("phone")]                public string  Phone             { get; set; } = string.Empty;
    [JsonPropertyName("role")]                 public string  Role              { get; set; } = string.Empty;
    [JsonPropertyName("unit")]                 public string  Unit              { get; set; } = string.Empty;
    [JsonPropertyName("job_title")]            public string  JobTitle          { get; set; } = string.Empty;
    [JsonPropertyName("bank_account_number")]  public string? BankAccountNumber { get; set; }
    [JsonPropertyName("bank_name")]            public string? BankName          { get; set; }
    [JsonPropertyName("is_active")]            public bool    IsActive          { get; set; }
}
