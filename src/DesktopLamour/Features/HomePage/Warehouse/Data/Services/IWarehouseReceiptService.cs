// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public interface IWarehouseReceiptService
{
    Task<IEnumerable<WarehouseReceiptResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseReceiptResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WarehouseReceiptResponseDto> CreateAsync(CreateWarehouseReceiptRequestDto request, CancellationToken ct = default);
    Task<WarehouseReceiptResponseDto> ConfirmAsync(int id, CancellationToken ct = default);
    Task<WarehouseReceiptResponseDto> UpdateAsync(int id, UpdateWarehouseReceiptRequestDto request, CancellationToken ct = default);
    Task<WarehouseReceiptResponseDto> UnconfirmAsync(int id, CancellationToken ct = default);
}
