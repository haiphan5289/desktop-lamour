// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;

namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public class GetNextSalesOrderCodeUseCase : IGetNextSalesOrderCodeUseCase
{
    private readonly ISalesOrderRepository _repository;

    public GetNextSalesOrderCodeUseCase(ISalesOrderRepository repository) => _repository = repository;

    public Task<string> ExecuteAsync(bool isFromWarehouseExport = true, CancellationToken ct = default)
        => _repository.GetNextCodeAsync(isFromWarehouseExport, ct);
}
