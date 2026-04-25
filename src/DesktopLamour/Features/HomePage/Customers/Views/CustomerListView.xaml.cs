// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Customers.Views;

public partial class CustomerListView : UserControl
{
    public CustomerListView(CustomerListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadCustomersCommand.ExecuteAsync(null);
    }
}
