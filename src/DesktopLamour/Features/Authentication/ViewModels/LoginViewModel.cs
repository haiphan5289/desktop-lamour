// LoginViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.Authentication.Domain.Models;
using DesktopLamour.Features.Authentication.Domain.UseCases;
using DesktopLamour.Features.Realtime;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DesktopLamour.Features.Authentication.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly ILoginWithPhoneUseCase      _loginUseCase;
    private readonly IAuthTokenStorage           _tokenStorage;
    private readonly INavigationService          _navigationService;
    private readonly IPostLoginSyncService       _postLoginSync;
    private readonly ILogger<LoginViewModel>     _logger;

    private static readonly Regex PhoneRegex =
        new(@"^(03|05|07|08|09)\d{8}$", RegexOptions.Compiled);

    // ── Observable properties ────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _phoneError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _generalError;

    [ObservableProperty]
    private bool _isPasswordVisible;

    public LoginViewModel(
        ILoginWithPhoneUseCase  loginUseCase,
        IAuthTokenStorage       tokenStorage,
        INavigationService      navigationService,
        IPostLoginSyncService   postLoginSync,
        ILogger<LoginViewModel> logger)
    {
        _loginUseCase      = loginUseCase;
        _tokenStorage      = tokenStorage;
        _navigationService = navigationService;
        _postLoginSync     = postLoginSync;
        _logger            = logger;
    }

    // ── Partial method hooks for real-time validation ────────────────────────
    partial void OnPhoneNumberChanged(string value)
    {
        PhoneError = null;
        if (!string.IsNullOrEmpty(value) && !PhoneRegex.IsMatch(value))
            PhoneError = "Please enter a valid 10-digit phone number.";
    }

    partial void OnPasswordChanged(string value)
    {
        PasswordError = null;
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        IsLoading    = true;
        PhoneError   = null;
        PasswordError = null;
        GeneralError = null;

        try
        {
            var input = new LoginInput(PhoneNumber, Password);
            var user  = await _loginUseCase.ExecuteAsync(input, cancellationToken);

            if (!string.IsNullOrEmpty(user.AccessToken))
                _tokenStorage.SaveToken(user.AccessToken);

            // Fire-and-forget: warm the Customer/Employee cache + open the realtime
            // connection in the background so login navigation isn't blocked on it.
            _ = _postLoginSync.InitializeAsync(CancellationToken.None);

            _navigationService.NavigateToHome();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for field {Field}: {Message}", ex.Field, ex.Message);
            if (ex.Field == nameof(LoginInput.PhoneNumber))
                PhoneError = ex.Message;
            else if (ex.Field == nameof(LoginInput.Password))
                PasswordError = ex.Message;
            else
                GeneralError = ex.Message;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Login request cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed.");
            GeneralError = "Login failed. Please check your credentials and try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLogin()
        => !string.IsNullOrWhiteSpace(PhoneNumber)
           && PhoneRegex.IsMatch(PhoneNumber)
           && !string.IsNullOrWhiteSpace(Password)
           && !IsLoading;

    [RelayCommand]
    private void NavigateToRegister()
        => _navigationService.NavigateTo(NavigationRoutes.Register);

    [RelayCommand]
    private void TogglePasswordVisibility()
        => IsPasswordVisible = !IsPasswordVisible;
}
