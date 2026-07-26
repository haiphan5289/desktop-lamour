// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public interface ICreateBackupUseCase
{
    Task<BackupInfo> ExecuteAsync(CancellationToken ct = default);
}
