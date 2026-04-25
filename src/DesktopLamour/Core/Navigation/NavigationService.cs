// NavigationService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

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
        // Parameter-aware navigation can be extended here
        NavigateTo(viewName);
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

    private object? ResolveView(string viewName)
    {
        // Extend this switch as new views are added
        return viewName switch
        {
            NavigationRoutes.Home.Dashboard  => _serviceProvider.GetService(typeof(Features.HomePage.Home.Views.HomeView)),
            NavigationRoutes.Products.List   => _serviceProvider.GetService(typeof(Features.HomePage.ProductList.Views.ProductListView)),
            NavigationRoutes.Suppliers.List  => _serviceProvider.GetService(typeof(Features.HomePage.Suppliers.Views.SupplierListView)),
            NavigationRoutes.Customers.List  => _serviceProvider.GetService(typeof(Features.HomePage.Customers.Views.CustomerListView)),
            NavigationRoutes.Employees.List  => _serviceProvider.GetService(typeof(Features.HomePage.Employees.Views.EmployeeListView)),
            NavigationRoutes.Register        => _serviceProvider.GetService(typeof(Features.Authentication.Views.RegisterView)),
            _ => null
        };
    }
}
