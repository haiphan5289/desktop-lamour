// MainWindow.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Navigation;

namespace DesktopLamour.MainWindow;

public partial class MainWindow : System.Windows.Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly INavigationService _navigationService;

    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _navigationService = navigationService;

        DataContext = _viewModel;

        // Wire the navigation service to the window view model
        ((NavigationService)_navigationService).Initialize(_viewModel);

        // Boot directly into Home
        _navigationService.NavigateTo(NavigationRoutes.Home);
    }
}
