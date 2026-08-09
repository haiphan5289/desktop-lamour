// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public interface IDeleteWarehouseSettingUseCase
{
    Task ExecuteAsync(int warehouseId, CancellationToken ct = default);
}
