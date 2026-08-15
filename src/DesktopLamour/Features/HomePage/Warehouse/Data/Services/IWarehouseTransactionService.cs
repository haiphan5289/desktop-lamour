// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public interface IWarehouseTransactionService
{
    Task<IEnumerable<WarehouseTransactionResponseDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? type, CancellationToken ct = default);
}
