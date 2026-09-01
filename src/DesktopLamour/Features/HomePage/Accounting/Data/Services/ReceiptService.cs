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

    public async Task<ReceiptResponseDto> ConfirmAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Confirming receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/accounting/receipts/{id}/confirm", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from confirm receipt endpoint.");
    }

    public async Task<ReceiptResponseDto> UnconfirmAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Unconfirming receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/accounting/receipts/{id}/unconfirm", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from unconfirm receipt endpoint.");
    }

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching next receipt code from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/accounting/receipts/next-code", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NextCodeResponse>(ct);
        return result?.Code ?? "PT00001";
    }

    public async Task<IEnumerable<OutstandingSalesOrderDto>> GetOutstandingSalesOrdersAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching outstanding sales orders {From} → {To}", fromDate, toDate);
        SetBearerToken();

        var url = $"/api/v1/accounting/receipts/outstanding-orders?from_date={fromDate:yyyy-MM-dd}&to_date={toDate:yyyy-MM-dd}";
        if (employeeId.HasValue)
            url += $"&employee_id={employeeId.Value}";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<OutstandingSalesOrderDto>>(ct)
            ?? Enumerable.Empty<OutstandingSalesOrderDto>();
    }

    public async Task<CreateBulkCustomerReceiptResponseDto> CreateBulkAsync(
        CreateBulkCustomerReceiptRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating bulk customer receipt covering {Count} sales orders", request.Lines.Count);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/accounting/receipts/bulk", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateBulkCustomerReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create bulk receipt endpoint.");
    }

    private record NextCodeResponse(string Code);

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }
}
