// RealtimeServiceCollectionExtensions.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Cache;
using DesktopLamour.Features.HomePage.Categories.Data.Cache;
using DesktopLamour.Features.HomePage.Customers.Data.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Cache;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Cache;
using DesktopLamour.Features.HomePage.Warehouses.Data.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.Realtime;

public static class RealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeModule(this IServiceCollection services, string serverUrl)
    {
        services.AddSingleton<IRealtimeSyncService>(sp => new RealtimeSyncService(
            sp.GetRequiredService<IAuthTokenStorage>(),
            sp.GetRequiredService<ICustomerCacheStore>(),
            sp.GetRequiredService<IEmployeeCacheStore>(),
            sp.GetRequiredService<IProductCacheStore>(),
            sp.GetRequiredService<ISupplierCacheStore>(),
            sp.GetRequiredService<ICategoryCacheStore>(),
            sp.GetRequiredService<IProductUnitCacheStore>(),
            sp.GetRequiredService<IAccountSettingCacheStore>(),
            sp.GetRequiredService<IWarehouseSettingCacheStore>(),
            sp.GetRequiredService<IDepartmentCacheStore>(),
            sp.GetRequiredService<IExpenseCategoryCacheStore>(),
            sp.GetRequiredService<ILogger<RealtimeSyncService>>(),
            serverUrl));

        services.AddSingleton<IPostLoginSyncService, PostLoginSyncService>();

        return services;
    }
}
