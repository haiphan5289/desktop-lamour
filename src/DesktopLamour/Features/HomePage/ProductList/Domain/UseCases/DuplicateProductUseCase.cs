// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public sealed class DuplicateProductUseCase : IDuplicateProductUseCase
{
    private readonly IProductRepository _repository;
    public DuplicateProductUseCase(IProductRepository repository) => _repository = repository;
    public Task<Product> ExecuteAsync(int productId, CancellationToken ct = default)
        => _repository.DuplicateAsync(productId, ct);
}
