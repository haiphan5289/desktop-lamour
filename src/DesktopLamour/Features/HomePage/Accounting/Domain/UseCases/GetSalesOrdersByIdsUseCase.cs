// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetSalesOrdersByIdsUseCase : IGetSalesOrdersByIdsUseCase
{
    private readonly IReceiptService _service;

    public GetSalesOrdersByIdsUseCase(IReceiptService service) => _service = service;

    public Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        IEnumerable<int> salesOrderIds, CancellationToken ct = default)
        => _service.GetSalesOrdersByIdsAsync(salesOrderIds, ct);
}
