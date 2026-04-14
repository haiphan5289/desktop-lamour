---
description: "Scaffold basic C#/.NET WPF files following MVVM + Clean Architecture patterns"
mode: "agent"
---

# WPF Basic File Scaffolding

Create basic barebone C#/.NET WPF files following MVVM + Clean Architecture and coding conventions.

## Instructions

Reference our C#/.NET WPF development guidelines:

-   **Primary**: [WPF Guidelines](../instructions/wpf-general-instructions.instructions.md)
-   **Fallback**: [AI Agent Context](../../AGENTS.md) (if primary unavailable)

Generate basic scaffold files with:

-   Proper region sections and using statements
-   MVVM interface structure (IViewModel, IViewModelFactory)
-   AppDesignSystem XAML components
-   CommunityToolkit.Mvvm patterns ([ObservableProperty], [RelayCommand])
-   TODO comments for implementation

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

## View Template (XAML)

```xml
<!-- [Name]View.xaml -->
<UserControl x:Class="App.[Module].Views.[Name]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:App.[Module].ViewModels"
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

        <!-- Loading indicator -->
        <ProgressBar Grid.RowSpan="3"
                     IsIndeterminate="True"
                     VerticalAlignment="Top"
                     Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>

        <!-- TODO: Add AppLabel, AppButton, ListView etc. using AppDesignSystem -->
        <!-- <local:AppLabel Grid.Row="0"
                            Text="{Binding Title}"
                            Style="{StaticResource AppTypography.HeaderSection}"/> -->

        <!-- TODO: Action button -->
        <!-- <local:AppButton Grid.Row="2"
                             Content="Submit"
                             Style="{StaticResource AppButton.Primary.Medium}"
                             Command="{Binding SubmitCommand}"/> -->
    </Grid>
</UserControl>
```

```csharp
// [Name]View.xaml.cs
using System.Windows;
using System.Windows.Controls;
using App.[Module].ViewModels;

namespace App.[Module].Views;

public partial class [Name]View : UserControl
{
    public [Name]View()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // TODO: Trigger initial load if needed
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
using App.[Module].Repositories;

namespace App.[Module].ViewModels;

public partial class [Name]ViewModel : ObservableObject
{
    // #region Fields
    private readonly I[Name]Repository _repository;
    private readonly ILogger<[Name]ViewModel> _logger;
    // #endregion

    // #region Observable Properties
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<ItemDto> _items = new();
    // #endregion

    public [Name]ViewModel(I[Name]Repository repository, ILogger<[Name]ViewModel> logger)
    {
        _repository = repository;
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
            var result = await _repository.GetListAsync(ct);
            Items = new ObservableCollection<ItemDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load items");
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
    // #endregion
}
```

## UseCase Template

```csharp
// I[Name]UseCase.cs
namespace App.[Module].Domain.UseCases;

public interface I[Name]UseCase
{
    Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken ct = default);
}

// [Name]UseCase.cs
public sealed class [Name]UseCase : I[Name]UseCase
{
    private readonly I[Name]Repository _repository;

    public [Name]UseCase(I[Name]Repository repository)
    {
        _repository = repository;
    }

    public async Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken ct = default)
    {
        // TODO: Implement business logic
        return await _repository.GetByIdAsync(input.Id, ct);
    }
}
```

## Repository Template

```csharp
// I[Name]Repository.cs
namespace App.[Module].Data.Repositories;

public interface I[Name]Repository
{
    Task<List<ItemDto>> GetListAsync(CancellationToken ct = default);
    Task<ItemDto?> GetByIdAsync(string id, CancellationToken ct = default);
}

// [Name]Repository.cs
public sealed class [Name]Repository : I[Name]Repository
{
    private readonly I[Name]Service _service;

    public [Name]Repository(I[Name]Service service)
    {
        _service = service;
    }

    public async Task<List<ItemDto>> GetListAsync(CancellationToken ct = default)
        => await _service.FetchListAsync(ct) ?? new();

    public async Task<ItemDto?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _service.FetchByIdAsync(id, ct);
}
```

## Service Template

```csharp
// I[Name]Service.cs
namespace App.[Module].Data.Services;

public interface I[Name]Service
{
    Task<List<ItemDto>?> FetchListAsync(CancellationToken ct = default);
    Task<ItemDto?> FetchByIdAsync(string id, CancellationToken ct = default);
}

// [Name]Service.cs
public sealed class [Name]Service : I[Name]Service
{
    private readonly HttpClient _http;

    public [Name]Service(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ItemDto>?> FetchListAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<ItemDto>>("/api/items", ct);

    public async Task<ItemDto?> FetchByIdAsync(string id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<ItemDto>($"/api/items/{id}", ct);
}
```

## Model Template

```csharp
// [Name]Dto.cs
using System.Text.Json.Serialization;

namespace App.[Module].Domain.Models;

public record [Name]Dto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    // TODO: Add properties matching API response
}
```

## DataTemplate (ItemViewModel for list items)

```csharp
// [Name]ItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.[Module].ViewModels;

public partial class [Name]ItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _subtitle;

    public static [Name]ItemViewModel FromDto([Name]Dto dto) =>
        new() { Title = dto.Name };
}
```

```xml
<!-- In ResourceDictionary -->
<DataTemplate DataType="{x:Type vm:[Name]ItemViewModel}">
    <Border Padding="12" Background="{StaticResource AppColor.BackgroundSecondary}">
        <StackPanel>
            <local:AppLabel Text="{Binding Title}"
                            Style="{StaticResource AppTypography.LabelSection}"/>
            <local:AppLabel Text="{Binding Subtitle}"
                            Style="{StaticResource AppTypography.BodyCaption}"
                            Foreground="{StaticResource AppColor.TextSecondary}"
                            Visibility="{Binding Subtitle, Converter={StaticResource NullToCollapsedConverter}}"/>
        </StackPanel>
    </Border>
</DataTemplate>
```

## File Types

Based on the file type requested, generate appropriate files:

### View (UserControl)
- XAML + code-behind pair
- Binds to ViewModel via `DataContext`
- Loading/error states using converters
- AppDesignSystem components only

### ViewModel
- `partial class` inheriting `ObservableObject`
- `[ObservableProperty]` for state, `[RelayCommand]` for commands
- Constructor injection of repository + `ILogger<T>`
- All async methods accept `CancellationToken`

### UseCase
- Interface + sealed implementation
- Single public `ExecuteAsync` method
- Orchestrates repository calls and business logic

### Repository
- Interface + sealed implementation
- Delegates to Service, maps DTOs to domain models

### Service
- Interface + sealed implementation
- Uses typed `HttpClient` with `System.Net.Http.Json`
- Returns nullable DTOs

### Model / DTO
- `record` for immutable DTOs
- `[JsonPropertyName]` for all properties
- `string.Empty` defaults for non-nullable strings

### DataTemplate / ItemViewModel
- `partial class` with `[ObservableProperty]`
- Static factory `FromDto()` method
- Paired XAML `DataTemplate` resource

## Output

Generate basic scaffolding with:

1. Required using statements
2. Proper region/section comments
3. MVVM interface structure
4. CommunityToolkit.Mvvm patterns
5. TODO comments for implementation
6. AppDesignSystem XAML components only

Keep implementations minimal with TODO guidance for developers.
