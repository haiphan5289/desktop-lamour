// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class BulkCustomerReceiptWindow : Window
{
    public BulkCustomerReceiptViewModel ViewModel { get; }

    public BulkCustomerReceiptWindow(BulkCustomerReceiptViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        ViewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };
    }

    public void Initialize(
        IReadOnlyList<OutstandingSalesOrderCheckItem> selected,
        string debitAccount, string? bankAccount, int? collectorEmployeeId)
        => ViewModel.Initialize(selected, debitAccount, bankAccount, collectorEmployeeId);

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
