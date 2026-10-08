// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class GetNextWarehouseReceiptNumberUseCase : IGetNextWarehouseReceiptNumberUseCase
{
    private readonly IWarehouseReceiptService _service;

    public GetNextWarehouseReceiptNumberUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task<string?> ExecuteAsync(CancellationToken ct = default)
        => _service.GetNextNumberAsync(ct);
}
