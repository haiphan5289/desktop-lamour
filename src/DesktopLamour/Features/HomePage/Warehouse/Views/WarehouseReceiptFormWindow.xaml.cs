// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class WarehouseReceiptFormWindow : Window
{
    public WarehouseReceiptFormViewModel ViewModel { get; }

    public WarehouseReceiptFormWindow(WarehouseReceiptFormViewModel viewModel)
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
