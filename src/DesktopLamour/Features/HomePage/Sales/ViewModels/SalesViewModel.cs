// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public SalesViewModel(INavigationService navigationService)
        => _navigationService = navigationService;

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private void NavigateToSalesOrders()
        => _navigationService.NavigateTo(NavigationRoutes.SalesOrders.List);

    [RelayCommand]
    private void NavigateToSalesReturns()
        => _navigationService.NavigateTo(NavigationRoutes.SalesReturns.List);
}
