// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;

public class UpdateDepartmentRequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
