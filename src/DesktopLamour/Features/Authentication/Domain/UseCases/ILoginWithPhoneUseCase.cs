// ILoginWithPhoneUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.UseCases;
using DesktopLamour.Features.Authentication.Domain.Models;

namespace DesktopLamour.Features.Authentication.Domain.UseCases;

/// <summary>
/// Validates login input and authenticates an existing account.
/// Throws <see cref="Core.Exceptions.ValidationException"/> for invalid input.
/// </summary>
public interface ILoginWithPhoneUseCase : IUseCase<LoginInput, UserInfo>
{
}
