// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public sealed class PaymentReceiptService : IPaymentReceiptService
{
    private readonly HttpClient                    _httpClient;
    private readonly IAuthTokenStorage             _tokenStorage;
    private readonly ILogger<PaymentReceiptService> _logger;

    public PaymentReceiptService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<PaymentReceiptService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<PaymentReceiptResponseDto> CreateAsync(
        CreatePaymentReceiptRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Creating payment receipt for customer {CustomerId}", request.CustomerId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/accounting/payment-receipts", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentReceiptResponseDto>(ct)
            ?? new PaymentReceiptResponseDto();
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
