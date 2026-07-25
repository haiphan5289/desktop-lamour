// SupplierCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Suppliers.Data.Cache;

public sealed class SupplierCacheStore : EntityCacheStore<SupplierResponseDto>, ISupplierCacheStore
{
    public SupplierCacheStore() : base(s => s.Id)
    {
    }
}
