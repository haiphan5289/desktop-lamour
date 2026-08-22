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
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default)
    {
        var dtos = await _service.GetInventorySummaryAsync(fromDate, toDate, warehouseIds, categoryId, productUnitId, productIds, ct);
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

    public async Task<InventoryDetail?> GetDetailAsync(
        int productId,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        CancellationToken ct = default)
    {
        var dto = await _service.GetInventoryDetailAsync(productId, fromDate, toDate, warehouseIds, ct);
        if (dto is null) return null;

        return new InventoryDetail
        {
            ProductId    = dto.ProductId,
            Code         = dto.Code,
            Name         = dto.Name,
            Unit         = dto.Unit,
            OpeningQty   = dto.OpeningQty,
            OpeningValue = dto.OpeningValue,
            ClosingQty   = dto.ClosingQty,
            ClosingValue = dto.ClosingValue,
            Lines = dto.Lines.Select(l => new InventoryDetailLine
            {
                AccountingDate = l.AccountingDate.ToLocalTime(),
                DocumentDate   = l.DocumentDate.ToLocalTime(),
                DocumentNumber = l.DocumentNumber,
                DocumentType   = l.DocumentType,
                SourceId       = l.SourceId,
                Description    = l.Description,
                Unit           = l.Unit,
                ImportQty      = l.ImportQty,
                ImportValue    = l.ImportValue,
                ExportQty      = l.ExportQty,
                ExportValue    = l.ExportValue,
                RunningQty     = l.RunningQty,
                RunningValue   = l.RunningValue,
            }).ToList(),
        };
    }
}
