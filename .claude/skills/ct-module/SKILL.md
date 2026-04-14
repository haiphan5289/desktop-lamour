---
name: ct-module
description: Generate a complete MVVM module structure with View (UserControl), ViewModel, and DI registration. Use when creating a new feature module from scratch. Generates all files with interface definitions, CommunityToolkit.Mvvm patterns, AppDesignSystem XAML, Microsoft.Extensions.DependencyInjection setup, and TODO guidance.
---

# WPF Basic Module Generator

Generate complete MVVM module with barebone structure following production patterns.

## Input Format

```
MODULE_NAME: <ModuleName, e.g. "UserProfile">
FEATURE: <Feature folder, e.g. "Features/UserManagement">
```

## Output Files

1. `[ModuleName]View.xaml` + `[ModuleName]View.xaml.cs` — UI layer with AppDesignSystem
2. `[ModuleName]ViewModel.cs` — Business logic with UseCase dependencies + all interfaces
3. `ServiceCollectionExtensions.cs` — Dependency injection registration

## View.xaml

```xml
<!-- [ModuleName]View.xaml -->
<UserControl x:Class="App.[Feature].Views.[ModuleName]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:App.[Feature].ViewModels"
             Loaded="OnLoaded">

    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/App;component/Shared/AppDesignSystem.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>

    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Loading overlay -->
        <ProgressBar Grid.RowSpan="3"
                     IsIndeterminate="True"
                     Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
                     VerticalAlignment="Top"/>

        <!-- Error state -->
        <local:AppLabel Grid.Row="0"
                        Text="{Binding ErrorMessage}"
                        Style="{StaticResource AppTypography.BodyCaption}"
                        Foreground="{StaticResource AppColor.TextError}"
                        Visibility="{Binding ErrorMessage, Converter={StaticResource NullToCollapsedConverter}}"/>

        <!-- TODO: Main content -->
        <!-- <local:AppLabel Grid.Row="1"
                            Text="{Binding Title}"
                            Style="{StaticResource AppTypography.HeaderSection}"/> -->

        <!-- TODO: Action button -->
        <!-- <local:AppButton Grid.Row="2"
                             Content="Load"
                             Style="{StaticResource AppButton.Primary.Medium}"
                             Command="{Binding LoadCommand}"/> -->
    </Grid>
</UserControl>
```

## View.xaml.cs

```csharp
// [ModuleName]View.xaml.cs
using System.Windows;
using System.Windows.Controls;
using App.[Feature].ViewModels;

namespace App.[Feature].Views;

public partial class [ModuleName]View : UserControl
{
    public [ModuleName]View()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is [ModuleName]ViewModel vm)
        {
            // vm.InitializeCommand.Execute(null);
        }
    }
}
```

## ViewModel.cs (includes all interfaces)

```csharp
// [ModuleName]ViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace App.[Feature].ViewModels;

// Interfaces
public interface I[ModuleName]ViewModel
{
    bool IsLoading { get; }
    string? ErrorMessage { get; }
    // ObservableCollection<SomeItemViewModel> Items { get; }
}

public interface I[ModuleName]NavigationService
{
    // void NavigateToDetail(string id);
}

// ViewModel implementation
public sealed partial class [ModuleName]ViewModel : ViewModelBase, I[ModuleName]ViewModel
{
    // #region Dependencies

    private readonly ILogger<[ModuleName]ViewModel> _logger;
    // private readonly I[Name]UseCase _[name]UseCase;
    // private readonly I[ModuleName]NavigationService _navigationService;

    // #region Properties

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    // [ObservableProperty]
    // private ObservableCollection<SomeItemViewModel> _items = new();

    // [ObservableProperty]
    // private string _title = string.Empty;

    // #region Initialization

    public [ModuleName]ViewModel(
        ILogger<[ModuleName]ViewModel> logger
        // I[Name]UseCase [name]UseCase,
        // I[ModuleName]NavigationService navigationService
    )
    {
        _logger = logger;
        // _[name]UseCase = [name]UseCase;
        // _navigationService = navigationService;
    }

    // #region Commands

    // [RelayCommand]
    // private async Task LoadAsync(CancellationToken cancellationToken)
    // {
    //     IsLoading = true;
    //     ErrorMessage = null;
    //     try
    //     {
    //         var result = await _[name]UseCase.ExecuteAsync(cancellationToken);
    //         Items = new ObservableCollection<SomeItemViewModel>(result.Select(SomeItemViewModel.From));
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to load [ModuleName]");
    //         ErrorMessage = "Failed to load data. Please try again.";
    //     }
    //     finally { IsLoading = false; }
    // }
}
```

## ServiceCollectionExtensions.cs

```csharp
// ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using App.[Feature].ViewModels;
using App.[Feature].Views;
// using App.[Feature].Domain.UseCases;
// using App.[Feature].Data.Repositories;
// using App.[Feature].Data.Services;

namespace App.[Feature];

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add[ModuleName]Module(this IServiceCollection services)
    {
        // ViewModels
        services.AddTransient<[ModuleName]ViewModel>();

        // UseCases
        // services.AddTransient<I[Name]UseCase, [Name]UseCase>();

        // Repositories
        // services.AddScoped<I[Name]Repository, [Name]Repository>();

        // Services (HttpClient)
        // services.AddHttpClient<I[Name]Service, [Name]Service>(client =>
        // {
        //     client.BaseAddress = new Uri("https://api.example.com");
        // });

        return services;
    }
}
```

## File Structure

```
Features/[ModuleName]/
├── Views/
│   ├── [ModuleName]View.xaml
│   └── [ModuleName]View.xaml.cs
├── ViewModels/
│   └── [ModuleName]ViewModel.cs
├── Domain/
│   ├── UseCases/
│   │   └── [Name]UseCase.cs
│   └── Models/
│       └── [Name]Model.cs
├── Data/
│   ├── Repositories/
│   │   ├── I[Name]Repository.cs
│   │   └── [Name]Repository.cs
│   └── Services/
│       ├── I[Name]Service.cs
│       └── [Name]Service.cs
└── ServiceCollectionExtensions.cs
```

## Rules

- All 3 files must be created together
- `[ModuleName]View.xaml` + code-behind pair hosts XAML UI
- `[ModuleName]ViewModel.cs` implements `ObservableObject` + exposes `[RelayCommand]` async methods
- `ServiceCollectionExtensions.cs` registers all types in the DI container
- Use `ILogger<T>` for logging — never `Console.WriteLine`
- `configureViewModel()` calls `viewModel?.didBecomeActive()`
- Use XAML layout for all constraints, never XAML code-behind layout
- Only use AppLabel, AppButton — never UILabel, UIButton directly
