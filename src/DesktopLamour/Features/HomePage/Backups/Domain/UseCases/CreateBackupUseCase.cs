// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Data.Repositories;
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public sealed class CreateBackupUseCase : ICreateBackupUseCase
{
    private readonly IBackupRepository _repository;
    public CreateBackupUseCase(IBackupRepository repository) => _repository = repository;

    public Task<BackupInfo> ExecuteAsync(CancellationToken ct = default)
        => _repository.CreateAsync(ct);
}
