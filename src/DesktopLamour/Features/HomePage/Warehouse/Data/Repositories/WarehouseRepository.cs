// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly IWarehouseService _service;

    public WarehouseRepository(IWarehouseService service)
        => _service = service;

    public async Task<IEnumerable<InventorySummaryItem>> GetSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        CancellationToken ct = default)
    {
        var dtos = await _service.GetInventorySummaryAsync(fromDate, toDate, warehouseIds, categoryId, productUnitId, ct);
        return dtos.Select(d => new InventorySummaryItem
        {
            ProductId    = d.ProductId,
            Code         = d.Code,
            Name         = d.Name,
            Unit         = d.Unit,
            OpeningQty   = d.OpeningQty,
            OpeningValue = d.OpeningValue,
            ImportQty    = d.ImportQty,
            ImportValue  = d.ImportValue,
            ExportQty    = d.ExportQty,
            ExportValue  = d.ExportValue,
            ClosingQty            = d.ClosingQty,
            ClosingValue          = d.ClosingValue,
            LatestAccountingDate  = d.LatestAccountingDate.HasValue
                                        ? d.LatestAccountingDate.Value.ToLocalTime()
                                        : null,
        });
    }
}
