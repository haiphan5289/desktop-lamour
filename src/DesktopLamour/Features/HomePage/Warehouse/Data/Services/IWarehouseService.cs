// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public interface IWarehouseService
{
    Task<IEnumerable<InventorySummaryItemDto>> GetInventorySummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default);

    // Tổng hợp tồn kho chia theo từng kho (MISA) — mỗi nhóm kho có số liệu riêng từng sản phẩm.
    Task<IEnumerable<InventorySummaryByWarehouseDto>> GetInventorySummaryByWarehouseAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        IReadOnlyList<int>? productIds = null,
        CancellationToken ct = default);

    Task<InventoryDetailResponseDto?> GetInventoryDetailAsync(
        int productId,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        CancellationToken ct = default);
}
