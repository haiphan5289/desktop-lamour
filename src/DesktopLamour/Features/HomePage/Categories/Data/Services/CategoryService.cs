// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Categories.Data.Cache;
using DesktopLamour.Features.HomePage.Categories.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Categories.Data.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ICategoryCacheStore     _cache;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ICategoryCacheStore cache,
        ILogger<CategoryService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning categories from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all categories from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/categories", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryResponseDto>>(ct)
            ?? Enumerable.Empty<CategoryResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating category '{Name}'", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/categories", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<CategoryResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create category endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<CategoryResponseDto> UpdateAsync(int categoryId, UpdateCategoryRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating category {Id}", categoryId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/categories/{categoryId}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var updated = await response.Content.ReadFromJsonAsync<CategoryResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update category endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting category {Id}", categoryId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/categories/{categoryId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        _cache.Remove(categoryId);
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
