// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
using DesktopLamour.Features.HomePage.AccountSettings.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace DesktopLamour.Features.HomePage.AccountSettings.Views;

public partial class AccountSettingFormWindow : Window
{
    public AccountSettingFormViewModel ViewModel { get; }

    public AccountSettingFormWindow(AccountSettingFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(AccountSetting? account = null) => ViewModel.Initialize(account);

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (ViewModel.IsDirty && DialogResult is null)
        {
            var r = MessageBox.Show(
                "Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) e.Cancel = true;
        }
    }
}
