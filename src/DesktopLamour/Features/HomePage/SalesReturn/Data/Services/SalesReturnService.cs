// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Services;

public sealed class SalesReturnService : ISalesReturnService
{
    private readonly HttpClient                  _httpClient;
    private readonly IAuthTokenStorage           _tokenStorage;
    private readonly ILogger<SalesReturnService> _logger;

    public SalesReturnService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<SalesReturnService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<SalesReturnResponseDto>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales returns (fromDate={FromDate}, toDate={ToDate}, search={Search})", fromDate, toDate, search);
        SetBearerToken();

        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"from_date={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)   queryParams.Add($"to_date={toDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
        var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : "";

        var response = await _httpClient.GetAsync($"/api/v1/sales-returns{queryString}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<SalesReturnResponseDto>>(ct)
            ?? Enumerable.Empty<SalesReturnResponseDto>();
    }

    public async Task<SalesReturnResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales return {Id}", id);
        SetBearerToken();

        var response = await _httpClient.GetAsync($"/api/v1/sales-returns/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SalesReturnResponseDto>(ct);
    }

    public async Task<SalesReturnResponseDto> CreateAsync(CreateSalesReturnRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating sales return for customer {CustomerId}", request.CustomerId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/sales-returns", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<SalesReturnResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create sales-return endpoint.");
    }

    public async Task<SalesReturnResponseDto> UpdateAsync(int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating sales return {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/sales-returns/{id}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<SalesReturnResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update sales-return endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting sales return {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/sales-returns/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching next sales return code");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/sales-returns/next-code", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NextCodeResponse>(ct);
        return result?.Code ?? "BTL00001";
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
