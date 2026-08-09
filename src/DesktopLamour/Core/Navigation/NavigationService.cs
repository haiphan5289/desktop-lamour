// NavigationService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows;
using DesktopLamour.MainWindow;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationService> _logger;
    private MainWindowViewModel? _mainWindowViewModel;
    private readonly Stack<string> _backStack = new();
    private string? _currentView;

    public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public bool CanGoBack => _backStack.Count > 0;

    public void Initialize(MainWindowViewModel mainWindowViewModel)
        => _mainWindowViewModel = mainWindowViewModel;

    public void NavigateTo(string viewName)
    {
        if (_mainWindowViewModel is null)
        {
            _logger.LogWarning("NavigationService not initialized. Call Initialize() first.");
            return;
        }

        if (_currentView is not null)
            _backStack.Push(_currentView);

        _currentView = viewName;
        _logger.LogInformation("Navigating to {ViewName}", viewName);

        var content = ResolveView(viewName);
        _mainWindowViewModel.CurrentContent = content;
    }

    public void NavigateTo(string viewName, object parameter)
    {
        NavigateTo(viewName);

        if (_mainWindowViewModel?.CurrentContent is FrameworkElement { DataContext: INavigationParameterAware aware })
            aware.OnNavigatedTo(parameter);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;
        var previous = _backStack.Pop();
        _currentView = previous;
        var content = ResolveView(previous);
        if (_mainWindowViewModel is not null)
            _mainWindowViewModel.CurrentContent = content;
    }

    public void NavigateToHome()
    {
        _backStack.Clear();
        _currentView = NavigationRoutes.Home.Dashboard;
        _logger.LogInformation("Navigating to Home (history cleared)");
        var content = ResolveView(NavigationRoutes.Home.Dashboard);
        if (_mainWindowViewModel is not null)
            _mainWindowViewModel.CurrentContent = content;
    }

    public void NavigateToLogin()
    {
        _backStack.Clear();
        _currentView = NavigationRoutes.Login;
        _logger.LogInformation("Navigating to Login (history cleared)");
        var content = ResolveView(NavigationRoutes.Login);
        if (_mainWindowViewModel is not null)
            _mainWindowViewModel.CurrentContent = content;
    }

    private object? ResolveView(string viewName)
    {
        // Extend this switch as new views are added
        return viewName switch
        {
            NavigationRoutes.Home.Dashboard  => _serviceProvider.GetService(typeof(Features.HomePage.Home.Views.HomeView)),
            NavigationRoutes.Products.List   => _serviceProvider.GetService(typeof(Features.HomePage.ProductList.Views.ProductListView)),
            NavigationRoutes.Suppliers.List  => _serviceProvider.GetService(typeof(Features.HomePage.Suppliers.Views.SupplierListView)),
            NavigationRoutes.Categories.List => _serviceProvider.GetService(typeof(Features.HomePage.Categories.Views.CategoryListView)),
            NavigationRoutes.ProductUnits.List     => _serviceProvider.GetService(typeof(Features.HomePage.ProductUnits.Views.ProductUnitListView)),
            NavigationRoutes.AccountSettings.List  => _serviceProvider.GetService(typeof(Features.HomePage.AccountSettings.Views.AccountSettingListView)),
            NavigationRoutes.Warehouses.List        => _serviceProvider.GetService(typeof(Features.HomePage.Warehouses.Views.WarehouseSettingListView)),
            NavigationRoutes.Backup.List     => _serviceProvider.GetService(typeof(Features.HomePage.Backups.Views.BackupView)),
            NavigationRoutes.Customers.List  => _serviceProvider.GetService(typeof(Features.HomePage.Customers.Views.CustomerListView)),
            NavigationRoutes.Employees.List  => _serviceProvider.GetService(typeof(Features.HomePage.Employees.Views.EmployeeListView)),
            NavigationRoutes.Warehouse.Hub         => _serviceProvider.GetService(typeof(Features.HomePage.Warehouse.Views.WarehouseView)),
            NavigationRoutes.Warehouse.TongHopTonKho => _serviceProvider.GetService(typeof(Features.HomePage.Warehouse.Views.TongHopTonKhoView)),
            NavigationRoutes.Warehouse.PhieuNhapKho  => _serviceProvider.GetService(typeof(Features.HomePage.Warehouse.Views.WarehouseReceiptListView)),
            NavigationRoutes.Sales.Hub             => _serviceProvider.GetService(typeof(Features.HomePage.Sales.Views.SalesView)),
            NavigationRoutes.SalesOrders.List      => _serviceProvider.GetService(typeof(Features.HomePage.Sales.Views.SalesOrderListView)),
            NavigationRoutes.SalesOrders.Report    => _serviceProvider.GetService(typeof(Features.HomePage.Sales.Views.SalesOrderReportView)),
            NavigationRoutes.SalesOrders.ReportDetail => _serviceProvider.GetService(typeof(Features.HomePage.Sales.Views.SalesOrderReportDetailView)),
            NavigationRoutes.SalesReturns.List     => _serviceProvider.GetService(typeof(Features.HomePage.SalesReturn.Views.SalesReturnListView)),
            NavigationRoutes.Accounting.Hub        => _serviceProvider.GetService(typeof(Features.HomePage.Accounting.Views.AccountingView)),
            NavigationRoutes.Deposits.DeductionReport => _serviceProvider.GetService(typeof(Features.HomePage.Deposits.Views.DepositDeductionReportView)),
            NavigationRoutes.Register        => _serviceProvider.GetService(typeof(Features.Authentication.Views.RegisterView)),
            NavigationRoutes.Login           => _serviceProvider.GetService(typeof(Features.Authentication.Views.LoginView)),
            NavigationRoutes.Main            => _serviceProvider.GetService(typeof(Features.HomePage.Home.Views.HomeView)),
            _ => null
        };
    }
}
