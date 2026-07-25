// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Suppliers.Data.Cache;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Suppliers.Data.Services;

public sealed class SupplierService : ISupplierService
{
    private readonly HttpClient            _httpClient;
    private readonly IAuthTokenStorage     _tokenStorage;
    private readonly ISupplierCacheStore   _cache;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ISupplierCacheStore cache,
        ILogger<SupplierService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning suppliers from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all suppliers from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/suppliers", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<SupplierResponseDto>>(ct)
            ?? Enumerable.Empty<SupplierResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating supplier with code {Code}", request.Code);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/suppliers", request, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<SupplierResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create supplier endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<SupplierResponseDto> UpdateAsync(int supplierId, UpdateSupplierRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating supplier {Id}", supplierId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/suppliers/{supplierId}", request, ct);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<SupplierResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update supplier endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int supplierId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting supplier {Id}", supplierId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/suppliers/{supplierId}", ct);
        response.EnsureSuccessStatusCode();
        _cache.Remove(supplierId);
    }

    public async Task<SupplierResponseDto> DuplicateAsync(int supplierId, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating supplier {Id}", supplierId);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/suppliers/{supplierId}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<SupplierResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate supplier endpoint.");
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
