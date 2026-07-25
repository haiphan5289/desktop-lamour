// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Categories.Data.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, CancellationToken ct = default);
    Task<CategoryResponseDto> UpdateAsync(int categoryId, UpdateCategoryRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int categoryId, CancellationToken ct = default);
}
