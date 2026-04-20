// HomeRepository.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Data.Services;
using DesktopLamour.Features.HomePage.Domain.Models;

namespace DesktopLamour.Features.HomePage.Data.Repositories;

public sealed class HomeRepository : IHomeRepository
{
    private readonly IHomeService _service;

    public HomeRepository(IHomeService service)
        => _service = service;

    public async Task<IEnumerable<ProductSummary>> GetProductsAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetProductsAsync(ct);
        return dtos.Select(d => new ProductSummary
        {
            Id            = d.Id,
            Name          = d.Name,
            Category      = d.Category,
            SalePrice     = d.SalePrice,
            StockQuantity = d.StockQuantity
        });
    }

    public async Task<IEnumerable<SupplierSummary>> GetSuppliersAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetSuppliersAsync(ct);
        return dtos.Select(d => new SupplierSummary
        {
            Id      = d.Id,
            Name    = d.Name,
            Phone   = d.Phone,
            Address = d.Address
        });
    }
}
