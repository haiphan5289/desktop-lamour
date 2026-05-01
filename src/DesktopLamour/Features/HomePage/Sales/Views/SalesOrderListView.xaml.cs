// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderListView : UserControl
{
    private SalesOrderListViewModel ViewModel => (SalesOrderListViewModel)DataContext;

    public SalesOrderListView(SalesOrderListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    private async void OrdersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.EditSalesOrderCommand.CanExecute(null))
            await ViewModel.EditSalesOrderCommand.ExecuteAsync(null);
    }
}
