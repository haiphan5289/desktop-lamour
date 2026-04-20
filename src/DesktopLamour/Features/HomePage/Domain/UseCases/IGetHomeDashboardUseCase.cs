// IGetHomeDashboardUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Domain.Models;

namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public interface IGetHomeDashboardUseCase
{
    Task<HomeDashboard> ExecuteAsync(CancellationToken ct = default);
}
