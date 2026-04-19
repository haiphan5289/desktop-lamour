// RegisterInput.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.Authentication.Domain.Models;

public record RegisterInput(
    string  PhoneNumber,
    string  Password,
    string? DisplayName = null
);
