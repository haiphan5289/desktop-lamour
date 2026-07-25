// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public interface IDeleteCategoryUseCase
{
    Task ExecuteAsync(int categoryId, CancellationToken ct = default);
}
