// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public interface IUpdateCategoryUseCase
{
    Task<Category> ExecuteAsync(UpdateCategoryInput input, CancellationToken ct = default);
}
