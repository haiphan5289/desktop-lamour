// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Services;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;

public sealed class WarehouseSettingRepository : IWarehouseSettingRepository
{
    private readonly IWarehouseSettingService _service;
    public WarehouseSettingRepository(IWarehouseSettingService service) => _service = service;

    public async Task<IEnumerable<WarehouseSetting>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<WarehouseSetting> CreateAsync(CreateWarehouseSettingInput input, CancellationToken ct = default)
    {
        var request = new CreateWarehouseSettingRequestDto { Code = input.Code, Name = input.Name, IsActive = input.IsActive };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<WarehouseSetting> UpdateAsync(UpdateWarehouseSettingInput input, CancellationToken ct = default)
    {
        var request = new UpdateWarehouseSettingRequestDto { Code = input.Code, Name = input.Name, IsActive = input.IsActive };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    public Task DeleteAsync(int warehouseId, CancellationToken ct = default)
        => _service.DeleteAsync(warehouseId, ct);

    private static WarehouseSetting MapToModel(WarehouseSettingResponseDto d) => new()
    {
        Id       = d.Id,
        Code     = d.Code,
        Name     = d.Name,
        IsActive = d.IsActive,
    };
}
