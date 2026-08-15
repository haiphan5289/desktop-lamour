// HomeViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public bool IsAdmin { get; }

    public HomeViewModel(INavigationService navigationService, IAuthTokenStorage tokenStorage)
    {
        _navigationService = navigationService;
        IsAdmin             = tokenStorage.GetRole() == "Admin";
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
        => _navigationService.NavigateTo(NavigationRoutes.Warehouse.NhapXuatKho);

    [RelayCommand]
    private void NavigateToAccounting()
        => _navigationService.NavigateTo(NavigationRoutes.Accounting.Hub);

    [RelayCommand]
    private void NavigateToSales()
        => _navigationService.NavigateTo(NavigationRoutes.Sales.Hub);

    [RelayCommand]
    private void NavigateToBackup()
        => _navigationService.NavigateTo(NavigationRoutes.Backup.List);

    [RelayCommand]
    private void NavigateToProductUnits()
        => _navigationService.NavigateTo(NavigationRoutes.ProductUnits.List);

    [RelayCommand]
    private void NavigateToAccountSettings()
        => _navigationService.NavigateTo(NavigationRoutes.AccountSettings.List);

    [RelayCommand]
    private void NavigateToWarehouses()
        => _navigationService.NavigateTo(NavigationRoutes.Warehouses.List);

    [RelayCommand]
    private void NavigateToDepartments()
        => _navigationService.NavigateTo(NavigationRoutes.Departments.List);

    [RelayCommand]
    private void NavigateToExpenseCategories()
        => _navigationService.NavigateTo(NavigationRoutes.ExpenseCategories.List);
}
