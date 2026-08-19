// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using System.IO;
namespace DesktopLamour.Features.HomePage.ProductList.Data.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int productId, CancellationToken ct = default);
    Task<Product> DuplicateAsync(int productId, CancellationToken ct = default);
    Task<Product> CreateAsync(CreateProductInput input, CancellationToken ct = default);
    Task<Product> UpdateAsync(UpdateProductInput input, CancellationToken ct = default);
    Task<ImportProductResult> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
