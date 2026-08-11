// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public interface IDeleteExpenseCategoryUseCase
{
    Task ExecuteAsync(int categoryId, CancellationToken ct = default);
}
