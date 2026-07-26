// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public sealed class DeleteBackupUseCase : IDeleteBackupUseCase
{
    private readonly IBackupRepository _repository;
    public DeleteBackupUseCase(IBackupRepository repository) => _repository = repository;

    public Task ExecuteAsync(string fileName, CancellationToken ct = default)
        => _repository.DeleteAsync(fileName, ct);
}
