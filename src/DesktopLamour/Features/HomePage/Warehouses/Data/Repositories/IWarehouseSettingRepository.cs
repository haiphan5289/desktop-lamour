// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;

public interface IWarehouseSettingRepository
{
    Task<IEnumerable<WarehouseSetting>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseSetting> CreateAsync(CreateWarehouseSettingInput input, CancellationToken ct = default);
    Task<WarehouseSetting> UpdateAsync(UpdateWarehouseSettingInput input, CancellationToken ct = default);
    Task DeleteAsync(int warehouseId, CancellationToken ct = default);
}
