// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Backups.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Backups.Views;

public partial class RestoreConfirmWindow : Window
{
    public RestoreConfirmViewModel ViewModel { get; }

    public RestoreConfirmWindow(RestoreConfirmViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(string fileName) => ViewModel.Initialize(fileName);

    public string Password => ViewModel.Password;
}
