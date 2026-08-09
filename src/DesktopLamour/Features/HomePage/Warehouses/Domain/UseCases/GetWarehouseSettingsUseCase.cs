// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class GetWarehouseSettingsUseCase : IGetWarehouseSettingsUseCase
{
    private readonly IWarehouseSettingRepository _repository;
    public GetWarehouseSettingsUseCase(IWarehouseSettingRepository repository) => _repository = repository;

    public Task<IEnumerable<WarehouseSetting>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
