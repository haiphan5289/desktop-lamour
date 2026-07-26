// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Backups.Domain.Models;

public class BackupSchedule
{
    public bool      IsEnabled     { get; set; }
    public string    TimeOfDay     { get; set; } = "02:00";
    public int       IntervalDays  { get; set; } = 1;
    public int       RetentionDays { get; set; } = 30;
    public string    Directory     { get; set; } = string.Empty;
    public DateTime? LastRunAt     { get; set; }
}
