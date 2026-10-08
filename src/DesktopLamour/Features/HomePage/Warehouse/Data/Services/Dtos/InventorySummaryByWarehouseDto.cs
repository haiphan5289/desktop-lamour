// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

// 1 nhóm kho của "Tổng hợp tồn kho" — số liệu từng sản phẩm tính riêng trong kho đó (khớp MISA).
public class InventorySummaryByWarehouseDto
{
    [JsonPropertyName("warehouse_id")]
    public int WarehouseId { get; set; }

    [JsonPropertyName("warehouse_code")]
    public string WarehouseCode { get; set; } = string.Empty;

    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<InventorySummaryItemDto> Items { get; set; } = new();
}
