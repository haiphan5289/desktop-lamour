// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class ReceiptWindow : Window
{
    public ReceiptViewModel ViewModel { get; }

    public ReceiptWindow(ReceiptViewModel viewModel)
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
        ViewModel.AddNewCommand.Execute(null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
