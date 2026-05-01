// HomeViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Views;

namespace DesktopLamour.Features.HomePage.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService      _navigationService;
    private readonly Func<SalesOrderWindow>  _salesWindowFactory;

    public HomeViewModel(INavigationService navigationService, Func<SalesOrderWindow> salesWindowFactory)
    {
        _navigationService  = navigationService;
        _salesWindowFactory = salesWindowFactory;
    }

    [RelayCommand]
    private void NavigateToProducts()
        => _navigationService.NavigateTo(NavigationRoutes.Products.List);

    [RelayCommand]
    private void NavigateToSuppliers()
        => _navigationService.NavigateTo(NavigationRoutes.Suppliers.List);

    [RelayCommand]
    private void NavigateToCustomers()
        => _navigationService.NavigateTo(NavigationRoutes.Customers.List);

    [RelayCommand]
    private void NavigateToEmployees()
        => _navigationService.NavigateTo(NavigationRoutes.Employees.List);

    [RelayCommand]
    private void NavigateToWarehouse()
        => _navigationService.NavigateTo(NavigationRoutes.Warehouse.Hub);

    [RelayCommand]
    private void NavigateToAccounting()
        => _navigationService.NavigateTo(NavigationRoutes.Accounting.Hub);

    [RelayCommand]
    private void NavigateToSales()
        => _salesWindowFactory().Show();
}
