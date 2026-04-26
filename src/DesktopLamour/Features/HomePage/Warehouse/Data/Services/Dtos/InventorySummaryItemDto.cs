// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

public class InventorySummaryItemDto
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("opening_qty")]
    public int OpeningQty { get; set; }

    [JsonPropertyName("opening_value")]
    public decimal OpeningValue { get; set; }

    [JsonPropertyName("import_qty")]
    public int ImportQty { get; set; }

    [JsonPropertyName("import_value")]
    public decimal ImportValue { get; set; }

    [JsonPropertyName("export_qty")]
    public int ExportQty { get; set; }

    [JsonPropertyName("export_value")]
    public decimal ExportValue { get; set; }

    [JsonPropertyName("closing_qty")]
    public int ClosingQty { get; set; }

    [JsonPropertyName("closing_value")]
    public decimal ClosingValue { get; set; }
}
