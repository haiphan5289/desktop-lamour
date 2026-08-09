// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Deposits.Views;

public partial class DepositWindow : Window
{
    public DepositViewModel ViewModel { get; }

    public DepositWindow(DepositViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += Close;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadAsync();
        if (ViewModel.CurrentDeposit is null)
            await ViewModel.AddNewCommand.ExecuteAsync(null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
