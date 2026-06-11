// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Data.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly ISalesOrderService _service;

    public SalesOrderRepository(ISalesOrderService service) => _service = service;

    public Task<IEnumerable<SalesOrderResponseDto>> GetAllAsync(CancellationToken ct = default)
        => _service.GetAllAsync(ct);

    public Task<SalesOrderResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _service.GetByIdAsync(id, ct);

    public Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);

    public Task<SalesOrderResponseDto> UpdateAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _service.DeleteAsync(id, ct);

    public Task<string> GetNextCodeAsync(CancellationToken ct = default)
        => _service.GetNextCodeAsync(ct);

    public Task<SalesOrderResponseDto> HoldAsync(int id, CancellationToken ct = default)
        => _service.HoldAsync(id, ct);

    public Task<SalesOrderResponseDto> ConfirmAsync(int id, CancellationToken ct = default)
        => _service.ConfirmAsync(id, ct);
}
