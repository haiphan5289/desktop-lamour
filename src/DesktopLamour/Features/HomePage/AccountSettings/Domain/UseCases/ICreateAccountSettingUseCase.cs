// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public interface ICreateAccountSettingUseCase
{
    Task<AccountSetting> ExecuteAsync(CreateAccountSettingInput input, CancellationToken ct = default);
}
