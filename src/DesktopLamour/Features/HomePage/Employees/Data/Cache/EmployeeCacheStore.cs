// EmployeeCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Employees.Data.Cache;

public sealed class EmployeeCacheStore : EntityCacheStore<EmployeeResponseDto>, IEmployeeCacheStore
{
    public EmployeeCacheStore() : base(e => e.Id)
    {
    }
}
