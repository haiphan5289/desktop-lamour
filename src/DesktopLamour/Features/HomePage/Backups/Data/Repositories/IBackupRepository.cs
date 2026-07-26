// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
namespace DesktopLamour.Features.HomePage.Backups.Data.Repositories;

public interface IBackupRepository
{
    Task<IEnumerable<BackupInfo>> GetAllAsync(CancellationToken ct = default);
    Task<BackupInfo> CreateAsync(CancellationToken ct = default);
    Task DeleteAsync(string fileName, CancellationToken ct = default);
    Task RestoreAsync(string fileName, string password, CancellationToken ct = default);
    Task<BackupSchedule> GetScheduleAsync(CancellationToken ct = default);
    Task<BackupSchedule> UpdateScheduleAsync(BackupSchedule schedule, CancellationToken ct = default);
}
