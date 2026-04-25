// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;

public class UpdateSupplierRequestDto
{
    [JsonPropertyName("code")]             public string Code           { get; set; } = string.Empty;
    [JsonPropertyName("name")]             public string Name           { get; set; } = string.Empty;
    [JsonPropertyName("phone")]            public string Phone          { get; set; } = string.Empty;
    [JsonPropertyName("address")]          public string Address        { get; set; } = string.Empty;
    [JsonPropertyName("group")]            public string Group          { get; set; } = string.Empty;
    [JsonPropertyName("tax_code")]         public string TaxCode        { get; set; } = string.Empty;
    [JsonPropertyName("is_stop_tracking")] public bool   IsStopTracking { get; set; }
}
