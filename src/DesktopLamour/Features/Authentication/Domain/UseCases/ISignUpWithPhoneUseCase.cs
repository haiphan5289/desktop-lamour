// ISignUpWithPhoneUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.UseCases;
using DesktopLamour.Features.Authentication.Domain.Models;

namespace DesktopLamour.Features.Authentication.Domain.UseCases;

/// <summary>
/// Validates registration input and creates a new account.
/// Throws <see cref="Core.Exceptions.ValidationException"/> for invalid input.
/// </summary>
public interface ISignUpWithPhoneUseCase : IUseCase<RegisterInput, UserInfo>
{
}
