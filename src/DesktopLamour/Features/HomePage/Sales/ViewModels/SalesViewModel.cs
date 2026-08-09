// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Deposits.Views;
using DesktopLamour.Features.HomePage.Sales.Views;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly Func<SalesOrderReportFilterWindow> _reportFilterWindowFactory;
    private readonly Func<DepositWindow> _depositWindowFactory;

    public SalesViewModel(
        INavigationService navigationService,
        Func<SalesOrderReportFilterWindow> reportFilterWindowFactory,
        Func<DepositWindow> depositWindowFactory)
    {
        _navigationService         = navigationService;
        _reportFilterWindowFactory = reportFilterWindowFactory;
        _depositWindowFactory      = depositWindowFactory;
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
    private void OpenReport()
    {
        var window = _reportFilterWindowFactory();
        if (window.ShowDialog() == true)
        {
            var filter = window.BuildFilter();
            _navigationService.NavigateTo(NavigationRoutes.SalesOrders.Report, filter);
        }
    }

    [RelayCommand]
    private void OpenDeposit()
    {
        var window = _depositWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.ShowDialog();
    }

    [RelayCommand]
    private void NavigateToDepositDeductionReport()
        => _navigationService.NavigateTo(NavigationRoutes.Deposits.DeductionReport);
}
