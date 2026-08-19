// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class WarehouseTransactionListView : System.Windows.Controls.UserControl
{
    private readonly WarehouseTransactionListViewModel _viewModel;

    public WarehouseTransactionListView(WarehouseTransactionListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is WarehouseTransactionListViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    private void TransactionGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TransactionGrid.SelectedItem is WarehouseTransactionResponseDto item && _viewModel.ShowDetailCommand.CanExecute(item))
            _viewModel.ShowDetailCommand.Execute(item);
    }
}
