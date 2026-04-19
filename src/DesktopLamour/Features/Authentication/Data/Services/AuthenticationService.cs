// AuthenticationService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.Data.Services.Dtos;
using DesktopLamour.Features.Authentication.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DesktopLamour.Features.Authentication.Data.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _logger     = logger;
    }

    public async Task<bool> CheckPhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if phone exists: {Phone}", phone);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/auth/check-phone",
            new CheckPhoneRequestDto(phone),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<CheckPhoneResponseDto>(
            cancellationToken: cancellationToken);

        return dto?.Exists ?? false;
    }

    public async Task<UserInfo> RegisterAsync(RegisterInput input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering new account for phone: {Phone}", input.PhoneNumber);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequestDto(input.PhoneNumber, input.Password, input.DisplayName),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from register endpoint.");

        return new UserInfo
        {
            UserId       = dto.UserId,
            Phone        = dto.Phone,
            Name         = dto.Name,
            AccessToken  = dto.AccessToken,
            CreatedAt    = DateTime.UtcNow,
        };
    }
}
