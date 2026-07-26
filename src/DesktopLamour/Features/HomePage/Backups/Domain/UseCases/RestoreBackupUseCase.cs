// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public sealed class RestoreBackupUseCase : IRestoreBackupUseCase
{
    private readonly IBackupRepository _repository;
    public RestoreBackupUseCase(IBackupRepository repository) => _repository = repository;

    public Task ExecuteAsync(string fileName, string password, CancellationToken ct = default)
        => _repository.RestoreAsync(fileName, password, ct);
}
