// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
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
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<SupplierService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all suppliers from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/suppliers", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<SupplierResponseDto>>(ct)
            ?? Enumerable.Empty<SupplierResponseDto>();
    }

    public async Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating supplier with code {Code}", request.Code);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/suppliers", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SupplierResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create supplier endpoint.");
    }

    public async Task<SupplierResponseDto> UpdateAsync(int supplierId, UpdateSupplierRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating supplier {Id}", supplierId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/suppliers/{supplierId}", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SupplierResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update supplier endpoint.");
    }

    public async Task DeleteAsync(int supplierId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting supplier {Id}", supplierId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/suppliers/{supplierId}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SupplierResponseDto> DuplicateAsync(int supplierId, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating supplier {Id}", supplierId);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/suppliers/{supplierId}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SupplierResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate supplier endpoint.");
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
