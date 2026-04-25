// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public sealed class GetProductsUseCase : IGetProductsUseCase
{
    private readonly IProductRepository _repository;
    public GetProductsUseCase(IProductRepository repository) => _repository = repository;
    public Task<IEnumerable<Product>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
