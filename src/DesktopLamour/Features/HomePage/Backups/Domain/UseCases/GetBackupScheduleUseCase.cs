// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Data.Repositories;
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public sealed class GetBackupScheduleUseCase : IGetBackupScheduleUseCase
{
    private readonly IBackupRepository _repository;
    public GetBackupScheduleUseCase(IBackupRepository repository) => _repository = repository;

    public Task<BackupSchedule> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetScheduleAsync(ct);
}
