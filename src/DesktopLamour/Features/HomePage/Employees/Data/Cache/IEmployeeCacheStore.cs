// IEmployeeCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Employees.Data.Cache;

public interface IEmployeeCacheStore : IEntityCacheStore<EmployeeResponseDto>
{
}
