// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Backups.Data.Services.Dtos;

public class BackupResponseDto
{
    [JsonPropertyName("file_name")]  public string   FileName  { get; set; } = string.Empty;
    [JsonPropertyName("size_bytes")] public long     SizeBytes { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
}
