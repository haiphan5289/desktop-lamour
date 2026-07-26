// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.Backups.ViewModels;

public partial class RestoreConfirmViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string _password = string.Empty;

    [ObservableProperty] private string _fileName = string.Empty;

    public event Action<bool>? RequestClose;

    public void Initialize(string fileName)
    {
        FileName = fileName;
        Password = string.Empty;
    }

    private bool CanConfirm => !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm() => RequestClose?.Invoke(true);

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);
}
