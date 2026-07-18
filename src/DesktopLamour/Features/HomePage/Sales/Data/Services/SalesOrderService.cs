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
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create sales order endpoint.");
    }

    public async Task<SalesOrderResponseDto> UpdateAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating sales order {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/sales-orders/{id}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

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

    public async Task<IEnumerable<SalesOrderReportLineDto>> GetReportAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales order report");
        SetBearerToken();

        var queryParams = new List<string>();
        if (productIds is not null)
            foreach (var id in productIds) queryParams.Add($"product_ids={id}");
        if (employeeId.HasValue) queryParams.Add($"employee_id={employeeId.Value}");
        if (customerId.HasValue) queryParams.Add($"customer_id={customerId.Value}");
        if (!string.IsNullOrWhiteSpace(unit))     queryParams.Add($"unit={Uri.EscapeDataString(unit)}");
        if (!string.IsNullOrWhiteSpace(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");
        if (fromDate.HasValue)   queryParams.Add($"from_date={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)     queryParams.Add($"to_date={toDate.Value:yyyy-MM-dd}");

        var queryString = string.Join("&", queryParams);

        var response = await _httpClient.GetAsync($"/api/v1/sales-orders/report?{queryString}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<SalesOrderReportLineDto>>(ct)
            ?? Enumerable.Empty<SalesOrderReportLineDto>();
    }

    public async Task<IEnumerable<SalesOrderSummaryLineDto>> GetSummaryReportAsync(
        IEnumerable<int>? productIds, int? employeeId, int? customerId,
        string? unit, string? category,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales order summary report");
        SetBearerToken();

        var queryParams = new List<string>();
        if (productIds is not null)
            foreach (var id in productIds) queryParams.Add($"product_ids={id}");
        if (employeeId.HasValue) queryParams.Add($"employee_id={employeeId.Value}");
        if (customerId.HasValue) queryParams.Add($"customer_id={customerId.Value}");
        if (!string.IsNullOrWhiteSpace(unit))     queryParams.Add($"unit={Uri.EscapeDataString(unit)}");
        if (!string.IsNullOrWhiteSpace(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");
        if (fromDate.HasValue) queryParams.Add($"from_date={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)   queryParams.Add($"to_date={toDate.Value:yyyy-MM-dd}");

        var queryString = string.Join("&", queryParams);

        var response = await _httpClient.GetAsync($"/api/v1/sales-orders/summary-report?{queryString}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<SalesOrderSummaryLineDto>>(ct)
            ?? Enumerable.Empty<SalesOrderSummaryLineDto>();
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }

    public async Task<SalesOrderResponseDto> HoldAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Holding sales order {Id}", id);
        SetBearerToken();
        var response = await _httpClient.PutAsync($"/api/v1/sales-orders/{id}/hold", null, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<SalesOrderResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from hold endpoint.");
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
