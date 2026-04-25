// LoginWithPhoneUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Exceptions;
using DesktopLamour.Features.Authentication.Data.Repositories;
using DesktopLamour.Features.Authentication.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DesktopLamour.Features.Authentication.Domain.UseCases;

public class LoginWithPhoneUseCase : ILoginWithPhoneUseCase
{
    // Vietnamese phone: 10 digits starting with 03x, 05x, 07x, 08x, 09x
    private static readonly Regex PhoneRegex =
        new(@"^(03|05|07|08|09)\d{8}$", RegexOptions.Compiled);

    private readonly IAuthenticationRepository     _repository;
    private readonly ILogger<LoginWithPhoneUseCase> _logger;

    public LoginWithPhoneUseCase(
        IAuthenticationRepository      repository,
        ILogger<LoginWithPhoneUseCase> logger)
    {
        _repository = repository;
        _logger     = logger;
    }

    public async Task<UserInfo> ExecuteAsync(LoginInput input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing login for phone: {Phone}", input.PhoneNumber);
        Validate(input);
        return await _repository.LoginAsync(input, cancellationToken);
    }

    private static void Validate(LoginInput input)
    {
        if (string.IsNullOrWhiteSpace(input.PhoneNumber) || !PhoneRegex.IsMatch(input.PhoneNumber))
            throw new ValidationException(
                nameof(input.PhoneNumber),
                "Please enter a valid 10-digit phone number.");

        if (string.IsNullOrWhiteSpace(input.Password))
            throw new ValidationException(
                nameof(input.Password),
                "Password must not be empty.");
    }
}
