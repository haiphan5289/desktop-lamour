// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Categories.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Categories.Data.Cache;

public sealed class CategoryCacheStore : EntityCacheStore<CategoryResponseDto>, ICategoryCacheStore
{
    public CategoryCacheStore() : base(c => c.Id)
    {
    }
}
