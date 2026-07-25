// PostLoginSyncService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Customers.Data.Cache;
using DesktopLamour.Features.HomePage.Customers.Data.Services;
using DesktopLamour.Features.HomePage.Employees.Data.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Services;
using DesktopLamour.Features.HomePage.ProductList.Data.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Services;
using DesktopLamour.Features.HomePage.Suppliers.Data.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.Realtime;

/// <summary>
/// Warms up the Customer/Employee/Product/Supplier local caches and opens the realtime
/// sync connection right after login; tears both down on logout so a different user
/// doesn't inherit another account's cached data.
/// </summary>
public sealed class PostLoginSyncService : IPostLoginSyncService
{
    private readonly ICustomerService             _customerService;
    private readonly IEmployeeService             _employeeService;
    private readonly IProductService              _productService;
    private readonly ISupplierService              _supplierService;
    private readonly ICustomerCacheStore          _customerCache;
    private readonly IEmployeeCacheStore          _employeeCache;
    private readonly IProductCacheStore           _productCache;
    private readonly ISupplierCacheStore          _supplierCache;
    private readonly IRealtimeSyncService         _realtimeSync;
    private readonly ILogger<PostLoginSyncService> _logger;

    public PostLoginSyncService(
        ICustomerService customerService,
        IEmployeeService employeeService,
        IProductService productService,
        ISupplierService supplierService,
        ICustomerCacheStore customerCache,
        IEmployeeCacheStore employeeCache,
        IProductCacheStore productCache,
        ISupplierCacheStore supplierCache,
        IRealtimeSyncService realtimeSync,
        ILogger<PostLoginSyncService> logger)
    {
        _customerService = customerService;
        _employeeService = employeeService;
        _productService  = productService;
        _supplierService = supplierService;
        _customerCache   = customerCache;
        _employeeCache   = employeeCache;
        _productCache    = productCache;
        _supplierCache   = supplierCache;
        _realtimeSync    = realtimeSync;
        _logger          = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            await Task.WhenAll(
                _customerService.GetAllAsync(ct),
                _employeeService.GetAllAsync(ct),
                _productService.GetAllAsync(ct),
                _supplierService.GetAllAsync(ct));

            await _realtimeSync.StartAsync(ct);
            _logger.LogInformation("Post-login cache warmup + realtime sync started.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Post-login sync warmup failed; features will fall back to per-call API fetches.");
        }
    }

    public async Task ShutdownAsync()
    {
        await _realtimeSync.StopAsync();
        _customerCache.Clear();
        _employeeCache.Clear();
        _productCache.Clear();
        _supplierCache.Clear();
    }
}
