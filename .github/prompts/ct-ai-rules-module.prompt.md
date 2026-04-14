---
description: "Generate a complete C#/.NET WPF MVVM module with View (UserControl), ViewModel, and DI registration"
mode: "agent"
---

# WPF Complete Module Generator

Generate a complete MVVM module following Clean Architecture patterns.

## Instructions

Reference our C#/.NET WPF development guidelines:

-   **Primary**: [WPF Guidelines](../instructions/wpf-general-instructions.instructions.md)
-   **Fallback**: [AI Agent Context](../../AGENTS.md) (if primary unavailable)

Generate all module files with:

-   XAML UserControl + code-behind pair
-   ViewModel with CommunityToolkit.Mvvm patterns
-   DI registration in `ServiceCollectionExtensions.cs`
-   Interface definitions per layer
-   TODO comments for implementation

## Generated Structure

```
Features/[ModuleName]/
├── Views/
│   ├── [ModuleName]View.xaml
│   └── [ModuleName]View.xaml.cs
├── ViewModels/
│   └── [ModuleName]ViewModel.cs
├── Domain/
│   ├── UseCases/
│   │   ├── I[ModuleName]UseCase.cs
│   │   └── [ModuleName]UseCase.cs
│   └── Models/
│       └── [ModuleName]Dto.cs
├── Data/
│   ├── Repositories/
│   │   ├── I[ModuleName]Repository.cs
│   │   └── [ModuleName]Repository.cs
│   └── Services/
│       ├── I[ModuleName]Service.cs
│       └── [ModuleName]Service.cs
└── ServiceCollectionExtensions.cs
```

## View.xaml

```xml
<!-- [ModuleName]View.xaml -->
<UserControl x:Class="App.Features.[ModuleName].Views.[ModuleName]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:App.Features.[ModuleName].ViewModels"
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
                     VerticalAlignment="Top"
                     Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>

        <!-- Error state -->
        <local:AppLabel Grid.Row="0"
                        Text="{Binding ErrorMessage}"
                        Style="{StaticResource AppTypography.BodyCaption}"
                        Foreground="{StaticResource AppColor.TextError}"
                        Visibility="{Binding ErrorMessage, Converter={StaticResource NullToCollapsedConverter}}"/>

        <!-- TODO: Main content -->
        <!-- <local:AppLabel Grid.Row="1"
                            Text="Content here"
                            Style="{StaticResource AppTypography.BodySection}"/> -->

        <!-- TODO: Action button -->
        <!-- <local:AppButton Grid.Row="2"
                             Content="Load Data"
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
using App.Features.[ModuleName].ViewModels;

namespace App.Features.[ModuleName].Views;

public partial class [ModuleName]View : UserControl
{
    public [ModuleName]View()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // TODO: Trigger initialization
        // (DataContext as [ModuleName]ViewModel)?.InitializeCommand.Execute(null);
    }
}
```

## ViewModel.cs

```csharp
// [ModuleName]ViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using App.Features.[ModuleName].Domain.UseCases;
using App.Features.[ModuleName].Domain.Models;

namespace App.Features.[ModuleName].ViewModels;

public partial class [ModuleName]ViewModel : ObservableObject
{
    // #region Fields
    private readonly I[ModuleName]UseCase _useCase;
    private readonly ILogger<[ModuleName]ViewModel> _logger;
    // #endregion

    // #region Observable Properties
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<[ModuleName]Dto> _items = new();

    // TODO: Add more [ObservableProperty] fields as needed
    // [ObservableProperty]
    // private string _title = string.Empty;
    // #endregion

    public [ModuleName]ViewModel(I[ModuleName]UseCase useCase, ILogger<[ModuleName]ViewModel> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    // #region Commands
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _useCase.ExecuteAsync(ct);
            Items = new ObservableCollection<[ModuleName]Dto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load [ModuleName]");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // TODO: Add more [RelayCommand] methods
    // [RelayCommand]
    // private async Task SubmitAsync(CancellationToken ct) { }
    // #endregion
}
```

## UseCase.cs

```csharp
// I[ModuleName]UseCase.cs
namespace App.Features.[ModuleName].Domain.UseCases;

public interface I[ModuleName]UseCase
{
    Task<List<[ModuleName]Dto>> ExecuteAsync(CancellationToken ct = default);
}

// [ModuleName]UseCase.cs
public sealed class [ModuleName]UseCase : I[ModuleName]UseCase
{
    private readonly I[ModuleName]Repository _repository;

    public [ModuleName]UseCase(I[ModuleName]Repository repository)
    {
        _repository = repository;
    }

    public async Task<List<[ModuleName]Dto>> ExecuteAsync(CancellationToken ct = default)
    {
        // TODO: Implement business logic
        return await _repository.GetListAsync(ct);
    }
}
```

## Repository.cs

```csharp
// I[ModuleName]Repository.cs
namespace App.Features.[ModuleName].Data.Repositories;

public interface I[ModuleName]Repository
{
    Task<List<[ModuleName]Dto>> GetListAsync(CancellationToken ct = default);
}

// [ModuleName]Repository.cs
public sealed class [ModuleName]Repository : I[ModuleName]Repository
{
    private readonly I[ModuleName]Service _service;

    public [ModuleName]Repository(I[ModuleName]Service service)
    {
        _service = service;
    }

    public async Task<List<[ModuleName]Dto>> GetListAsync(CancellationToken ct = default)
        => await _service.FetchListAsync(ct) ?? new();
}
```

## Service.cs

```csharp
// I[ModuleName]Service.cs
namespace App.Features.[ModuleName].Data.Services;

public interface I[ModuleName]Service
{
    Task<List<[ModuleName]Dto>?> FetchListAsync(CancellationToken ct = default);
}

// [ModuleName]Service.cs
public sealed class [ModuleName]Service : I[ModuleName]Service
{
    private readonly HttpClient _http;

    public [ModuleName]Service(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<[ModuleName]Dto>?> FetchListAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<[ModuleName]Dto>>("/api/[module-endpoint]", ct);
}
```

## DI Registration

```csharp
// ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using App.Features.[ModuleName].ViewModels;
using App.Features.[ModuleName].Domain.UseCases;
using App.Features.[ModuleName].Data.Repositories;
using App.Features.[ModuleName].Data.Services;

namespace App.Features.[ModuleName];

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add[ModuleName](this IServiceCollection services)
    {
        services.AddTransient<[ModuleName]ViewModel>();
        services.AddTransient<I[ModuleName]UseCase, [ModuleName]UseCase>();
        services.AddTransient<I[ModuleName]Repository, [ModuleName]Repository>();
        services.AddHttpClient<I[ModuleName]Service, [ModuleName]Service>(client =>
        {
            // TODO: Configure base address if different from global
        });
        return services;
    }
}
```

## File Types to Generate

### View (UserControl)
- XAML + code-behind pair using AppDesignSystem only
- Loaded event to trigger initialization command

### ViewModel
- `partial class` inheriting `ObservableObject`
- `[ObservableProperty]` for state, `[RelayCommand]` for async actions
- Constructor injection: UseCase + `ILogger<T>`

### UseCase
- Interface + sealed implementation
- Orchestrates repository, contains business logic

### Repository
- Interface + sealed implementation
- Delegates to Service, null-safe returns

### Service
- Interface + sealed implementation
- Typed `HttpClient` with `System.Net.Http.Json`

### DI (ServiceCollectionExtensions)
- Extension method `Add[ModuleName]` on `IServiceCollection`
- Register all types as `Transient`

## Rules

- All 3 core files (View, ViewModel, DI) must be created together
- `ViewModel` is `partial class` for source generator support
- Use `ILogger<T>` for logging — never `Console.WriteLine`
- All async methods accept `CancellationToken`
- Never use raw WPF components — only `AppLabel`, `AppButton`, etc.
- XAML layout only — no programmatic layout in code-behind
