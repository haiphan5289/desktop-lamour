// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.Backups.Data.Services.Dtos;

public class RestoreBackupRequestDto
{
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
}
