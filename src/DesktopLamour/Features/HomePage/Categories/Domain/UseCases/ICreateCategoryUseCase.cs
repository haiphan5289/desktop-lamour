// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public interface ICreateCategoryUseCase
{
    Task<Category> ExecuteAsync(CreateCategoryInput input, CancellationToken ct = default);
}
