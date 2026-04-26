// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public interface IGetInventorySummaryUseCase
{
    Task<IEnumerable<InventorySummaryItem>> ExecuteAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default);
}
