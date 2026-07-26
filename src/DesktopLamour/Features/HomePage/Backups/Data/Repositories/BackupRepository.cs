// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.Data.Services;
using DesktopLamour.Features.HomePage.Backups.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
namespace DesktopLamour.Features.HomePage.Backups.Data.Repositories;

public sealed class BackupRepository : IBackupRepository
{
    private readonly IBackupService _service;
    public BackupRepository(IBackupService service) => _service = service;

    public async Task<IEnumerable<BackupInfo>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public async Task<BackupInfo> CreateAsync(CancellationToken ct = default)
    {
        var d = await _service.CreateAsync(ct);
        return MapToModel(d);
    }

    public Task DeleteAsync(string fileName, CancellationToken ct = default)
        => _service.DeleteAsync(fileName, ct);

    public Task RestoreAsync(string fileName, string password, CancellationToken ct = default)
        => _service.RestoreAsync(fileName, password, ct);

    public async Task<BackupSchedule> GetScheduleAsync(CancellationToken ct = default)
    {
        var d = await _service.GetScheduleAsync(ct);
        return MapScheduleToModel(d);
    }

    public async Task<BackupSchedule> UpdateScheduleAsync(BackupSchedule schedule, CancellationToken ct = default)
    {
        var request = new UpdateBackupScheduleRequestDto
        {
            IsEnabled     = schedule.IsEnabled,
            TimeOfDay     = schedule.TimeOfDay,
            IntervalDays  = schedule.IntervalDays,
            RetentionDays = schedule.RetentionDays,
            Directory     = schedule.Directory,
        };
        var d = await _service.UpdateScheduleAsync(request, ct);
        return MapScheduleToModel(d);
    }

    private static BackupInfo MapToModel(BackupResponseDto d) => new()
    {
        FileName  = d.FileName,
        SizeBytes = d.SizeBytes,
        CreatedAt = DateTime.SpecifyKind(d.CreatedAt, DateTimeKind.Utc).ToLocalTime(),
    };

    private static BackupSchedule MapScheduleToModel(BackupScheduleResponseDto d) => new()
    {
        IsEnabled     = d.IsEnabled,
        TimeOfDay     = d.TimeOfDay,
        IntervalDays  = d.IntervalDays,
        RetentionDays = d.RetentionDays,
        Directory     = d.Directory,
        LastRunAt     = d.LastRunAt.HasValue
            ? DateTime.SpecifyKind(d.LastRunAt.Value, DateTimeKind.Utc).ToLocalTime()
            : null,
    };
}
