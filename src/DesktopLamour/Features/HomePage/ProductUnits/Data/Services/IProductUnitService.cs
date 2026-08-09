// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.ProductUnits.Data.Services;

public interface IProductUnitService
{
    Task<IEnumerable<ProductUnitResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductUnitResponseDto> CreateAsync(CreateProductUnitRequestDto request, CancellationToken ct = default);
    Task<ProductUnitResponseDto> UpdateAsync(int unitId, UpdateProductUnitRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int unitId, CancellationToken ct = default);
}
