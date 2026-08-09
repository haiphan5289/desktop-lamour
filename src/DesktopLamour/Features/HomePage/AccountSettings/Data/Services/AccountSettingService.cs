// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Cache;
using DesktopLamour.Features.HomePage.AccountSettings.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.AccountSettings.Data.Services;

public sealed class AccountSettingService : IAccountSettingService
{
    private readonly HttpClient                    _httpClient;
    private readonly IAuthTokenStorage             _tokenStorage;
    private readonly IAccountSettingCacheStore     _cache;
    private readonly ILogger<AccountSettingService> _logger;

    public AccountSettingService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        IAccountSettingCacheStore cache,
        ILogger<AccountSettingService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<AccountSettingResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning account settings from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all account settings from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/account-settings", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<AccountSettingResponseDto>>(ct)
            ?? Enumerable.Empty<AccountSettingResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<AccountSettingResponseDto> CreateAsync(CreateAccountSettingRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating account setting '{Code}'", request.Code);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/account-settings", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<AccountSettingResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create account setting endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<AccountSettingResponseDto> UpdateAsync(int accountId, UpdateAccountSettingRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating account setting {Id}", accountId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/account-settings/{accountId}", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var updated = await response.Content.ReadFromJsonAsync<AccountSettingResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update account setting endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int accountId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting account setting {Id}", accountId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/account-settings/{accountId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        _cache.Remove(accountId);
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
