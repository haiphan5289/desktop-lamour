// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Data.Services;
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.ProductList.Data.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IProductService _service;
    public ProductRepository(IProductService service) => _service = service;

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public Task DeleteAsync(int productId, CancellationToken ct = default)
        => _service.DeleteAsync(productId, ct);

    public async Task<Product> DuplicateAsync(int productId, CancellationToken ct = default)
    {
        var d = await _service.DuplicateAsync(productId, ct);
        return MapToModel(d);
    }

    public async Task<Product> CreateAsync(CreateProductInput input, CancellationToken ct = default)
    {
        var request = new CreateProductRequestDto
        {
            Code             = input.Code,
            Name             = input.Name,
            CategoryId       = input.CategoryId,
            Unit             = input.Unit,
            CostPrice        = input.CostPrice,
            SellingPrice     = input.SellingPrice,
            StockQuantity    = input.StockQuantity,
            IsActive         = input.IsActive,
            VatRate          = input.VatRate?.ToString(),
            TaxReductionType = input.TaxReductionType?.ToString(),
            ImportTaxRate    = input.ImportTaxRate,
            ExportTaxRate    = input.ExportTaxRate,
            ExciseTaxGroup   = input.ExciseTaxGroup,
        };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<Product> UpdateAsync(UpdateProductInput input, CancellationToken ct = default)
    {
        var request = new UpdateProductRequestDto
        {
            Code             = input.Code,
            Name             = input.Name,
            CategoryId       = input.CategoryId,
            Unit             = input.Unit,
            CostPrice        = input.CostPrice,
            SellingPrice     = input.SellingPrice,
            StockQuantity    = input.StockQuantity,
            IsActive         = input.IsActive,
            VatRate          = input.VatRate?.ToString(),
            TaxReductionType = input.TaxReductionType?.ToString(),
            ImportTaxRate    = input.ImportTaxRate,
            ExportTaxRate    = input.ExportTaxRate,
            ExciseTaxGroup   = input.ExciseTaxGroup,
        };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    private static Product MapToModel(ProductResponseDto d) => new()
    {
        Id               = d.Id,
        Code             = d.Code,
        Name             = d.Name,
        CategoryId       = d.CategoryId,
        CategoryName     = d.CategoryName,
        Unit             = d.Unit,
        CostPrice        = d.CostPrice,
        SellingPrice     = d.SellingPrice,
        StockQuantity    = d.StockQuantity,
        IsActive         = d.IsActive,
        VatRate          = Enum.TryParse<VatRateType>(d.VatRate, out var vr) ? vr : null,
        TaxReductionType = Enum.TryParse<TaxReductionStatus>(d.TaxReductionType, out var tr) ? tr : null,
        ImportTaxRate    = d.ImportTaxRate,
        ExportTaxRate    = d.ExportTaxRate,
        ExciseTaxGroup   = d.ExciseTaxGroup,
    };
}
