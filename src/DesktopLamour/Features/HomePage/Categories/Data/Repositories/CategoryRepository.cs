// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Data.Services;
using DesktopLamour.Features.HomePage.Categories.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.Categories.Data.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ICategoryService _service;
    public CategoryRepository(ICategoryService service) => _service = service;

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<Category> CreateAsync(CreateCategoryInput input, CancellationToken ct = default)
    {
        var request = new CreateCategoryRequestDto { Name = input.Name };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<Category> UpdateAsync(UpdateCategoryInput input, CancellationToken ct = default)
    {
        var request = new UpdateCategoryRequestDto { Name = input.Name };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    public Task DeleteAsync(int categoryId, CancellationToken ct = default)
        => _service.DeleteAsync(categoryId, ct);

    private static Category MapToModel(CategoryResponseDto d) => new()
    {
        Id   = d.Id,
        Name = d.Name,
    };
}
