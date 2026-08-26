// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class CreateSalesReturnWarehouseReceiptUseCase : ICreateSalesReturnWarehouseReceiptUseCase
{
    private readonly ISalesReturnRepository _repository;

    public CreateSalesReturnWarehouseReceiptUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task<CreateWarehouseReceiptResultDto> ExecuteAsync(int id, CancellationToken ct = default)
        => _repository.CreateWarehouseReceiptAsync(id, ct);
}
