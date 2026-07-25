// IProductCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.ProductList.Data.Cache;

public interface IProductCacheStore : IEntityCacheStore<ProductResponseDto>
{
}
