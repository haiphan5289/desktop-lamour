// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class GetWarehouseTransactionsUseCase : IGetWarehouseTransactionsUseCase
{
    private readonly IWarehouseTransactionService _service;

    public GetWarehouseTransactionsUseCase(IWarehouseTransactionService service)
        => _service = service;

    public Task<IEnumerable<WarehouseTransactionResponseDto>> ExecuteAsync(
        DateTime? fromDate, DateTime? toDate, string? type, CancellationToken ct = default)
        => _service.GetAllAsync(fromDate, toDate, type, ct);
}
