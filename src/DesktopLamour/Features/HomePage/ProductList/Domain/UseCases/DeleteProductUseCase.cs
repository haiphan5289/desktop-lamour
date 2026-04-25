// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public sealed class DeleteProductUseCase : IDeleteProductUseCase
{
    private readonly IProductRepository _repository;
    public DeleteProductUseCase(IProductRepository repository) => _repository = repository;
    public Task ExecuteAsync(int productId, CancellationToken ct = default)
        => _repository.DeleteAsync(productId, ct);
}
