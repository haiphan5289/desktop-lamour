// IRealtimeSyncService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.Realtime;

public interface IRealtimeSyncService
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
}
