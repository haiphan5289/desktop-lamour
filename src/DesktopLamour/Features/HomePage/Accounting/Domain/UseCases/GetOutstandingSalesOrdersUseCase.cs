// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetOutstandingSalesOrdersUseCase : IGetOutstandingSalesOrdersUseCase
{
    private readonly IReceiptService _service;

    public GetOutstandingSalesOrdersUseCase(IReceiptService service) => _service = service;

    public Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId = null, CancellationToken ct = default)
        => _service.GetOutstandingSalesOrdersAsync(fromDate, toDate, employeeId, ct);
}
