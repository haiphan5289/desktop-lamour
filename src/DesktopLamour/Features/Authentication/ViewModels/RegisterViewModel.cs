// RegisterViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.Authentication.Domain.Models;
using DesktopLamour.Features.Authentication.Domain.UseCases;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DesktopLamour.Features.Authentication.ViewModels;

public enum RegisterStep { Phone, Password }

public partial class RegisterViewModel : ViewModelBase
{
    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly ICheckPhoneExistUseCase _checkPhoneExistUseCase;
    private readonly ISignUpWithPhoneUseCase _signUpUseCase;
    private readonly INavigationService      _navigationService;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ILogger<RegisterViewModel> _logger;

    private static readonly Regex PhoneRegex =
        new(@"^(03|05|07|08|09)\d{8}$", RegexOptions.Compiled);

    // ── Observable properties ────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _phoneError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _confirmPasswordError;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private RegisterStep _currentStep = RegisterStep.Phone;

    // ── Computed properties ──────────────────────────────────────────────────
    public bool IsPhoneStep    => CurrentStep == RegisterStep.Phone;
    public bool IsPasswordStep => CurrentStep == RegisterStep.Password;

    public RegisterViewModel(
        ICheckPhoneExistUseCase   checkPhoneExistUseCase,
        ISignUpWithPhoneUseCase   signUpUseCase,
        INavigationService        navigationService,
        IAuthTokenStorage         tokenStorage,
        ILogger<RegisterViewModel> logger)
    {
        _checkPhoneExistUseCase = checkPhoneExistUseCase;
        _signUpUseCase          = signUpUseCase;
        _navigationService      = navigationService;
        _tokenStorage           = tokenStorage;
        _logger                 = logger;
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
        if (!string.IsNullOrEmpty(value) && value.Length < 8)
            PasswordError = "Password must be at least 8 characters.";
        ValidatePasswordMatch();
        RegisterCommand.NotifyCanExecuteChanged();
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        ValidatePasswordMatch();
        RegisterCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentStepChanged(RegisterStep value)
    {
        OnPropertyChanged(nameof(IsPhoneStep));
        OnPropertyChanged(nameof(IsPasswordStep));
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task ContinueAsync(CancellationToken cancellationToken)
    {
        IsLoading    = true;
        PhoneError   = null;
        ErrorMessage = null;

        try
        {
            var exists = await _checkPhoneExistUseCase.ExecuteAsync(PhoneNumber, cancellationToken);

            if (exists)
            {
                PhoneError = "This phone number is already registered. Sign in instead.";
                return;
            }

            CurrentStep = RegisterStep.Password;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Phone check cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check phone existence.");
            ErrorMessage = "Network error. Please check your connection and try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanContinue()
        => !string.IsNullOrWhiteSpace(PhoneNumber)
           && PhoneRegex.IsMatch(PhoneNumber)
           && !IsLoading;

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        IsLoading    = true;
        ErrorMessage = null;

        try
        {
            var input = new RegisterInput(PhoneNumber, Password, DisplayName);
            var user  = await _signUpUseCase.ExecuteAsync(input, cancellationToken);

            if (!string.IsNullOrEmpty(user.AccessToken))
                _tokenStorage.SaveToken(user.AccessToken);

            _navigationService.NavigateTo(NavigationRoutes.Main);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for field {Field}: {Message}", ex.Field, ex.Message);
            if (ex.Field == nameof(RegisterInput.Password))
                PasswordError = ex.Message;
            else
                ErrorMessage = ex.Message;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Register request cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed.");
            ErrorMessage = "Registration failed. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanRegister()
        => !string.IsNullOrWhiteSpace(Password)
           && Password.Length >= 8
           && Password == ConfirmPassword
           && !IsLoading;

    [RelayCommand]
    private void NavigateToLogin()
        => _navigationService.NavigateTo(NavigationRoutes.Login);

    [RelayCommand]
    private void GoBack()
    {
        if (CurrentStep == RegisterStep.Password)
        {
            CurrentStep           = RegisterStep.Phone;
            Password              = string.Empty;
            ConfirmPassword       = string.Empty;
            PasswordError         = null;
            ConfirmPasswordError  = null;
            ErrorMessage          = null;
        }
        else
        {
            _navigationService.GoBack();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void ValidatePasswordMatch()
    {
        ConfirmPasswordError = !string.IsNullOrEmpty(ConfirmPassword) && Password != ConfirmPassword
            ? "Passwords do not match."
            : null;
    }
}
