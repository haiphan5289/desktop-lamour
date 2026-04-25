// HomeViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.Home.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public HomeViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
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
}
