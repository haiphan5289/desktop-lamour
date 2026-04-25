// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
namespace DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;

public sealed class UpdateProductUseCase : IUpdateProductUseCase
{
    private readonly IProductRepository _repository;
    public UpdateProductUseCase(IProductRepository repository) => _repository = repository;

    public async Task<Product> ExecuteAsync(UpdateProductInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ValidationException("Name", "Tên sản phẩm không được để trống.");
        if (string.IsNullOrWhiteSpace(input.Category))
            throw new ValidationException("Category", "Danh mục không được để trống.");
        if (input.CostPrice <= 0)
            throw new ValidationException("CostPrice", "Giá nhập phải lớn hơn 0.");
        if (input.SellingPrice <= 0)
            throw new ValidationException("SellingPrice", "Giá bán phải lớn hơn 0.");

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var existing = await _repository.GetAllAsync(ct);
            if (existing.Any(p => p.Id != input.Id && p.Code.Equals(input.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new ValidationException("Code", $"Mã '{input.Code}' đã tồn tại.");
        }

        return await _repository.UpdateAsync(input, ct);
    }
}
