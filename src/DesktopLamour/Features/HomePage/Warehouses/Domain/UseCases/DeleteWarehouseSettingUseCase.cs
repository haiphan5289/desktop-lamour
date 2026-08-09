// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class DeleteWarehouseSettingUseCase : IDeleteWarehouseSettingUseCase
{
    private readonly IWarehouseSettingRepository _repository;
    public DeleteWarehouseSettingUseCase(IWarehouseSettingRepository repository) => _repository = repository;
    public Task ExecuteAsync(int warehouseId, CancellationToken ct = default)
        => _repository.DeleteAsync(warehouseId, ct);
}
