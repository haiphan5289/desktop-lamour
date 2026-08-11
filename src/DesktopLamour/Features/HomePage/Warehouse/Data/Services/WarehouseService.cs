// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public sealed class WarehouseService : IWarehouseService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<WarehouseService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<InventorySummaryItemDto>> GetInventorySummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<int>? warehouseIds = null,
        int? categoryId = null,
        int? productUnitId = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching inventory summary {From} → {To}", fromDate, toDate);
        SetBearerToken();

        var url = $"/api/v1/inventory/summary?from_date={fromDate:yyyy-MM-dd}&to_date={toDate:yyyy-MM-dd}";
        if (warehouseIds is { Count: > 0 })
            url += string.Concat(warehouseIds.Select(id => $"&warehouse_ids={id}"));
        if (categoryId.HasValue)
            url += $"&category_id={categoryId.Value}";
        if (productUnitId.HasValue)
            url += $"&product_unit_id={productUnitId.Value}";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<InventorySummaryItemDto>>(ct)
            ?? Enumerable.Empty<InventorySummaryItemDto>();
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }
}
