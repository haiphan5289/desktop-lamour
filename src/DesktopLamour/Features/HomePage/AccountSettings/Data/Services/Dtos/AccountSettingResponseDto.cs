// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.AccountSettings.Data.Services.Dtos;

public class AccountSettingResponseDto
{
    [JsonPropertyName("id")]          public int    Id          { get; set; }
    [JsonPropertyName("code")]        public string Code        { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
}
