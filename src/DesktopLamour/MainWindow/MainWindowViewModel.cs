// MainWindowViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.Realtime;
using System.Windows;

namespace DesktopLamour.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IAuthTokenStorage    _tokenStorage;
    private readonly INavigationService   _navigationService;
    private readonly IPostLoginSyncService _postLoginSync;

    [ObservableProperty] private object? _currentContent;
    [ObservableProperty] private bool    _isLoggedIn;

    public MainWindowViewModel(IAuthTokenStorage tokenStorage, INavigationService navigationService, IPostLoginSyncService postLoginSync)
    {
        _tokenStorage      = tokenStorage;
        _navigationService = navigationService;
        _postLoginSync     = postLoginSync;
    }

    partial void OnCurrentContentChanged(object? value)
        => IsLoggedIn = _tokenStorage.HasToken;

    [RelayCommand]
    private void Logout()
    {
        var r = MessageBox.Show(
            "Bạn có chắc muốn đăng xuất?",
            "Xác nhận đăng xuất",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;

        _tokenStorage.Clear();
        _ = _postLoginSync.ShutdownAsync(); // stop realtime + clear cached Customer/Employee lists
        _navigationService.NavigateToLogin();
    }
}
