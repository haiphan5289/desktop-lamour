// LoginView.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.ViewModels;

namespace DesktopLamour.Features.Authentication.Views;

public partial class LoginView : System.Windows.Controls.UserControl
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
