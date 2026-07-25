// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Categories.Data.Services.Dtos;

public class UpdateCategoryRequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
