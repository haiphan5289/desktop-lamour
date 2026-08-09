// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public interface IUpdateAccountSettingUseCase
{
    Task<AccountSetting> ExecuteAsync(UpdateAccountSettingInput input, CancellationToken ct = default);
}
