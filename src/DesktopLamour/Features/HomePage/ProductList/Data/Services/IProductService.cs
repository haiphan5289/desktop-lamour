// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;
using System.IO;
namespace DesktopLamour.Features.HomePage.ProductList.Data.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken ct = default);
    Task<ProductResponseDto> UpdateAsync(int productId, UpdateProductRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int productId, CancellationToken ct = default);
    Task<ProductResponseDto> DuplicateAsync(int productId, CancellationToken ct = default);
    Task<ImportProductResultDto> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
