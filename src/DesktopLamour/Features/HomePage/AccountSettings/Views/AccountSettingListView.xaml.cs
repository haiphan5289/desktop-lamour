// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.AccountSettings.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.AccountSettings.Views;

public partial class AccountSettingListView : UserControl
{
    public AccountSettingListView(AccountSettingListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAccountsCommand.ExecuteAsync(null);
    }
}
