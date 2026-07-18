// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class GetSalesOrderReportUseCase : IGetSalesOrderReportUseCase
{
    private readonly ISalesOrderRepository _repository;

    public GetSalesOrderReportUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<IEnumerable<SalesOrderReportLineDto>> ExecuteAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        => _repository.GetReportAsync(productIds, employeeId, customerId, unit, category, fromDate, toDate, ct);
}
