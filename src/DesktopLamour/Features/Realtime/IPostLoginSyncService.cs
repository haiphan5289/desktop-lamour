// IPostLoginSyncService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.Realtime;

public interface IPostLoginSyncService
{
    Task InitializeAsync(CancellationToken ct = default);
    Task ShutdownAsync();
}
