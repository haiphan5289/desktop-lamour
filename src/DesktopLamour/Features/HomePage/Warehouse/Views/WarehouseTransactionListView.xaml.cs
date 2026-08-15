// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class WarehouseTransactionListView : System.Windows.Controls.UserControl
{
    public WarehouseTransactionListView(WarehouseTransactionListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is WarehouseTransactionListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
