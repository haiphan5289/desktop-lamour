// ISupplierCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Suppliers.Data.Cache;

public interface ISupplierCacheStore : IEntityCacheStore<SupplierResponseDto>
{
}
