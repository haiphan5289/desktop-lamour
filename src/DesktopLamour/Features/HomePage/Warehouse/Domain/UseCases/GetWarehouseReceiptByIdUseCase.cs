// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class GetWarehouseReceiptByIdUseCase : IGetWarehouseReceiptByIdUseCase
{
    private readonly IWarehouseReceiptService _service;

    public GetWarehouseReceiptByIdUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task<WarehouseReceiptResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
        => _service.GetByIdAsync(id, ct);
}
