// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public interface IGetAccountSettingsUseCase
{
    Task<IEnumerable<AccountSetting>> ExecuteAsync(CancellationToken ct = default);
}
