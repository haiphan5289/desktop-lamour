// ICustomerCacheStore.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Cache;
using DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Customers.Data.Cache;

public interface ICustomerCacheStore : IEntityCacheStore<CustomerResponseDto>
{
}
