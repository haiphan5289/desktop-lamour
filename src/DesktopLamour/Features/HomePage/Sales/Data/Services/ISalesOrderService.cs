// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderResponseDto>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default);
    Task<SalesOrderResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default);
    Task<SalesOrderResponseDto> UpdateAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<string> GetNextCodeAsync(bool isFromWarehouseExport = true, CancellationToken ct = default);
    Task<SalesOrderResponseDto> HoldAsync(int id, CancellationToken ct = default);

    Task<IEnumerable<SalesOrderReportLineDto>> GetReportAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);

    Task<IEnumerable<SalesOrderSummaryLineDto>> GetSummaryReportAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
