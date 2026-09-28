// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

public class CashLedgerResponseDto
{
    [JsonPropertyName("opening_balance")] public decimal                  OpeningBalance { get; set; }
    [JsonPropertyName("closing_balance")] public decimal                  ClosingBalance { get; set; }
    // 2026-09-28: tồn quỹ tính tới hôm nay, độc lập bộ lọc Từ/Đến ngày.
    [JsonPropertyName("current_balance")] public decimal                  CurrentBalance { get; set; }
    [JsonPropertyName("entries")]         public List<CashLedgerEntryDto> Entries        { get; set; } = new();
}
