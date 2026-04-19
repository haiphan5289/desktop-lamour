// AuthenticationRepository.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.Data.Services;
using DesktopLamour.Features.Authentication.Domain.Models;

namespace DesktopLamour.Features.Authentication.Data.Repositories;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly IAuthenticationService _service;

    public AuthenticationRepository(IAuthenticationService service)
        => _service = service;

    public Task<bool> CheckPhoneExistsAsync(string phone, CancellationToken cancellationToken = default)
        => _service.CheckPhoneExistsAsync(phone, cancellationToken);

    public Task<UserInfo> SignUpAsync(RegisterInput input, CancellationToken cancellationToken = default)
        => _service.RegisterAsync(input, cancellationToken);
}
