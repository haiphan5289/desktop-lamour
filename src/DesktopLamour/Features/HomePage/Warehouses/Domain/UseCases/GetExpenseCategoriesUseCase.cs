// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public sealed class GetExpenseCategoriesUseCase : IGetExpenseCategoriesUseCase
{
    private readonly IExpenseCategoryRepository _repository;
    public GetExpenseCategoriesUseCase(IExpenseCategoryRepository repository) => _repository = repository;

    public Task<IEnumerable<ExpenseCategory>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
