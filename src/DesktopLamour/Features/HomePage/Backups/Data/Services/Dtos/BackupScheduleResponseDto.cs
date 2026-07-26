// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Backups.Data.Services.Dtos;

public class BackupScheduleResponseDto
{
    [JsonPropertyName("is_enabled")]     public bool      IsEnabled     { get; set; }
    [JsonPropertyName("time_of_day")]    public string    TimeOfDay     { get; set; } = "02:00";
    [JsonPropertyName("interval_days")]  public int       IntervalDays  { get; set; } = 1;
    [JsonPropertyName("retention_days")] public int       RetentionDays { get; set; }
    [JsonPropertyName("directory")]      public string    Directory     { get; set; } = string.Empty;
    [JsonPropertyName("last_run_at")]    public DateTime? LastRunAt     { get; set; }
}
