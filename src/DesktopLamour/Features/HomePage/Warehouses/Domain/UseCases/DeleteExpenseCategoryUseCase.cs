// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class DeleteExpenseCategoryUseCase : IDeleteExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repository;
    public DeleteExpenseCategoryUseCase(IExpenseCategoryRepository repository) => _repository = repository;
    public Task ExecuteAsync(int categoryId, CancellationToken ct = default)
        => _repository.DeleteAsync(categoryId, ct);
}
