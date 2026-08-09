// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public interface IUpdateWarehouseSettingUseCase
{
    Task<WarehouseSetting> ExecuteAsync(UpdateWarehouseSettingInput input, CancellationToken ct = default);
}
