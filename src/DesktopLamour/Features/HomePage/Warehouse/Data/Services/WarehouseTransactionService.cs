// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public sealed class WarehouseTransactionService : IWarehouseTransactionService
{
    private readonly HttpClient                          _httpClient;
    private readonly IAuthTokenStorage                   _tokenStorage;
    private readonly ILogger<WarehouseTransactionService> _logger;

    public WarehouseTransactionService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<WarehouseTransactionService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<WarehouseTransactionResponseDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? type, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching warehouse transactions (Nhập/Xuất kho)");
        SetBearerToken();

        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"from_date={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)   queryParams.Add($"to_date={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(type)) queryParams.Add($"type={Uri.EscapeDataString(type)}");

        var queryString = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"/api/v1/warehouse-transactions?{queryString}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<WarehouseTransactionResponseDto>>(ct)
            ?? Enumerable.Empty<WarehouseTransactionResponseDto>();
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null ? new AuthenticationHeaderValue("Bearer", token) : null;
    }
}
