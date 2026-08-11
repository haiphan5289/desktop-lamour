// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Repositories;

public interface IExpenseCategoryRepository
{
    Task<IEnumerable<ExpenseCategory>> GetAllAsync(CancellationToken ct = default);
    Task<ExpenseCategory> CreateAsync(CreateExpenseCategoryInput input, CancellationToken ct = default);
    Task<ExpenseCategory> UpdateAsync(UpdateExpenseCategoryInput input, CancellationToken ct = default);
    Task DeleteAsync(int categoryId, CancellationToken ct = default);
}
