// CheckPhoneRequestDto.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Text.Json.Serialization;

namespace DesktopLamour.Features.Authentication.Data.Services.Dtos;

internal record CheckPhoneRequestDto(
    [property: JsonPropertyName("phone")] string Phone
);
