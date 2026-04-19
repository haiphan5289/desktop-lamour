// ICheckPhoneExistUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.UseCases;

namespace DesktopLamour.Features.Authentication.Domain.UseCases;

/// <summary>
/// Returns true if the given phone number already has a registered account.
/// </summary>
public interface ICheckPhoneExistUseCase : IUseCase<string, bool>
{
}
