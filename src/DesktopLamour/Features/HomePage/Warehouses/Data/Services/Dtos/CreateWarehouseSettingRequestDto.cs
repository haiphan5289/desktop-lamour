// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;

public class CreateWarehouseSettingRequestDto
{
    [JsonPropertyName("code")]      public string Code     { get; set; } = string.Empty;
    [JsonPropertyName("name")]      public string Name     { get; set; } = string.Empty;
    [JsonPropertyName("is_active")] public bool   IsActive { get; set; } = true;
}
