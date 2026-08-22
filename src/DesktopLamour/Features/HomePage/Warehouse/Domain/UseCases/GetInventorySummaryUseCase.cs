// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class GetInventorySummaryUseCase : IGetInventorySummaryUseCase
{
    private readonly IWarehouseRepository _repository;

    public GetInventorySummaryUseCase(IWarehouseRepository repository)
        => _repository = repository;

    public Task<IEnumerable<InventorySummaryItem>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default)
        => _repository.GetSummaryAsync(fromDate, toDate, warehouseIds, categoryId, productUnitId, productIds, ct);
}
