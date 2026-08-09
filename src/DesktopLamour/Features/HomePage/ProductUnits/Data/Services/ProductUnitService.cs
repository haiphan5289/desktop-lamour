// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Cache;
using DesktopLamour.Features.HomePage.ProductUnits.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.ProductUnits.Data.Services;

public sealed class ProductUnitService : IProductUnitService
{
    private readonly HttpClient                 _httpClient;
    private readonly IAuthTokenStorage          _tokenStorage;
    private readonly IProductUnitCacheStore     _cache;
    private readonly ILogger<ProductUnitService> _logger;

    public ProductUnitService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        IProductUnitCacheStore cache,
        ILogger<ProductUnitService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<ProductUnitResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning product units from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all product units from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/product-units", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ProductUnitResponseDto>>(ct)
            ?? Enumerable.Empty<ProductUnitResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<ProductUnitResponseDto> CreateAsync(CreateProductUnitRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating product unit '{Name}'", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/product-units", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<ProductUnitResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create product unit endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<ProductUnitResponseDto> UpdateAsync(int unitId, UpdateProductUnitRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating product unit {Id}", unitId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/product-units/{unitId}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var updated = await response.Content.ReadFromJsonAsync<ProductUnitResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update product unit endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int unitId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting product unit {Id}", unitId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/product-units/{unitId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        _cache.Remove(unitId);
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
