// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.Data.Repositories;
namespace DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;

public sealed class DeleteProductUnitUseCase : IDeleteProductUnitUseCase
{
    private readonly IProductUnitRepository _repository;
    public DeleteProductUnitUseCase(IProductUnitRepository repository) => _repository = repository;
    public Task ExecuteAsync(int unitId, CancellationToken ct = default)
        => _repository.DeleteAsync(unitId, ct);
}
