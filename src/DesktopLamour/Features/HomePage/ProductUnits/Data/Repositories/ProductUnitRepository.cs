// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.Data.Services;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.ProductUnits.Data.Repositories;

public sealed class ProductUnitRepository : IProductUnitRepository
{
    private readonly IProductUnitService _service;
    public ProductUnitRepository(IProductUnitService service) => _service = service;

    public async Task<IEnumerable<ProductUnit>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<ProductUnit> CreateAsync(CreateProductUnitInput input, CancellationToken ct = default)
    {
        var request = new CreateProductUnitRequestDto { Name = input.Name };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<ProductUnit> UpdateAsync(UpdateProductUnitInput input, CancellationToken ct = default)
    {
        var request = new UpdateProductUnitRequestDto { Name = input.Name };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    public Task DeleteAsync(int unitId, CancellationToken ct = default)
        => _service.DeleteAsync(unitId, ct);

    private static ProductUnit MapToModel(ProductUnitResponseDto d) => new()
    {
        Id   = d.Id,
        Name = d.Name,
    };
}
