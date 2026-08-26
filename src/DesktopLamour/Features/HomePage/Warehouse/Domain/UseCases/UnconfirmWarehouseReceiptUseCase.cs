// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class UnconfirmWarehouseReceiptUseCase : IUnconfirmWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptService _service;

    public UnconfirmWarehouseReceiptUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task<WarehouseReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
        => _service.UnconfirmAsync(id, ct);
}
