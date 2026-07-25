// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public interface IGetCategoriesUseCase
{
    Task<IEnumerable<Category>> ExecuteAsync(CancellationToken ct = default);
}
