// RegisterRequestDto.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Text.Json.Serialization;

namespace DesktopLamour.Features.Authentication.Data.Services.Dtos;

internal record RegisterRequestDto(
    [property: JsonPropertyName("phone")]        string  Phone,
    [property: JsonPropertyName("password")]     string  Password,
    [property: JsonPropertyName("display_name")] string? DisplayName
);
