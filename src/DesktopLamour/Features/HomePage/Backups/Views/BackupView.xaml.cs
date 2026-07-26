// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Backups.Views;

public partial class BackupView : UserControl
{
    public BackupView(BackupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) =>
        {
            await viewModel.LoadBackupsCommand.ExecuteAsync(null);
            await viewModel.LoadScheduleCommand.ExecuteAsync(null);
        };
    }
}
