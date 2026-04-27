// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class CreateWarehouseReceiptUseCase : ICreateWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptService _service;

    public CreateWarehouseReceiptUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task<WarehouseReceiptResponseDto> ExecuteAsync(
        CreateWarehouseReceiptRequestDto request,
        CancellationToken ct = default)
        => _service.CreateAsync(request, ct);
}
