// SignUpWithPhoneUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.Authentication.Data.Repositories;
using DesktopLamour.Features.Authentication.Domain.Models;
using System.Text.RegularExpressions;

namespace DesktopLamour.Features.Authentication.Domain.UseCases;

public class SignUpWithPhoneUseCase : ISignUpWithPhoneUseCase
{
    // Vietnamese phone: 10 digits starting with 03x, 05x, 07x, 08x, 09x
    private static readonly Regex PhoneRegex =
        new(@"^(03|05|07|08|09)\d{8}$", RegexOptions.Compiled);

    private readonly IAuthenticationRepository _repository;

    public SignUpWithPhoneUseCase(IAuthenticationRepository repository)
        => _repository = repository;

    public async Task<UserInfo> ExecuteAsync(RegisterInput input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        return await _repository.SignUpAsync(input, cancellationToken);
    }

    private static void Validate(RegisterInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PhoneNumber) || !PhoneRegex.IsMatch(input.PhoneNumber))
            throw new ValidationException(
                nameof(input.PhoneNumber),
                "Please enter a valid 10-digit phone number.");

        if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 8)
            throw new ValidationException(
                nameof(input.Password),
                "Password must be at least 8 characters.");

        if (!input.Password.Any(char.IsUpper) || !input.Password.Any(char.IsDigit))
            throw new ValidationException(
                nameof(input.Password),
                "Password must contain at least one uppercase letter and one number.");
    }
}
