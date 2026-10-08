// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Warehouse.Data.Services;

public sealed class WarehouseReceiptService : IWarehouseReceiptService
{
    private readonly HttpClient                       _httpClient;
    private readonly IAuthTokenStorage                _tokenStorage;
    private readonly ILogger<WarehouseReceiptService> _logger;

    public WarehouseReceiptService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<WarehouseReceiptService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<WarehouseReceiptResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all warehouse receipts from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/warehouse-receipts", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<WarehouseReceiptResponseDto>>(ct)
            ?? Enumerable.Empty<WarehouseReceiptResponseDto>();
    }

    public async Task<WarehouseReceiptResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching warehouse receipt {Id} from API", id);
        SetBearerToken();

        var response = await _httpClient.GetAsync($"/api/v1/warehouse-receipts/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WarehouseReceiptResponseDto>(ct);
    }

    public async Task<WarehouseReceiptResponseDto> CreateAsync(
        CreateWarehouseReceiptRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Creating warehouse receipt of type {ReceiptType}", request.ReceiptType);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/warehouse-receipts", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WarehouseReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create warehouse receipt endpoint.");
    }

    public async Task<WarehouseReceiptResponseDto> ConfirmAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Confirming warehouse receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/warehouse-receipts/{id}/confirm", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WarehouseReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from confirm warehouse receipt endpoint.");
    }

    public async Task<WarehouseReceiptResponseDto> UpdateAsync(
        int id,
        UpdateWarehouseReceiptRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Updating warehouse receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/warehouse-receipts/{id}", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WarehouseReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update warehouse receipt endpoint.");
    }

    public async Task<WarehouseReceiptResponseDto> UnconfirmAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Unconfirming warehouse receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/warehouse-receipts/{id}/unconfirm", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WarehouseReceiptResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from unconfirm warehouse receipt endpoint.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting warehouse receipt {Id}", id);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/warehouse-receipts/{id}", ct);
        if (response.IsSuccessStatusCode) return;

        // BE trả { "error": "..." } (vd. phiếu đã ghi sổ phải Bỏ ghi trước) — hiện đúng lý do thay vì mã lỗi HTTP trần.
        string? message = null;
        try { message = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(ct))?.Error; }
        catch { /* body không phải JSON — dùng thông báo mặc định bên dưới */ }
        throw new Exception(message ?? $"Lỗi {(int)response.StatusCode}");
    }

    // Số chứng từ kế tiếp — chỉ để hiện trước trên phiếu mới; số thật vẫn do BE sinh lúc Cất.
    public async Task<string?> GetNextNumberAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching next warehouse receipt number from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/warehouse-receipts/next-number", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NextCodeResponse>(ct);
        return result?.Code;
    }

    private sealed record NextCodeResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("code")] string? Code);

    private sealed record ApiErrorResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("error")] string? Error);

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }
}
