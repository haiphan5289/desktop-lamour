// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services;

public interface IWarehouseSettingService
{
    Task<IEnumerable<WarehouseSettingResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseSettingResponseDto> CreateAsync(CreateWarehouseSettingRequestDto request, CancellationToken ct = default);
    Task<WarehouseSettingResponseDto> UpdateAsync(int warehouseId, UpdateWarehouseSettingRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int warehouseId, CancellationToken ct = default);
}
