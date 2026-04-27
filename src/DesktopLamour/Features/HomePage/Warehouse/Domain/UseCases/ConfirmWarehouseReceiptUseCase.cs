// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class ConfirmWarehouseReceiptUseCase : IConfirmWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptService _service;

    public ConfirmWarehouseReceiptUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task<WarehouseReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
        => _service.ConfirmAsync(id, ct);
}
