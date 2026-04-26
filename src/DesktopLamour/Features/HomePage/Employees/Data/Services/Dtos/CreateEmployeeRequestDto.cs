// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;

public class CreateEmployeeRequestDto
{
    [JsonPropertyName("name")]      public string Name     { get; set; } = string.Empty;
    [JsonPropertyName("phone")]     public string Phone    { get; set; } = string.Empty;
    [JsonPropertyName("role")]      public string Role     { get; set; } = "Cashier";
    [JsonPropertyName("unit")]      public string Unit     { get; set; } = "Spa";
    [JsonPropertyName("password")]  public string Password { get; set; } = string.Empty;
    [JsonPropertyName("is_active")] public bool   IsActive { get; set; } = true;
}
