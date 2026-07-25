// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Views;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly Func<SalesOrderReportFilterWindow> _reportFilterWindowFactory;

    public SalesViewModel(
        INavigationService navigationService,
        Func<SalesOrderReportFilterWindow> reportFilterWindowFactory)
    {
        _navigationService         = navigationService;
        _reportFilterWindowFactory = reportFilterWindowFactory;
    }

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

    [RelayCommand]
    private void NavigateToCategories()
        => _navigationService.NavigateTo(NavigationRoutes.Categories.List);

    [RelayCommand]
    private void OpenReport()
    {
        var window = _reportFilterWindowFactory();
        if (window.ShowDialog() == true)
        {
            var filter = window.BuildFilter();
            _navigationService.NavigateTo(NavigationRoutes.SalesOrders.Report, filter);
        }
    }
}
