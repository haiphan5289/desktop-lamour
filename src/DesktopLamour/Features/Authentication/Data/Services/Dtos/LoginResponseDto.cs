// LoginResponseDto.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Text.Json.Serialization;

namespace DesktopLamour.Features.Authentication.Data.Services.Dtos;

internal record LoginResponseDto(
    [property: JsonPropertyName("user_id")]      int    UserId,
    [property: JsonPropertyName("phone")]        string Phone,
    [property: JsonPropertyName("name")]         string Name,
    [property: JsonPropertyName("role")]         string Role,
    [property: JsonPropertyName("access_token")] string AccessToken
);
