// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderWindow : Window
{
    public SalesOrderViewModel ViewModel { get; }

    public SalesOrderWindow(SalesOrderViewModel viewModel)
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
