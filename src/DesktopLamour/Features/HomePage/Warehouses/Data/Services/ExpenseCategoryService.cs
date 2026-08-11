// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Warehouses.Data.Cache;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services;

public sealed class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly HttpClient                       _httpClient;
    private readonly IAuthTokenStorage                _tokenStorage;
    private readonly IExpenseCategoryCacheStore       _cache;
    private readonly ILogger<ExpenseCategoryService>  _logger;

    public ExpenseCategoryService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        IExpenseCategoryCacheStore cache,
        ILogger<ExpenseCategoryService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<ExpenseCategoryResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning expense categories from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all expense categories from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/expense-categories", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ExpenseCategoryResponseDto>>(ct)
            ?? Enumerable.Empty<ExpenseCategoryResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<ExpenseCategoryResponseDto> CreateAsync(CreateExpenseCategoryRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating expense category '{Code}'", request.Code);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/expense-categories", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<ExpenseCategoryResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create expense category endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<ExpenseCategoryResponseDto> UpdateAsync(int categoryId, UpdateExpenseCategoryRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating expense category {Id}", categoryId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/expense-categories/{categoryId}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var updated = await response.Content.ReadFromJsonAsync<ExpenseCategoryResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update expense category endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting expense category {Id}", categoryId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/expense-categories/{categoryId}", ct);
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
