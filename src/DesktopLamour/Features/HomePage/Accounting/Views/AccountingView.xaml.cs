// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class AccountingView : System.Windows.Controls.UserControl
{
    public AccountingView(AccountingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AccountingViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
