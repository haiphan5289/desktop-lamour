// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<PaymentService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all payments from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/accounting/payments", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<PaymentResponseDto>>(ct)
            ?? Enumerable.Empty<PaymentResponseDto>();
    }

    public async Task<PaymentResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching payment {Id} from API", id);
        SetBearerToken();

        var response = await _httpClient.GetAsync($"/api/v1/accounting/payments/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentResponseDto>(ct);
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating payment for supplier {SupplierId}", request.SupplierId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/accounting/payments", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create payment endpoint.");
    }

    public async Task<PaymentResponseDto> UpdateAsync(int id, UpdatePaymentRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating payment {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/accounting/payments/{id}", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update payment endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting payment {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/accounting/payments/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PaymentResponseDto> DuplicateAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating payment {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/accounting/payments/{id}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate payment endpoint.");
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
