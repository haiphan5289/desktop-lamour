// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class GetSalesOrdersUseCase : IGetSalesOrdersUseCase
{
    private readonly ISalesOrderRepository _repository;

    public GetSalesOrdersUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<IEnumerable<SalesOrderResponseDto>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
