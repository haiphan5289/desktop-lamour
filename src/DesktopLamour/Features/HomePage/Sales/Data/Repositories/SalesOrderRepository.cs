// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Data.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly ISalesOrderService _service;

    public SalesOrderRepository(ISalesOrderService service) => _service = service;

    public Task<IEnumerable<SalesOrderResponseDto>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
        => _service.GetAllAsync(fromDate, toDate, search, ct);

    public Task<SalesOrderResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _service.GetByIdAsync(id, ct);

    public Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);

    public Task<SalesOrderResponseDto> UpdateAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _service.DeleteAsync(id, ct);

    public Task<string> GetNextCodeAsync(bool isFromWarehouseExport = true, CancellationToken ct = default)
        => _service.GetNextCodeAsync(isFromWarehouseExport, ct);

    public Task<SalesOrderResponseDto> HoldAsync(int id, CancellationToken ct = default)
        => _service.HoldAsync(id, ct);

    public Task<SalesOrderResponseDto> DuplicateAsync(int id, CancellationToken ct = default)
        => _service.DuplicateAsync(id, ct);

    public Task<IEnumerable<SalesOrderReportLineDto>> GetReportAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        => _service.GetReportAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);

    public Task<IEnumerable<SalesOrderSummaryLineDto>> GetSummaryReportAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        => _service.GetSummaryReportAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);
}
