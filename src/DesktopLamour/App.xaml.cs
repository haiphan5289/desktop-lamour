// App.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.Authentication;
using DesktopLamour.Features.HomePage;
using DesktopLamour.Features.Realtime;
using DesktopLamour.MainWindow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DesktopLamour;

public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var serverUrl = config["ServerUrl"] ?? "http://localhost:5282";

        var services = new ServiceCollection();
        ConfigureServices(services, serverUrl);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow.MainWindow>();
        mainWindow.Show();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var msg = $"[{DateTime.Now:HH:mm:ss}] DISPATCHER EXCEPTION\n{e.Exception}\n\n";
        File.AppendAllText(@"C:\crash_log.txt", msg);
        MessageBox.Show($"Lỗi: {e.Exception.Message}\n\nXem chi tiết tại C:\\crash_log.txt",
                        "Lỗi ứng dụng", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var msg = $"[{DateTime.Now:HH:mm:ss}] UNOBSERVED TASK EXCEPTION\n{e.Exception}\n\n";
        File.AppendAllText(@"C:\crash_log.txt", msg);
        e.SetObserved();
    }

    private static void ConfigureServices(IServiceCollection services, string serverUrl)
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
        services.AddAuthenticationModule(serverUrl);
        services.AddHomeModule(serverUrl);
        services.AddRealtimeModule(serverUrl);
    }
}
