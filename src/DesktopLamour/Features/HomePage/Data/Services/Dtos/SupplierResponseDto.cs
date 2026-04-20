// SupplierResponseDto.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Data.Services.Dtos;

public class SupplierResponseDto
{
    [JsonPropertyName("id")]      public int Id { get; set; }
    [JsonPropertyName("name")]    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("phone")]   public string Phone { get; set; } = string.Empty;
    [JsonPropertyName("address")] public string Address { get; set; } = string.Empty;
}
