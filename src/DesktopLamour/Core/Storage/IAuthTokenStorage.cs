// IAuthTokenStorage.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Storage;

public interface IAuthTokenStorage
{
    void SaveToken(string accessToken);
    string? GetToken();
    void SaveRole(string? role);
    string? GetRole();
    void Clear();
    bool HasToken { get; }
}
