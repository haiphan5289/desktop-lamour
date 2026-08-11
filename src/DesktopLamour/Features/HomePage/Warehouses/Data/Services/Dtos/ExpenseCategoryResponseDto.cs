// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;

public class ExpenseCategoryResponseDto
{
    [JsonPropertyName("id")]              public int     Id             { get; set; }
    [JsonPropertyName("code")]            public string  Code           { get; set; } = string.Empty;
    [JsonPropertyName("name")]            public string  Name           { get; set; } = string.Empty;
    [JsonPropertyName("department_id")]   public int?    DepartmentId   { get; set; }
    [JsonPropertyName("department_name")] public string? DepartmentName { get; set; }
    [JsonPropertyName("description")]     public string? Description    { get; set; }
}
