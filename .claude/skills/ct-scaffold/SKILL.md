---
name: ct-scaffold
description: Scaffold barebone C# files for Desktop Lamour following MVVM + Clean Architecture. Generates ViewModel, UseCase (interface+impl), Repository (interface+impl), Service (interface+impl), UserControl (XAML+codebehind), or Model with correct namespace, DI-ready constructor injection, and CommunityToolkit.Mvvm attributes.
model: haiku
effort: low
---

# C# File Scaffolding for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Scaffold barebone C# and XAML files following MVVM + Clean Architecture patterns.

## Input Format

```
FILE_TYPE: <ViewModel | UseCase | Repository | Service | UserControl | Model | DTO>
NAME: <BaseName, e.g. "EmployeeList">
MODULE: <Module name, e.g. "Employees">
DESCRIPTION: <brief description of the class purpose>
```

---

## ViewModel Template

```csharp
// Namespace: DesktopLamour.Features.[Module].ViewModels
// File: src/DesktopLamour/Features/[Module]/ViewModels/[Name]ViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Features.[Module].Domain;

namespace DesktopLamour.Features.[Module].ViewModels;

public partial class [Name]ViewModel : ObservableObject
{
    private readonly I[Name]UseCase _[name]UseCase;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // [ObservableProperty]
    // private ObservableCollection<[Entity]> _items = new();

    public [Name]ViewModel(I[Name]UseCase [name]UseCase)
    {
        _[name]UseCase = [name]UseCase;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            // var result = await _[name]UseCase.ExecuteAsync(ct);
            // Items = new ObservableCollection<[Entity]>(result);
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

---

## UseCase Interface + Implementation Templates

```csharp
// Interface
// File: src/DesktopLamour/Features/[Module]/Domain/I[Name]UseCase.cs

namespace DesktopLamour.Features.[Module].Domain;

public interface I[Name]UseCase
{
    Task<[OutputType]> ExecuteAsync(CancellationToken ct = default);
}
```

```csharp
// Implementation
// File: src/DesktopLamour/Features/[Module]/Domain/[Name]UseCase.cs

namespace DesktopLamour.Features.[Module].Domain;

public class [Name]UseCase : I[Name]UseCase
{
    private readonly I[Name]Repository _repository;

    public [Name]UseCase(I[Name]Repository repository)
    {
        _repository = repository;
    }

    public async Task<[OutputType]> ExecuteAsync(CancellationToken ct = default)
    {
        // Business logic here
        return await _repository.GetAsync(ct);
    }
}
```

---

## Repository Interface + Implementation Templates

```csharp
// Interface
// File: src/DesktopLamour/Features/[Module]/Data/I[Name]Repository.cs

namespace DesktopLamour.Features.[Module].Data;

public interface I[Name]Repository
{
    Task<[OutputType]> GetAsync(CancellationToken ct = default);
    // Task CreateAsync([InputType] request, CancellationToken ct = default);
    // Task UpdateAsync(int id, [InputType] request, CancellationToken ct = default);
    // Task DeleteAsync(int id, CancellationToken ct = default);
}
```

```csharp
// Implementation
// File: src/DesktopLamour/Features/[Module]/Data/[Name]Repository.cs

namespace DesktopLamour.Features.[Module].Data;

public class [Name]Repository : I[Name]Repository
{
    private readonly I[Name]Service _service;

    public [Name]Repository(I[Name]Service service)
    {
        _service = service;
    }

    public async Task<[OutputType]> GetAsync(CancellationToken ct = default)
    {
        var dto = await _service.GetAsync(ct);
        // Map DTO to domain model if needed
        return dto;
    }
}
```

---

## Service Interface + Implementation Templates

```csharp
// Interface
// File: src/DesktopLamour/Features/[Module]/Data/I[Name]Service.cs

namespace DesktopLamour.Features.[Module].Data;

public interface I[Name]Service
{
    Task<[ResponseDto]> GetAsync(CancellationToken ct = default);
    // Task<[ResponseDto]> CreateAsync([RequestDto] request, CancellationToken ct = default);
}
```

```csharp
// Implementation
// File: src/DesktopLamour/Features/[Module]/Data/[Name]Service.cs

using System.Net.Http.Json;

namespace DesktopLamour.Features.[Module].Data;

public class [Name]Service : I[Name]Service
{
    private readonly HttpClient _httpClient;

    public [Name]Service(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<[ResponseDto]> GetAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<[ResponseDto]>(
            "/api/[endpoint]", ct);
        return response ?? throw new InvalidOperationException("No response received.");
    }
}
```

---

## UserControl Template (XAML + Code-Behind)

```xml
<!-- File: src/DesktopLamour/Features/[Module]/Views/[Name]View.xaml -->
<UserControl x:Class="DesktopLamour.Features.[Module].Views.[Name]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <!-- Loading overlay -->
        <!-- <Grid Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibility}}">
            <TextBlock Text="Đang tải..." Style="{StaticResource TextBodyStyle}"/>
        </Grid> -->

        <!-- Error message -->
        <!-- <TextBlock Text="{Binding ErrorMessage}"
                      Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibility}}"
                      Style="{StaticResource TextErrorStyle}"/> -->

        <!-- Main content -->
    </Grid>
</UserControl>
```

```csharp
// File: src/DesktopLamour/Features/[Module]/Views/[Name]View.xaml.cs

namespace DesktopLamour.Features.[Module].Views;

public partial class [Name]View : UserControl
{
    public [Name]View()
    {
        InitializeComponent();
    }
}
```

---

## Model Template

```csharp
// File: src/DesktopLamour/Features/[Module]/Domain/[Name].cs

namespace DesktopLamour.Features.[Module].Domain;

public class [Name]
{
    public int Id { get; set; }
    // Add domain properties here
}
```

---

## DTO Template

```csharp
// File: src/DesktopLamour/Features/[Module]/Data/DTOs/[Name]Dto.cs

using System.Text.Json.Serialization;

namespace DesktopLamour.Features.[Module].Data.DTOs;

public class [Name]Dto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    // [JsonPropertyName("field_name")]
    // public string FieldName { get; set; } = string.Empty;
}
```

---

## Rules

- ViewModel must be `partial class` and inherit `ObservableObject`
- All `[ObservableProperty]` fields must be `private` with underscore prefix
- All UseCase methods must accept `CancellationToken ct = default`
- Never call `.Result` or `.Wait()` on async methods
- Inject interfaces, never concrete types
- Namespace must match folder path exactly
