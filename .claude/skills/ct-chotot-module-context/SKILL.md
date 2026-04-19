---
name: ct-chotot-module-context
description: Quick reference for Desktop Lamour module architecture, MVVM + Clean Architecture patterns, DI setup, and standard file naming. Use when working on Authentication, Employees, Inventory, ImportInvoices, or ExportInvoices — understanding directory structure, service registration, ViewModel patterns, UseCase/Repository conventions, and module-specific business rules.
model: sonnet
effort: low
argument-hint: "[module name or pattern — e.g. Employees, ImportInvoices, DI, ViewModel]"
---

# Desktop Lamour Module Context

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Quick reference for architecture patterns, file locations, and conventions used across all Desktop Lamour modules.

---

## How to Use

```
/ct-chotot-module-context Employees
/ct-chotot-module-context ImportInvoices
/ct-chotot-module-context DI registration
/ct-chotot-module-context ViewModel pattern
```

---

## Solution Structure

```
desktop-lamour/
├── src/
│   └── DesktopLamour/
│       ├── App.xaml / App.xaml.cs          — DI host, startup
│       ├── MainWindow/
│       │   ├── MainWindow.xaml             — Shell window
│       │   └── MainWindow.xaml.cs
│       ├── Features/
│       │   ├── Authentication/
│       │   ├── Employees/
│       │   ├── Inventory/
│       │   ├── ImportInvoices/
│       │   └── ExportInvoices/
│       ├── Shared/
│       │   ├── Controls/                   — AppLabel, custom controls
│       │   ├── Converters/                 — BoolToVisibility, etc.
│       │   └── AppStyles.xaml
│       └── Themes/
│           └── AppTypography.xaml
└── tests/
    └── DesktopLamour.Tests/
        └── Features/
            ├── Employees/
            ├── Inventory/
            └── ...
```

---

## Per-Module Structure

Each feature module follows the same 5-layer layout:

```
Features/[Module]/
├── Domain/
│   ├── Models/
│   │   └── [Entity].cs                     — record type, no dependencies
│   └── UseCases/
│       ├── I[Feature]UseCase.cs            — interface
│       └── [Feature]UseCase.cs             — implementation, depends on IRepository
├── Data/
│   ├── Repositories/
│   │   ├── I[Entity]Repository.cs          — interface
│   │   └── [Entity]Repository.cs           — depends on IService, maps DTOs→Domain
│   └── Services/
│       ├── I[Entity]Service.cs             — interface
│       ├── [Entity]Service.cs              — HttpClient calls
│       └── DTOs/
│           ├── [Entity]Dto.cs              — [JsonPropertyName] attributes
│           └── Create[Entity]Request.cs
├── ViewModels/
│   └── [Name]ViewModel.cs                  — partial class : ObservableObject
├── Views/
│   ├── [Name]View.xaml                     — UserControl
│   ├── [Name]View.xaml.cs
│   ├── [Name]Window.xaml                   — Window (dialogs)
│   └── [Name]Window.xaml.cs
└── [Module]ServiceExtensions.cs            — DI registration
```

---

## Module Summary

| Module | Entity | Key Business Rules |
|--------|--------|-------------------|
| `Authentication` | — | Phone-based login; role = Admin / Thu ngân / Kho |
| `Employees` | `Employee` | Admin-only create/delete; unique phone number |
| `Inventory` | `Product` | Stock quantity never negative; unit of measure required |
| `ImportInvoices` | `ImportInvoice` | Confirmed = immutable; stock increases on confirm |
| `ExportInvoices` | `ExportInvoice` | Confirmed = immutable; stock decreases on confirm; reject if insufficient stock |

---

## MVVM Data Flow

```
View (XAML)
  ↕ Binding (ObservableProperty, RelayCommand)
ViewModel (partial class : ObservableObject)
  ↓ ExecuteAsync()
IUseCase (Domain)
  ↓ Repository method
IRepository (Data)
  ↓ Service call
IService (Data)
  ↓ HttpClient
REST API
```

---

## Standard ViewModel Pattern

```csharp
// Features/[Module]/ViewModels/[Name]ViewModel.cs
public partial class [Name]ViewModel : ObservableObject
{
    private readonly I[Feature]UseCase _useCase;

    public [Name]ViewModel(I[Feature]UseCase useCase)
        => _useCase = useCase;

    // State
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private [Entity]? _selectedItem;

    // List
    public ObservableCollection<[Entity]> Items { get; } = [];

    // Command — generated name: LoadAsyncCommand
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _useCase.ExecuteAsync(ct);
            Items.Clear();
            foreach (var item in items) Items.Add(item);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsLoading = false; }
    }
}
```

Key rules:
- `partial class` — required for CommunityToolkit.Mvvm source generators
- `_camelCase` field → binding uses generated `PascalCase` property name
- `ObservableCollection<T>` not `List<T>`
- `OperationCanceledException` always caught separately
- `finally { IsLoading = false; }` always present

---

## DI Registration Pattern

```csharp
// Features/[Module]/[Module]ServiceExtensions.cs
public static class [Module]ServiceExtensions
{
    public static IServiceCollection Add[Module]Services(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Service — HttpClient
        services.AddHttpClient<I[Entity]Service, [Entity]Service>(client =>
            client.BaseAddress = new Uri(configuration["Api:BaseUrl"]!));

        // Repository — Scoped
        services.AddScoped<I[Entity]Repository, [Entity]Repository>();

        // UseCases — Scoped
        services.AddScoped<I[Feature]UseCase, [Feature]UseCase>();

        // ViewModels — Transient (new instance per Window/View)
        services.AddTransient<[Name]ViewModel>();

        // Windows/Views — Transient
        services.AddTransient<[Name]View>();
        services.AddTransient<[Name]Window>();

        return services;
    }
}
```

Register in `App.xaml.cs`:
```csharp
services.AddEmployeesServices(configuration);
services.AddInventoryServices(configuration);
services.AddImportInvoicesServices(configuration);
services.AddExportInvoicesServices(configuration);
```

---

## Generated Name Reference

| Declaration | Binding path in XAML |
|---|---|
| `[ObservableProperty] private bool _isLoading;` | `{Binding IsLoading}` |
| `[ObservableProperty] private string _errorMessage;` | `{Binding ErrorMessage}` |
| `[ObservableProperty] private Employee? _selectedEmployee;` | `{Binding SelectedEmployee}` |
| `[RelayCommand] private async Task LoadAsync()` | `{Binding LoadAsyncCommand}` |
| `[RelayCommand] private void Delete(int id)` | `{Binding DeleteCommand}` |
| `[RelayCommand] private async Task CreateAsync()` | `{Binding CreateAsyncCommand}` |

---

## Standard File Naming

| What | Pattern | Example |
|------|---------|---------|
| Domain model | `[Entity].cs` | `Employee.cs` |
| UseCase interface | `I[Feature]UseCase.cs` | `ICreateEmployeeUseCase.cs` |
| UseCase implementation | `[Feature]UseCase.cs` | `CreateEmployeeUseCase.cs` |
| Repository interface | `I[Entity]Repository.cs` | `IEmployeeRepository.cs` |
| Repository implementation | `[Entity]Repository.cs` | `EmployeeRepository.cs` |
| Service interface | `I[Entity]Service.cs` | `IEmployeeService.cs` |
| Service implementation | `[Entity]Service.cs` | `EmployeeService.cs` |
| Response DTO | `[Entity]Dto.cs` | `EmployeeDto.cs` |
| Request DTO | `Create[Entity]Request.cs` | `CreateEmployeeRequest.cs` |
| ViewModel | `[Name]ViewModel.cs` | `EmployeesViewModel.cs` |
| List view | `[Name]View.xaml` | `EmployeesView.xaml` |
| Dialog window | `[Name]Window.xaml` | `CreateEmployeeWindow.xaml` |
| DI extensions | `[Module]ServiceExtensions.cs` | `EmployeesServiceExtensions.cs` |
| xUnit test class | `[Feature]UseCaseTests.cs` | `CreateEmployeeUseCaseTests.cs` |

---

## Common Patterns Quick Reference

### Open a dialog from ViewModel

```csharp
var dialog = _serviceProvider.GetRequiredService<CreateEmployeeWindow>();
dialog.Owner = Application.Current.MainWindow;
var result = dialog.ShowDialog();
if (result == true)
    await LoadAsyncCommand.ExecuteAsync(null);
```

### DataTemplate command binding (DataGrid row → parent ViewModel)

```xml
<Button Command="{Binding DataContext.DeleteCommand,
                  RelativeSource={RelativeSource AncestorType=DataGrid}}"
        CommandParameter="{Binding Id}"/>
```

### Visibility converters (registered in App.xaml)

```xml
Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
Visibility="{Binding IsLoading, Converter={StaticResource InverseBoolToVisibilityConverter}}"
Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"
```

See `docs/project-overview.md` for full business domain context.
