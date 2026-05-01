// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class UpdateSalesOrderUseCase : IUpdateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repository;

    public UpdateSalesOrderUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<SalesOrderResponseDto> ExecuteAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
        => _repository.UpdateAsync(id, request, ct);
}
