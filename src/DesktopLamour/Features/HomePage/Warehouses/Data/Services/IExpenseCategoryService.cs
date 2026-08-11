// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services;

public interface IExpenseCategoryService
{
    Task<IEnumerable<ExpenseCategoryResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<ExpenseCategoryResponseDto> CreateAsync(CreateExpenseCategoryRequestDto request, CancellationToken ct = default);
    Task<ExpenseCategoryResponseDto> UpdateAsync(int categoryId, UpdateExpenseCategoryRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int categoryId, CancellationToken ct = default);
}
