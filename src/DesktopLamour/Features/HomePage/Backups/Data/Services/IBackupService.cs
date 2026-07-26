// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Backups.Data.Services;

public interface IBackupService
{
    Task<IEnumerable<BackupResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<BackupResponseDto> CreateAsync(CancellationToken ct = default);
    Task DeleteAsync(string fileName, CancellationToken ct = default);
    Task RestoreAsync(string fileName, string password, CancellationToken ct = default);
    Task<BackupScheduleResponseDto> GetScheduleAsync(CancellationToken ct = default);
    Task<BackupScheduleResponseDto> UpdateScheduleAsync(UpdateBackupScheduleRequestDto request, CancellationToken ct = default);
}
