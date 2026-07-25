// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.ProductList.Data.Cache;
using DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.ProductList.Data.Services;

public sealed class ProductService : IProductService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly IProductCacheStore      _cache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        IProductCacheStore cache,
        ILogger<ProductService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning products from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all products from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/products", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ProductResponseDto>>(ct)
            ?? Enumerable.Empty<ProductResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating product '{Name}'", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/products", request, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ProductResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create product endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<ProductResponseDto> UpdateAsync(int productId, UpdateProductRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating product {Id}", productId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/products/{productId}", request, ct);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<ProductResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update product endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int productId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting product {Id}", productId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/products/{productId}", ct);
        response.EnsureSuccessStatusCode();
        _cache.Remove(productId);
    }

    public async Task<ProductResponseDto> DuplicateAsync(int productId, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating product {Id}", productId);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/products/{productId}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ProductResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate product endpoint.");
        _cache.Upsert(created);
        return created;
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
