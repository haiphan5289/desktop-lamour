// LoginInput.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.Authentication.Domain.Models;

public record LoginInput(
    string PhoneNumber,
    string Password
);
