// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class DeleteSalesOrderUseCase : IDeleteSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repository;

    public DeleteSalesOrderUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task ExecuteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}
