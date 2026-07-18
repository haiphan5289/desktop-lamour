// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class GetSalesOrderSummaryReportUseCase : IGetSalesOrderSummaryReportUseCase
{
    private readonly ISalesOrderRepository _repository;

    public GetSalesOrderSummaryReportUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<IEnumerable<SalesOrderSummaryLineDto>> ExecuteAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        => _repository.GetSummaryReportAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);
}
