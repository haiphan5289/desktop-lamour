// InMemoryAuthTokenStorage.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Storage;

/// <summary>
/// In-memory token storage. Replace with encrypted persistent storage for production.
/// </summary>
public class InMemoryAuthTokenStorage : IAuthTokenStorage
{
    private string? _accessToken;
    private string? _role;

    public bool HasToken => !string.IsNullOrEmpty(_accessToken);

    public void SaveToken(string accessToken)
        => _accessToken = accessToken;

    public string? GetToken()
        => _accessToken;

    public void SaveRole(string? role)
        => _role = role;

    public string? GetRole()
        => _role;

    public void Clear()
    {
        _accessToken = null;
        _role        = null;
    }
}
