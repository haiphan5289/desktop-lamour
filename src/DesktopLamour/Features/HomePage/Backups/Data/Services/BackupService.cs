// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Backups.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Backups.Data.Services;

public sealed class BackupService : IBackupService
{
    private readonly HttpClient             _httpClient;
    private readonly IAuthTokenStorage      _tokenStorage;
    private readonly ILogger<BackupService> _logger;

    public BackupService(HttpClient httpClient, IAuthTokenStorage tokenStorage, ILogger<BackupService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<BackupResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all backups from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/backups", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<IEnumerable<BackupResponseDto>>(ct)
            ?? Enumerable.Empty<BackupResponseDto>();
    }

    public async Task<BackupResponseDto> CreateAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Creating a new backup");
        SetBearerToken();

        var response = await _httpClient.PostAsync("/api/v1/backups", null, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<BackupResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create backup endpoint.");
    }

    public async Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting backup {File}", fileName);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/backups/{Uri.EscapeDataString(fileName)}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task RestoreAsync(string fileName, string password, CancellationToken ct = default)
    {
        _logger.LogWarning("Restoring database from backup {File}", fileName);
        SetBearerToken();

        var request  = new RestoreBackupRequestDto { Password = password };
        var response = await _httpClient.PostAsJsonAsync($"/api/v1/backups/{Uri.EscapeDataString(fileName)}/restore", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<BackupScheduleResponseDto> GetScheduleAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching backup schedule");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/backup-schedule", ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<BackupScheduleResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from get backup schedule endpoint.");
    }

    public async Task<BackupScheduleResponseDto> UpdateScheduleAsync(UpdateBackupScheduleRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating backup schedule");
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync("/api/v1/backup-schedule", request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<BackupScheduleResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update backup schedule endpoint.");
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
