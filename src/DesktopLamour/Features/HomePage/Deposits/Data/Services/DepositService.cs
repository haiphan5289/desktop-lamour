// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services;

public sealed class DepositService : IDepositService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ILogger<DepositService> _logger;

    public DepositService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<DepositService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<DepositResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all deposits from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/deposits", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<DepositResponseDto>>(ct)
            ?? Enumerable.Empty<DepositResponseDto>();
    }

    public async Task<DepositResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposit {Id} from API", id);
        SetBearerToken();

        var response = await _httpClient.GetAsync($"/api/v1/deposits/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DepositResponseDto>(ct);
    }

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/deposits/next-code", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NextCodeResponse>(ct);
        return result?.Code ?? "DC00001";
    }

    public async Task<IEnumerable<DepositResponseDto>> GetByCustomerAsync(int customerId, int? excludeSalesOrderId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposits with remaining balance for customer {CustomerId}", customerId);
        SetBearerToken();

        var url = $"/api/v1/deposits/by-customer/{customerId}";
        if (excludeSalesOrderId.HasValue)
            url += $"?exclude_sales_order_id={excludeSalesOrderId.Value}";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<DepositResponseDto>>(ct)
            ?? Enumerable.Empty<DepositResponseDto>();
    }

    public async Task<DepositResponseDto> CreateAsync(CreateDepositRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating deposit for customer {CustomerId}", request.CustomerId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/deposits", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<DepositResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create deposit endpoint.");
    }

    public async Task<DepositResponseDto> UpdateAsync(int id, UpdateDepositRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating deposit {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/deposits/{id}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<DepositResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update deposit endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting deposit {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/deposits/{id}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
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

    private record NextCodeResponse(string Code);
    private record ApiErrorResponse([property: System.Text.Json.Serialization.JsonPropertyName("error")] string? Error);
}
