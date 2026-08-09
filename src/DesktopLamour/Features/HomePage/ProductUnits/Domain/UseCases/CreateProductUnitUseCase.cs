// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;

public sealed class CreateProductUnitUseCase : ICreateProductUnitUseCase
{
    private readonly IProductUnitRepository _repository;
    public CreateProductUnitUseCase(IProductUnitRepository repository) => _repository = repository;

    public async Task<ProductUnit> ExecuteAsync(CreateProductUnitInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên đơn vị tính không được để trống.");

        var existing = await _repository.GetAllAsync(ct);
        if (existing.Any(u => u.Name.Equals(input.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Name", $"Đơn vị tính '{input.Name}' đã tồn tại.");

        return await _repository.CreateAsync(input, ct);
    }
}
