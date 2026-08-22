// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public interface IGetInventoryDetailByProductUseCase
{
    Task<InventoryDetail?> ExecuteAsync(
        int productId,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        CancellationToken ct = default);
}
