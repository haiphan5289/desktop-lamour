// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
using DesktopLamour.Features.HomePage.AccountSettings.Views;

namespace DesktopLamour.Features.HomePage.AccountSettings.ViewModels;

public partial class AccountSettingListViewModel : ViewModelBase
{
    private readonly INavigationService             _navigationService;
    private readonly IGetAccountSettingsUseCase     _getAccounts;
    private readonly IDeleteAccountSettingUseCase   _deleteAccount;
    private readonly Func<AccountSettingFormWindow> _formWindowFactory;

    [ObservableProperty] private bool            _isLoading;
    [ObservableProperty] private bool            _hasError;
    [ObservableProperty] private string          _errorMessage = string.Empty;
    [ObservableProperty] private bool            _hasAccounts;
    [ObservableProperty] private AccountSetting? _selectedAccount;

    public ObservableCollection<AccountSetting> Accounts { get; } = new();

    private bool HasSelection => SelectedAccount is not null;

    public AccountSettingListViewModel(
        INavigationService             navigationService,
        IGetAccountSettingsUseCase     getAccounts,
        IDeleteAccountSettingUseCase   deleteAccount,
        Func<AccountSettingFormWindow> formWindowFactory)
    {
        _navigationService = navigationService;
        _getAccounts        = getAccounts;
        _deleteAccount       = deleteAccount;
        _formWindowFactory   = formWindowFactory;
    }

    partial void OnSelectedAccountChanged(AccountSetting? value)
    {
        EditAccountCommand.NotifyCanExecuteChanged();
        DeleteAccountCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadAccountsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getAccounts.ExecuteAsync(ct);
            Accounts.Clear();
            foreach (var a in items) Accounts.Add(a);
            HasAccounts = Accounts.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AddAccountAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadAccountsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditAccountAsync(CancellationToken ct = default)
    {
        if (SelectedAccount is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedAccount);
        if (window.ShowDialog() == true)
            await LoadAccountsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteAccountAsync(CancellationToken ct = default)
    {
        if (SelectedAccount is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa tài khoản '{SelectedAccount.Code} — {SelectedAccount.Description}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteAccount.ExecuteAsync(SelectedAccount.Id, ct);
            Accounts.Remove(SelectedAccount);
            SelectedAccount = null;
            HasAccounts       = Accounts.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
