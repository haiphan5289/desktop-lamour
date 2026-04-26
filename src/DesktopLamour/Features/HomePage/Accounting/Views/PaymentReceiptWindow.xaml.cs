// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class PaymentReceiptWindow : Window
{
    public PaymentReceiptViewModel ViewModel { get; }

    public PaymentReceiptWindow(PaymentReceiptViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadAsync();
    }
}
