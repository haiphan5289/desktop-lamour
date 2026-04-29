// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public sealed class ReceiptService : IReceiptService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<ReceiptService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<ReceiptResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all receipts from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/accounting/receipts", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<ReceiptResponseDto>>(ct)
            ?? Enumerable.Empty<ReceiptResponseDto>();
    }

    public async Task<ReceiptResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching receipt {Id} from API", id);
        SetBearerToken();

        var response = await _httpClient.GetAsync($"/api/v1/accounting/receipts/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReceiptResponseDto>(ct);
    }

    public async Task<ReceiptResponseDto> CreateAsync(CreateReceiptRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating receipt for customer {CustomerId}", request.CustomerId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/accounting/receipts", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create receipt endpoint.");
    }

    public async Task<ReceiptResponseDto> UpdateAsync(int id, UpdateReceiptRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/accounting/receipts/{id}", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update receipt endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/accounting/receipts/{id}", ct);
        response.EnsureSuccessStatusCode();
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
