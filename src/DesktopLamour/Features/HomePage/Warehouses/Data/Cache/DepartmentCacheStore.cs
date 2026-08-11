// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Warehouses.Data.Cache;

public sealed class DepartmentCacheStore : EntityCacheStore<DepartmentResponseDto>, IDepartmentCacheStore
{
    public DepartmentCacheStore() : base(d => d.Id)
    {
    }
}
