// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<SalesOrderResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default);
    Task<SalesOrderResponseDto> UpdateAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
