// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

// Kết quả rút gọn của POST /api/v1/sales-returns/{id}/create-warehouse-receipt — chỉ cần đủ để
// hiện thông báo xác nhận, không cần map lại toàn bộ WarehouseReceiptResponseDto của module khác.
public class CreateWarehouseReceiptResultDto
{
    [JsonPropertyName("id")]             public int    Id            { get; set; }
    [JsonPropertyName("receipt_number")] public string ReceiptNumber { get; set; } = "";
}
