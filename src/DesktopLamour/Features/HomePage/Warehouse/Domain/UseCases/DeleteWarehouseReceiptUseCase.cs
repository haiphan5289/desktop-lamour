// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;

public sealed class DeleteWarehouseReceiptUseCase : IDeleteWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptService _service;

    public DeleteWarehouseReceiptUseCase(IWarehouseReceiptService service)
        => _service = service;

    public Task ExecuteAsync(int id, CancellationToken ct = default)
        => _service.DeleteAsync(id, ct);
}
