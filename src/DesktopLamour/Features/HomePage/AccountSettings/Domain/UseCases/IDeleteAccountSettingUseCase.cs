// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public interface IDeleteAccountSettingUseCase
{
    Task ExecuteAsync(int accountId, CancellationToken ct = default);
}
