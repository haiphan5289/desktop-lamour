// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public sealed class CreateProductUseCase : ICreateProductUseCase
{
    private readonly IProductRepository _repository;
    public CreateProductUseCase(IProductRepository repository) => _repository = repository;

    public async Task<Product> ExecuteAsync(CreateProductInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên sản phẩm không được để trống.");
        if (input.CategoryId <= 0)
            throw new ValidationException("CategoryId", "Vui lòng chọn danh mục.");

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var existing = await _repository.GetAllAsync(ct);
            if (existing.Any(p => p.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new ValidationException("Code", $"Mã '{input.Code}' đã tồn tại.");
        }

        return await _repository.CreateAsync(input, ct);
    }
}
