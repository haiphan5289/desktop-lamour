// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Warehouses.Data.Cache;
using DesktopLamour.Features.HomePage.Warehouses.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Warehouses.Data.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly HttpClient                  _httpClient;
    private readonly IAuthTokenStorage           _tokenStorage;
    private readonly IDepartmentCacheStore       _cache;
    private readonly ILogger<DepartmentService>  _logger;

    public DepartmentService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        IDepartmentCacheStore cache,
        ILogger<DepartmentService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning departments from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all departments from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/departments", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<DepartmentResponseDto>>(ct)
            ?? Enumerable.Empty<DepartmentResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating department '{Name}'", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/departments", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<DepartmentResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create department endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<DepartmentResponseDto> UpdateAsync(int departmentId, UpdateDepartmentRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating department {Id}", departmentId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/departments/{departmentId}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var updated = await response.Content.ReadFromJsonAsync<DepartmentResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update department endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int departmentId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting department {Id}", departmentId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/departments/{departmentId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        _cache.Remove(departmentId);
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
