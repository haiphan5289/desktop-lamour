// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.ProductUnits.Data.Cache;

public sealed class ProductUnitCacheStore : EntityCacheStore<ProductUnitResponseDto>, IProductUnitCacheStore
{
    public ProductUnitCacheStore() : base(u => u.Id)
    {
    }
}
