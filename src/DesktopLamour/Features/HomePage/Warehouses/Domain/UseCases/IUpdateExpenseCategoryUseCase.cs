// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public interface IUpdateExpenseCategoryUseCase
{
    Task<ExpenseCategory> ExecuteAsync(UpdateExpenseCategoryInput input, CancellationToken ct = default);
}
