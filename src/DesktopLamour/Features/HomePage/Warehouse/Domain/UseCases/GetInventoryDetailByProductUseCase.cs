// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class GetInventoryDetailByProductUseCase : IGetInventoryDetailByProductUseCase
{
    private readonly IWarehouseRepository _repository;

    public GetInventoryDetailByProductUseCase(IWarehouseRepository repository)
        => _repository = repository;

    public Task<InventoryDetail?> ExecuteAsync(
        int productId,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        CancellationToken ct = default)
        => _repository.GetDetailAsync(productId, fromDate, toDate, warehouseIds, ct);
}
