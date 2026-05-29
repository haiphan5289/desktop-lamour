// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Sales.Data.Services;

public sealed class SalesOrderService : ISalesOrderService
{
    private readonly HttpClient                _httpClient;
    private readonly IAuthTokenStorage         _tokenStorage;
    private readonly ILogger<SalesOrderService> _logger;

    public SalesOrderService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<SalesOrderService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<SalesOrderResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all sales orders");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/sales-orders", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<SalesOrderResponseDto>>(ct)
            ?? Enumerable.Empty<SalesOrderResponseDto>();
    }

    public async Task<SalesOrderResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales order {Id}", id);
        SetBearerToken();

        var response = await _httpClient.GetAsync($"/api/v1/sales-orders/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>(ct);
    }

    public async Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating sales order for customer {CustomerId}", request.CustomerId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/sales-orders", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create sales order endpoint.");
    }

    public async Task<SalesOrderResponseDto> UpdateAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating sales order {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/sales-orders/{id}", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update sales order endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting sales order {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/sales-orders/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching next sales order code");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/sales-orders/next-code", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NextCodeResponse>(ct);
        return result?.Code ?? "BC00001";
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }

    private record NextCodeResponse(string Code);
}
