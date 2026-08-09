// RealtimeSyncService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Cache;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Categories.Data.Cache;
using DesktopLamour.Features.HomePage.Categories.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Customers.Data.Cache;
using DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Employees.Data.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.ProductList.Data.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Cache;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Suppliers.Data.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouses.Data.Cache;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.Realtime;

/// <summary>
/// Wraps a SignalR connection to the BE's DataSyncHub. Server pushes Customer/Employee/
/// Product/Supplier/Category change events here so the local cache stores stay fresh without polling.
/// </summary>
public sealed class RealtimeSyncService : IRealtimeSyncService
{
    private readonly IAuthTokenStorage            _tokenStorage;
    private readonly ICustomerCacheStore          _customerCache;
    private readonly IEmployeeCacheStore          _employeeCache;
    private readonly IProductCacheStore           _productCache;
    private readonly ISupplierCacheStore          _supplierCache;
    private readonly ICategoryCacheStore          _categoryCache;
    private readonly IProductUnitCacheStore       _productUnitCache;
    private readonly IAccountSettingCacheStore    _accountSettingCache;
    private readonly IWarehouseSettingCacheStore  _warehouseCache;
    private readonly ILogger<RealtimeSyncService> _logger;
    private readonly string                       _serverUrl;
    private HubConnection?                        _connection;

    public RealtimeSyncService(
        IAuthTokenStorage tokenStorage,
        ICustomerCacheStore customerCache,
        IEmployeeCacheStore employeeCache,
        IProductCacheStore productCache,
        ISupplierCacheStore supplierCache,
        ICategoryCacheStore categoryCache,
        IProductUnitCacheStore productUnitCache,
        IAccountSettingCacheStore accountSettingCache,
        IWarehouseSettingCacheStore warehouseCache,
        ILogger<RealtimeSyncService> logger,
        string serverUrl)
    {
        _tokenStorage         = tokenStorage;
        _customerCache        = customerCache;
        _employeeCache        = employeeCache;
        _productCache         = productCache;
        _supplierCache        = supplierCache;
        _categoryCache        = categoryCache;
        _productUnitCache     = productUnitCache;
        _accountSettingCache  = accountSettingCache;
        _warehouseCache       = warehouseCache;
        _logger               = logger;
        _serverUrl            = serverUrl;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await StopAsync();

        // WebSocket transport can't carry an Authorization header, so the JWT is
        // passed via ?access_token= — the BE JwtBearer handler reads it from there.
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/hubs/data-sync", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_tokenStorage.GetToken());
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<CustomerResponseDto>("CustomerCreated", _customerCache.Upsert);
        _connection.On<CustomerResponseDto>("CustomerUpdated", _customerCache.Upsert);
        _connection.On<int>("CustomerDeleted", _customerCache.Remove);
        _connection.On("CustomersBulkChanged", _customerCache.Clear);

        _connection.On<EmployeeResponseDto>("EmployeeCreated", _employeeCache.Upsert);
        _connection.On<EmployeeResponseDto>("EmployeeUpdated", _employeeCache.Upsert);
        _connection.On<int>("EmployeeDeleted", _employeeCache.Remove);

        _connection.On<ProductResponseDto>("ProductCreated", _productCache.Upsert);
        _connection.On<ProductResponseDto>("ProductUpdated", _productCache.Upsert);
        _connection.On<int>("ProductDeleted", _productCache.Remove);

        _connection.On<SupplierResponseDto>("SupplierCreated", _supplierCache.Upsert);
        _connection.On<SupplierResponseDto>("SupplierUpdated", _supplierCache.Upsert);
        _connection.On<int>("SupplierDeleted", _supplierCache.Remove);

        _connection.On<CategoryResponseDto>("CategoryCreated", _categoryCache.Upsert);
        _connection.On<CategoryResponseDto>("CategoryUpdated", _categoryCache.Upsert);
        _connection.On<int>("CategoryDeleted", _categoryCache.Remove);

        _connection.On<ProductUnitResponseDto>("ProductUnitCreated", _productUnitCache.Upsert);
        _connection.On<ProductUnitResponseDto>("ProductUnitUpdated", _productUnitCache.Upsert);
        _connection.On<int>("ProductUnitDeleted", _productUnitCache.Remove);

        _connection.On<AccountSettingResponseDto>("AccountSettingCreated", _accountSettingCache.Upsert);
        _connection.On<AccountSettingResponseDto>("AccountSettingUpdated", _accountSettingCache.Upsert);
        _connection.On<int>("AccountSettingDeleted", _accountSettingCache.Remove);

        _connection.On<WarehouseSettingResponseDto>("WarehouseCreated", _warehouseCache.Upsert);
        _connection.On<WarehouseSettingResponseDto>("WarehouseUpdated", _warehouseCache.Upsert);
        _connection.On<int>("WarehouseDeleted", _warehouseCache.Remove);

        _connection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "Realtime sync connection lost, reconnecting...");
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync(ct);
            _logger.LogInformation("Realtime sync connected to {Url}", _serverUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start realtime sync connection; cache will stay static until reconnect.");
        }
    }

    public async Task StopAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
