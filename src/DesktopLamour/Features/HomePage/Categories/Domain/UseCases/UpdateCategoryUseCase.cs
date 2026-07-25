// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Categories.Data.Repositories;
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public sealed class UpdateCategoryUseCase : IUpdateCategoryUseCase
{
    private readonly ICategoryRepository _repository;
    public UpdateCategoryUseCase(ICategoryRepository repository) => _repository = repository;

    public async Task<Category> ExecuteAsync(UpdateCategoryInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên danh mục không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(c => c.Id != input.Id && c.Name.Equals(input.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Name", $"Danh mục '{input.Name}' đã tồn tại.");

        return await _repository.UpdateAsync(input, ct);
    }
}
