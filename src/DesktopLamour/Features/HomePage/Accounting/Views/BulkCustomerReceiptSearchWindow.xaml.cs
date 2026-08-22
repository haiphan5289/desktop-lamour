// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class BulkCustomerReceiptSearchWindow : Window
{
    public BulkCustomerReceiptSearchViewModel ViewModel { get; }

    public BulkCustomerReceiptSearchWindow(BulkCustomerReceiptSearchViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        ViewModel.RequestClose += Close;
        Loaded += async (_, _) => await ViewModel.InitializeCommand.ExecuteAsync(null);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
