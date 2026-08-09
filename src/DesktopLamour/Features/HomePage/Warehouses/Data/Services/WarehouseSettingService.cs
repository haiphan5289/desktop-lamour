// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Warehouses.Data.Cache;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services;

public sealed class WarehouseSettingService : IWarehouseSettingService
{
    private readonly HttpClient                       _httpClient;
    private readonly IAuthTokenStorage                _tokenStorage;
    private readonly IWarehouseSettingCacheStore      _cache;
    private readonly ILogger<WarehouseSettingService> _logger;

    public WarehouseSettingService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        IWarehouseSettingCacheStore cache,
        ILogger<WarehouseSettingService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<WarehouseSettingResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning warehouses from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all warehouses from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/warehouses", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<WarehouseSettingResponseDto>>(ct)
            ?? Enumerable.Empty<WarehouseSettingResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<WarehouseSettingResponseDto> CreateAsync(CreateWarehouseSettingRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating warehouse '{Code}'", request.Code);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/warehouses", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<WarehouseSettingResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create warehouse endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<WarehouseSettingResponseDto> UpdateAsync(int warehouseId, UpdateWarehouseSettingRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating warehouse {Id}", warehouseId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/warehouses/{warehouseId}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var updated = await response.Content.ReadFromJsonAsync<WarehouseSettingResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update warehouse endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int warehouseId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting warehouse {Id}", warehouseId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/warehouses/{warehouseId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        _cache.Remove(warehouseId);
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(ct);
        throw new Exception(body?.Error ?? $"Lỗi {(int)response.StatusCode}");
    }

    private record ApiErrorResponse([property: System.Text.Json.Serialization.JsonPropertyName("error")] string? Error);
}
