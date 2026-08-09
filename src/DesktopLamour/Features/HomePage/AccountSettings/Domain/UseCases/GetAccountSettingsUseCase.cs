// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Data.Repositories;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public sealed class GetAccountSettingsUseCase : IGetAccountSettingsUseCase
{
    private readonly IAccountSettingRepository _repository;
    public GetAccountSettingsUseCase(IAccountSettingRepository repository) => _repository = repository;

    public Task<IEnumerable<AccountSetting>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
