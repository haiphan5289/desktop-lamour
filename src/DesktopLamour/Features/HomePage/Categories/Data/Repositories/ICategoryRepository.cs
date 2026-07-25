// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Categories.Data.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category> CreateAsync(CreateCategoryInput input, CancellationToken ct = default);
    Task<Category> UpdateAsync(UpdateCategoryInput input, CancellationToken ct = default);
    Task DeleteAsync(int categoryId, CancellationToken ct = default);
}
