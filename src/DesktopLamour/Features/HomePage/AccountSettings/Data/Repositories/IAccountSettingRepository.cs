// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
namespace DesktopLamour.Features.HomePage.AccountSettings.Data.Repositories;

public interface IAccountSettingRepository
{
    Task<IEnumerable<AccountSetting>> GetAllAsync(CancellationToken ct = default);
    Task<AccountSetting> CreateAsync(CreateAccountSettingInput input, CancellationToken ct = default);
    Task<AccountSetting> UpdateAsync(UpdateAccountSettingInput input, CancellationToken ct = default);
    Task DeleteAsync(int accountId, CancellationToken ct = default);
}
