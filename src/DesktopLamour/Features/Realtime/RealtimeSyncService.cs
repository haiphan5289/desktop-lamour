// RealtimeSyncService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Customers.Data.Cache;
using DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Employees.Data.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.ProductList.Data.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Suppliers.Data.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.Realtime;

/// <summary>
/// Wraps a SignalR connection to the BE's DataSyncHub. Server pushes Customer/Employee/
/// Product/Supplier change events here so the local cache stores stay fresh without polling.
/// </summary>
public sealed class RealtimeSyncService : IRealtimeSyncService
{
    private readonly IAuthTokenStorage            _tokenStorage;
    private readonly ICustomerCacheStore          _customerCache;
    private readonly IEmployeeCacheStore          _employeeCache;
    private readonly IProductCacheStore           _productCache;
    private readonly ISupplierCacheStore          _supplierCache;
    private readonly ILogger<RealtimeSyncService> _logger;
    private readonly string                       _serverUrl;
    private HubConnection?                        _connection;

    public RealtimeSyncService(
        IAuthTokenStorage tokenStorage,
        ICustomerCacheStore customerCache,
        IEmployeeCacheStore employeeCache,
        IProductCacheStore productCache,
        ISupplierCacheStore supplierCache,
        ILogger<RealtimeSyncService> logger,
        string serverUrl)
    {
        _tokenStorage  = tokenStorage;
        _customerCache = customerCache;
        _employeeCache = employeeCache;
        _productCache  = productCache;
        _supplierCache = supplierCache;
        _logger        = logger;
        _serverUrl     = serverUrl;
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
