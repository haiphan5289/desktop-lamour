// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public interface IWarehouseService
{
    Task<IEnumerable<InventorySummaryItemDto>> GetInventorySummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);
}
