// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.HomePage.Backups.Data.Repositories;
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
using System.Text.RegularExpressions;
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public sealed class UpdateBackupScheduleUseCase : IUpdateBackupScheduleUseCase
{
    private static readonly Regex TimeRegex = new(@"^([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    private readonly IBackupRepository _repository;
    public UpdateBackupScheduleUseCase(IBackupRepository repository) => _repository = repository;

    public Task<BackupSchedule> ExecuteAsync(BackupSchedule schedule, CancellationToken ct = default)
    {
        if (!TimeRegex.IsMatch(schedule.TimeOfDay))
            throw new ValidationException(nameof(schedule.TimeOfDay), "Giờ chạy backup không hợp lệ, định dạng phải là HH:mm.");

        if (schedule.RetentionDays <= 0)
            throw new ValidationException(nameof(schedule.RetentionDays), "Số ngày giữ bản sao lưu phải lớn hơn 0.");

        if (schedule.IntervalDays <= 0)
            throw new ValidationException(nameof(schedule.IntervalDays), "Số ngày giữa 2 lần chạy backup phải lớn hơn 0.");

        if (string.IsNullOrWhiteSpace(schedule.Directory))
            throw new ValidationException(nameof(schedule.Directory), "Thư mục lưu trữ không được để trống.");

        return _repository.UpdateScheduleAsync(schedule, ct);
    }
}
