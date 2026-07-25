// ProductCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.ProductList.Data.Cache;

public sealed class ProductCacheStore : EntityCacheStore<ProductResponseDto>, IProductCacheStore
{
    public ProductCacheStore() : base(p => p.Id)
    {
    }
}
