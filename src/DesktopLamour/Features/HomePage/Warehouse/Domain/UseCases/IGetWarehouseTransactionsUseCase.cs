// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public interface IGetWarehouseTransactionsUseCase
{
    Task<IEnumerable<WarehouseTransactionResponseDto>> ExecuteAsync(
        DateTime? fromDate, DateTime? toDate, string? type, CancellationToken ct = default);
}
