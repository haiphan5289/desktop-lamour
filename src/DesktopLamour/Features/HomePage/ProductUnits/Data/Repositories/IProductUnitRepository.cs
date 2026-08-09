// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.ProductUnits.Data.Repositories;

public interface IProductUnitRepository
{
    Task<IEnumerable<ProductUnit>> GetAllAsync(CancellationToken ct = default);
    Task<ProductUnit> CreateAsync(CreateProductUnitInput input, CancellationToken ct = default);
    Task<ProductUnit> UpdateAsync(UpdateProductUnitInput input, CancellationToken ct = default);
    Task DeleteAsync(int unitId, CancellationToken ct = default);
}
