// IAuthenticationRepository.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.Domain.Models;

namespace DesktopLamour.Features.Authentication.Data.Repositories;

public interface IAuthenticationRepository
{
    Task<bool>     CheckPhoneExistsAsync(string phone, CancellationToken cancellationToken = default);
    Task<UserInfo> SignUpAsync(RegisterInput input, CancellationToken cancellationToken = default);
    Task<UserInfo> LoginAsync(LoginInput input, CancellationToken cancellationToken = default);
}
