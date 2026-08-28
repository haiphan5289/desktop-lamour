// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class DuplicateSalesOrderUseCase : IDuplicateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repository;

    public DuplicateSalesOrderUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
        => _repository.DuplicateAsync(id, ct);
}
