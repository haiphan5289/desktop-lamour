// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class UpdateWarehouseReceiptUseCase : IUpdateWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptService _service;

    public UpdateWarehouseReceiptUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task<WarehouseReceiptResponseDto> ExecuteAsync(
        int id, UpdateWarehouseReceiptRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);
}
