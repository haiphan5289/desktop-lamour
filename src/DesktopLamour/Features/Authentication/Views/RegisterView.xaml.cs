// RegisterView.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.ViewModels;

namespace DesktopLamour.Features.Authentication.Views;

public partial class RegisterView : System.Windows.Controls.UserControl
{
    public RegisterView(RegisterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
