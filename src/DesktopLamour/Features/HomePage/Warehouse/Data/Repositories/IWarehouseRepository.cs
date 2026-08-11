// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Repositories;

public interface IWarehouseRepository
{
    Task<IEnumerable<InventorySummaryItem>> GetSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        CancellationToken ct = default);
}
