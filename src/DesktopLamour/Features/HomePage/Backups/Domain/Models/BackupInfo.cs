// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Backups.Domain.Models;

public class BackupInfo
{
    public string   FileName  { get; set; } = string.Empty;
    public long     SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string SizeDisplay => SizeBytes switch
    {
        >= 1024 * 1024 => $"{SizeBytes / (1024.0 * 1024.0):F1} MB",
        >= 1024        => $"{SizeBytes / 1024.0:F1} KB",
        _              => $"{SizeBytes} B",
    };
}
