// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;

public sealed class GetProductUnitsUseCase : IGetProductUnitsUseCase
{
    private readonly IProductUnitRepository _repository;
    public GetProductUnitsUseCase(IProductUnitRepository repository) => _repository = repository;

    public Task<IEnumerable<ProductUnit>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
