// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class GetInventorySummaryByWarehouseUseCase : IGetInventorySummaryByWarehouseUseCase
{
    private readonly IWarehouseRepository _repository;

    public GetInventorySummaryByWarehouseUseCase(IWarehouseRepository repository)
        => _repository = repository;

    public Task<IEnumerable<InventoryWarehouseGroup>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default)
        => _repository.GetSummaryByWarehouseAsync(fromDate, toDate, warehouseIds, categoryId, productUnitId, productIds, ct);
}
