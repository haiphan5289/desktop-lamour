// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Data.Repositories;
namespace DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;

public sealed class DeleteAccountSettingUseCase : IDeleteAccountSettingUseCase
{
    private readonly IAccountSettingRepository _repository;
    public DeleteAccountSettingUseCase(IAccountSettingRepository repository) => _repository = repository;
    public Task ExecuteAsync(int accountId, CancellationToken ct = default)
        => _repository.DeleteAsync(accountId, ct);
}
