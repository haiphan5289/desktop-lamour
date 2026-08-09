// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services;

public sealed class DepositDeductionService : IDepositDeductionService
{
    private readonly HttpClient                       _httpClient;
    private readonly IAuthTokenStorage                _tokenStorage;
    private readonly ILogger<DepositDeductionService> _logger;

    public DepositDeductionService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<DepositDeductionService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<DepositDeductionResponseDto>> GetAllAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching deposit deduction report");
        SetBearerToken();

        var queryParams = new List<string>();
        if (customerId.HasValue)   queryParams.Add($"customer_id={customerId.Value}");
        if (employeeId.HasValue)   queryParams.Add($"employee_id={employeeId.Value}");
        if (salesOrderId.HasValue) queryParams.Add($"sales_order_id={salesOrderId.Value}");
        if (fromDate.HasValue)     queryParams.Add($"from_date={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)       queryParams.Add($"to_date={toDate.Value:yyyy-MM-dd}");

        var url = "/api/v1/deposit-deductions";
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<DepositDeductionResponseDto>>(ct)
            ?? Enumerable.Empty<DepositDeductionResponseDto>();
    }

    public async Task<DepositDeductionResponseDto> CreateAsync(CreateDepositDeductionRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating deposit deduction for deposit {DepositId}, sales order {SalesOrderId}",
            request.DepositId, request.SalesOrderId);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/deposit-deductions", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<DepositDeductionResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create deposit deduction endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting deposit deduction {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/deposit-deductions/{id}", ct);
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

    private record ApiErrorResponse([property: System.Text.Json.Serialization.JsonPropertyName("error")] string? Error);
}
