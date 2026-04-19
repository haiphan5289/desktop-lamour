// App.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.Authentication;
using DesktopLamour.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour;

public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow.MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder => builder.AddConsole());

        // Core
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAuthTokenStorage, InMemoryAuthTokenStorage>();

        // Windows
        services.AddTransient<MainWindow.MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        // Feature modules
        services.AddAuthenticationModule();
    }
}
