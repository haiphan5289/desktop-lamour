// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class PaymentWindow : Window
{
    public PaymentViewModel ViewModel { get; }

    public PaymentWindow(PaymentViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += Close;
        PreviewKeyDown += PaymentWindow_PreviewKeyDown;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadAsync();
        ViewModel.AddNewCommand.Execute(null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();

    // F3 - Tìm nhanh: đưa focus tới ô "Đối tượng" để người dùng gõ tìm ngay.
    private void PaymentWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F3) return;
        PartnerCombo.Focus();
        e.Handled = true;
    }
}
