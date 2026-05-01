// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class CreateSalesOrderUseCase : ICreateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repository;

    public CreateSalesOrderUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<SalesOrderResponseDto> ExecuteAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default)
        => _repository.CreateAsync(request, ct);
}
