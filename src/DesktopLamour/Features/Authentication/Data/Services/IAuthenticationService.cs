// IAuthenticationService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.Domain.Models;

namespace DesktopLamour.Features.Authentication.Data.Services;

public interface IAuthenticationService
{
    /// <summary>Returns true if the phone number already has an account.</summary>
    Task<bool> CheckPhoneExistsAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>Creates a new account and returns the authenticated user.</summary>
    Task<UserInfo> RegisterAsync(RegisterInput input, CancellationToken cancellationToken = default);

    /// <summary>Authenticates an existing account and returns the authenticated user.</summary>
    Task<UserInfo> LoginAsync(LoginInput input, CancellationToken cancellationToken = default);
}
