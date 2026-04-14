---
name: ct-scaffold
description: Scaffold basic barebone C#/.NET WPF files following MVVM + Clean Architecture. Use when creating a View (UserControl), ViewModel, UseCase, Repository, Service, Model, or DataTemplate from scratch. Generates proper region sections, using statements, interface structure, CommunityToolkit.Mvvm patterns, AppDesignSystem XAML, and TODO comments. Supports: View, ViewModel, UseCase, Repository, Service, Model, DataTemplate.
---

# WPF Basic File Scaffolding

Create basic barebone C#/.NET WPF files following MVVM + Clean Architecture and coding conventions.

## Input Format

```
FILE_TYPE: <View | ViewModel | UseCase | Repository | Service | Model | DataTemplate>
NAME: <BaseName, e.g. "UserProfile">
MODULE: <Module name, e.g. "Features/UserManagement">
```

## Required Usings

```csharp
// ViewModel
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

// Repository / Service
using System.Net.Http.Json;
using System.Text.Json.Serialization;
```

## View Template (UserControl)

```xml
<!-- [Name]View.xaml -->
<UserControl x:Class="App.[Module].Views.[Name]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:App.[Module].Views"
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
        </Grid.RowDefinitions>

        <!-- Loading indicator -->
        <ProgressBar Grid.RowSpan="2"
                     IsIndeterminate="True"
                     VerticalAlignment="Top"
                     Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>

        <!-- TODO: Add AppLabel, AppButton, ListView etc. -->
        <!-- <local:AppLabel Grid.Row="0"
                            Text="{Binding Title}"
                            Style="{StaticResource AppTypography.HeaderSection}"/> -->
    </Grid>
</UserControl>
```

```csharp
// [Name]View.xaml.cs
using System.Windows;
using System.Windows.Controls;

namespace App.[Module].Views;

public partial class [Name]View : UserControl
{
    public [Name]View()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // (DataContext as [Name]ViewModel)?.LoadCommand.Execute(null);
    }
}
```

## ViewModel Template

```csharp
// [Name]ViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace App.[Module].ViewModels;

// Interfaces
public interface I[Name]ViewModel
{
    bool IsLoading { get; }
    string? ErrorMessage { get; }
}

public interface I[Name]NavigationService
{
    // void NavigateToDetail(string id);
}

// ViewModel
public sealed partial class [Name]ViewModel : ViewModelBase, I[Name]ViewModel
{
    // #region Dependencies

    private readonly ILogger<[Name]ViewModel> _logger;
    // private readonly I[Name]UseCase _[name]UseCase;

    // #region Properties

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    // [ObservableProperty]
    // private ObservableCollection<[Name]ItemViewModel> _items = new();

    // [ObservableProperty]
    // private string _title = string.Empty;

    // #region Initialization

    public [Name]ViewModel(
        ILogger<[Name]ViewModel> logger
        // I[Name]UseCase [name]UseCase
    )
    {
        _logger = logger;
        // _[name]UseCase = [name]UseCase;
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
    //         Items = new ObservableCollection<[Name]ItemViewModel>(result.Select([Name]ItemViewModel.From));
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to load [Name]");
    //         ErrorMessage = "Failed to load data. Please try again.";
    //     }
    //     finally { IsLoading = false; }
    // }
}
```

## UseCase Template

```csharp
// Domain/UseCases/I[Name]UseCase.cs
namespace App.[Module].Domain.UseCases;

public interface I[Name]UseCase
{
    Task<IReadOnlyList<[OutputType]>> ExecuteAsync(CancellationToken cancellationToken = default);
    // Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken cancellationToken = default);
}

// Domain/UseCases/[Name]UseCase.cs
using App.[Module].Domain.Repositories;

namespace App.[Module].Domain.UseCases;

public sealed class [Name]UseCase : I[Name]UseCase
{
    private readonly I[Name]Repository _repository;

    public [Name]UseCase(I[Name]Repository repository)
        => _repository = repository;

    public async Task<IReadOnlyList<[OutputType]>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: add business logic / validation / sorting
        return await _repository.GetAllAsync(cancellationToken);
    }
}
```

## Repository Template

```csharp
// Domain/Repositories/I[Name]Repository.cs
namespace App.[Module].Domain.Repositories;

public interface I[Name]Repository
{
    // Task<[Entity]?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    // Task<IReadOnlyList<[Entity]>> GetAllAsync(CancellationToken cancellationToken = default);
}

// Data/Repositories/[Name]Repository.cs
using App.[Module].Domain.Repositories;
using App.[Module].Data.Services;
using Microsoft.Extensions.Logging;

namespace App.[Module].Data.Repositories;

public sealed class [Name]Repository : I[Name]Repository
{
    private readonly I[Name]Service _service;
    private readonly ILogger<[Name]Repository> _logger;

    public [Name]Repository(I[Name]Service service, ILogger<[Name]Repository> logger)
    {
        _service = service;
        _logger = logger;
    }

    // public async Task<IReadOnlyList<[Entity]>> GetAllAsync(CancellationToken cancellationToken = default)
    //     => (await _service.GetAllAsync(cancellationToken)).Select(MapToEntity).ToList();
    //
    // private static [Entity] MapToEntity([Entity]Dto dto) => new() { Id = dto.Id, Name = dto.Name };
}
```

## Service Template

```csharp
// Data/Services/I[Name]Service.cs
namespace App.[Module].Data.Services;

public interface I[Name]Service
{
    // Task<IReadOnlyList<[Entity]Dto>> GetAllAsync(CancellationToken cancellationToken = default);
}

// Data/Services/[Name]Service.cs
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace App.[Module].Data.Services;

public sealed class [Name]Service : I[Name]Service
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<[Name]Service> _logger;

    public [Name]Service(HttpClient httpClient, ILogger<[Name]Service> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // public async Task<IReadOnlyList<[Entity]Dto>> GetAllAsync(CancellationToken cancellationToken = default)
    // {
    //     var result = await _httpClient.GetFromJsonAsync<List<[Entity]Dto>>("api/[entity]", cancellationToken);
    //     return result ?? [];
    // }
}
```

## Model Template

```csharp
// Domain/Models/[Name]Model.cs (domain entity)
namespace App.[Module].Domain.Models;

public sealed record [Name]Model
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    // public string? Description { get; init; }
    // public DateTimeOffset CreatedAt { get; init; }
}
```

```csharp
// Data/Dtos/[Name]Dto.cs (API response DTO)
using System.Text.Json.Serialization;

namespace App.[Module].Data.Dtos;

public sealed record [Name]Dto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
```

## DataTemplate (List Item)

```xml
<!-- [Name]DataTemplate.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:App.[Module].Views">

    <DataTemplate x:Key="[Name]DataTemplate">
        <Border Padding="12,8"
                BorderThickness="0,0,0,1"
                BorderBrush="{StaticResource AppColor.BorderThin}">
            <StackPanel>
                <local:AppLabel Text="{Binding Title}"
                                Style="{StaticResource AppTypography.LabelSection}"/>
                <!-- <local:AppLabel Text="{Binding Subtitle}"
                                    Style="{StaticResource AppTypography.BodyCaption}"/> -->
            </StackPanel>
        </Border>
    </DataTemplate>

</ResourceDictionary>
```

## Rules

- **ALWAYS** use AppDesignSystem (`AppLabel`, `AppButton`, etc.) — never raw WPF `TextBlock`, `Button`
- **ALWAYS** use XAML `Grid`/`StackPanel`/`DockPanel` for layout — no code-behind sizing
- ViewModel class **MUST** be `partial` for `[ObservableProperty]` / `[RelayCommand]` source generators
- All async methods accept `CancellationToken` and are named with `Async` suffix
- Use `ILogger<T>` for all logging — never `Console.WriteLine`
- Implement `IDisposable` and unsubscribe event handlers when using event subscription patterns
