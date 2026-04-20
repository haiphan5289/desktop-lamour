---
description: "Scaffold barebone C# and XAML files for Desktop Lamour following MVVM + Clean Architecture."
mode: "agent"
---

# WPF File Scaffolding — Desktop Lamour

Generate barebone files following MVVM + Clean Architecture for Desktop Lamour.

## Input

```
MODULE:    <Authentication | Employees | Inventory | ImportInvoices | ExportInvoices>
FILE_TYPE: <ViewModel | UseCase | Repository | Service | View | Model | All>
NAME:      <e.g. EmployeeList>
```

## ViewModel Template

```csharp
// Features/[Module]/ViewModels/[Name]ViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.[Module].Domain.UseCases;

namespace DesktopLamour.Features.[Module].ViewModels;

public partial class [Name]ViewModel : ViewModelBase
{
    private readonly I[Name]UseCase _useCase;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public [Name]ViewModel(I[Name]UseCase useCase)
    {
        _useCase = useCase;
    }

    [RelayCommand]
    private async Task Load[Name]Async(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            // TODO: call _useCase.ExecuteAsync(ct)
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

## UseCase Template

```csharp
// Features/[Module]/Domain/UseCases/I[Name]UseCase.cs
namespace DesktopLamour.Features.[Module].Domain.UseCases;

public interface I[Name]UseCase
{
    Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken ct = default);
}

// Features/[Module]/Domain/UseCases/[Name]UseCase.cs
public class [Name]UseCase : I[Name]UseCase
{
    private readonly I[Name]Repository _repository;

    public [Name]UseCase(I[Name]Repository repository)
    {
        _repository = repository;
    }

    public async Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken ct = default)
    {
        // TODO: business logic
        throw new NotImplementedException();
    }
}
```

## Repository Template

```csharp
// Features/[Module]/Data/Repositories/I[Name]Repository.cs
namespace DesktopLamour.Features.[Module].Data.Repositories;

public interface I[Name]Repository
{
    Task<[OutputType]> [Action]Async([InputType] input, CancellationToken ct = default);
}

// Features/[Module]/Data/Repositories/[Name]Repository.cs
public class [Name]Repository : I[Name]Repository
{
    private readonly I[Name]Service _service;

    public [Name]Repository(I[Name]Service service)
    {
        _service = service;
    }

    public async Task<[OutputType]> [Action]Async([InputType] input, CancellationToken ct = default)
    {
        var dto = await _service.[Action]Async(input, ct);
        // TODO: map DTO → domain model
        throw new NotImplementedException();
    }
}
```

## Service Template

```csharp
// Features/[Module]/Data/Services/I[Name]Service.cs
namespace DesktopLamour.Features.[Module].Data.Services;

public interface I[Name]Service
{
    Task<[ResponseDto]?> [Action]Async([RequestDto] request, CancellationToken ct = default);
}

// Features/[Module]/Data/Services/[Name]Service.cs
public class [Name]Service : I[Name]Service
{
    private readonly HttpClient _http;

    public [Name]Service(HttpClient http)
    {
        _http = http;
    }

    public async Task<[ResponseDto]?> [Action]Async([RequestDto] request, CancellationToken ct = default)
    {
        return await _http.PostAsJsonAsync<[RequestDto], [ResponseDto]>(
            "/api/endpoint", request, ct);
    }
}
```

## View (UserControl) Template

```xml
<!-- Features/[Module]/Views/[Name]View.xaml -->
<UserControl x:Class="DesktopLamour.Features.[Module].Views.[Name]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="clr-namespace:DesktopLamour.Shared.Controls"
             Background="{StaticResource AppColor.BackgroundPrimary}">
    <Grid>
        <!-- Loading overlay -->
        <Grid Panel.ZIndex="100"
              Background="{StaticResource AppColor.BackgroundOverlay}"
              Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}">
            <ProgressBar IsIndeterminate="True" Width="48" Height="48"
                         HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Grid>

        <!-- Content -->
        <StackPanel Margin="24">
            <controls:AppLabel Text="[Title]"
                               Style="{StaticResource AppTypography.HeaderPage}"/>
            <!-- TODO: add content -->
        </StackPanel>
    </Grid>
</UserControl>
```

## DI Registration

```csharp
// Features/[Module]/[Module]ServiceCollectionExtensions.cs
public static IServiceCollection Add[Module](this IServiceCollection services)
{
    services.AddHttpClient<I[Name]Service, [Name]Service>(c =>
        c.BaseAddress = new Uri("https://api.example.com"));

    services.AddScoped<I[Name]Repository, [Name]Repository>();
    services.AddScoped<I[Name]UseCase, [Name]UseCase>();
    services.AddTransient<[Name]ViewModel>();

    return services;
}
```
