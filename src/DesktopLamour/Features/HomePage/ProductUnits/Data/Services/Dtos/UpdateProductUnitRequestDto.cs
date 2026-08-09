// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.ProductUnits.Data.Services.Dtos;

public class UpdateProductUnitRequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
