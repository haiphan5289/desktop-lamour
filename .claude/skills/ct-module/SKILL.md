---
name: ct-module
description: Generate a complete Desktop Lamour feature module — Domain (Model + IUseCase + UseCase) + Data (IRepository + Repository + IService + Service + DTOs) + Views (UserControl XAML) + ViewModels + ServiceCollectionExtensions DI registration. Use when creating a new feature module from scratch.
model: sonnet
effort: high
---

# Complete Module Generator for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate a full MVVM + Clean Architecture feature module with all 5 layers wired together.

## Input Format

```
MODULE_NAME: <ModuleName, e.g. "Employees">
DESCRIPTION: <brief module description>
ENTITIES: <comma-separated domain entities, e.g. "Employee">
```

## Output Files

For a module named `Employees` with entity `Employee`:

```
src/DesktopLamour/Features/Employees/
├── Domain/
│   ├── Employee.cs                          (domain model)
│   ├── IGetEmployeesUseCase.cs             (use case interface)
│   └── GetEmployeesUseCase.cs              (use case implementation)
├── Data/
│   ├── DTOs/
│   │   └── EmployeeDto.cs                  (API response DTO)
│   ├── IEmployeeRepository.cs              (repository interface)
│   ├── EmployeeRepository.cs               (repository implementation)
│   ├── IEmployeeService.cs                 (service interface)
│   └── EmployeeService.cs                  (service implementation)
├── ViewModels/
│   └── EmployeeListViewModel.cs            (ViewModel)
├── Views/
│   ├── EmployeeListView.xaml               (UserControl XAML)
│   └── EmployeeListView.xaml.cs            (code-behind)
└── EmployeesServiceExtensions.cs           (DI registration)
```

---

## Domain/Employee.cs

```csharp
namespace DesktopLamour.Features.Employees.Domain;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin | Thu ngân | Kho
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## Domain/IGetEmployeesUseCase.cs + GetEmployeesUseCase.cs

```csharp
namespace DesktopLamour.Features.Employees.Domain;

public interface IGetEmployeesUseCase
{
    Task<IEnumerable<Employee>> ExecuteAsync(CancellationToken ct = default);
}

public class GetEmployeesUseCase : IGetEmployeesUseCase
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeesUseCase(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Employee>> ExecuteAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(ct);
    }
}
```

---

## Data/DTOs/EmployeeDto.cs

```csharp
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.Employees.Data.DTOs;

public class EmployeeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
```

---

## Data/IEmployeeRepository.cs + EmployeeRepository.cs

```csharp
namespace DesktopLamour.Features.Employees.Data;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IEmployeeService _service;

    public EmployeeRepository(IEmployeeService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(d => new Employee
        {
            Id = d.Id,
            FullName = d.FullName,
            PhoneNumber = d.PhoneNumber,
            Role = d.Role,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt
        });
    }

    // Implement CreateAsync, UpdateAsync, DeleteAsync similarly
    public Task CreateAsync(Employee employee, CancellationToken ct = default) => throw new NotImplementedException();
    public Task UpdateAsync(Employee employee, CancellationToken ct = default) => throw new NotImplementedException();
    public Task DeleteAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
}
```

---

## Data/IEmployeeService.cs + EmployeeService.cs

```csharp
using System.Net.Http.Json;
using DesktopLamour.Features.Employees.Data.DTOs;

namespace DesktopLamour.Features.Employees.Data;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken ct = default);
    Task<EmployeeDto> CreateAsync(EmployeeDto dto, CancellationToken ct = default);
    Task<EmployeeDto> UpdateAsync(int id, EmployeeDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public class EmployeeService : IEmployeeService
{
    private readonly HttpClient _httpClient;

    public EmployeeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<EmployeeDto>>("/api/employees", ct)
               ?? Enumerable.Empty<EmployeeDto>();
    }

    public async Task<EmployeeDto> CreateAsync(EmployeeDto dto, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/employees", dto, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmployeeDto>(ct)
               ?? throw new InvalidOperationException("Failed to create employee.");
    }

    public async Task<EmployeeDto> UpdateAsync(int id, EmployeeDto dto, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/employees/{id}", dto, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmployeeDto>(ct)
               ?? throw new InvalidOperationException("Failed to update employee.");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/api/employees/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
```

---

## ViewModels/EmployeeListViewModel.cs

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Features.Employees.Domain;

namespace DesktopLamour.Features.Employees.ViewModels;

public partial class EmployeeListViewModel : ObservableObject
{
    private readonly IGetEmployeesUseCase _getEmployeesUseCase;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ObservableCollection<Employee> Employees { get; } = new();

    public EmployeeListViewModel(IGetEmployeesUseCase getEmployeesUseCase)
    {
        _getEmployeesUseCase = getEmployeesUseCase;
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            var employees = await _getEmployeesUseCase.ExecuteAsync(ct);
            Employees.Clear();
            foreach (var emp in employees)
                Employees.Add(emp);
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

## Views/EmployeeListView.xaml

```xml
<UserControl x:Class="DesktopLamour.Features.Employees.Views.EmployeeListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0" Text="Quản lý nhân viên"
                   Style="{StaticResource TextHeadingStyle}"
                   Margin="16,16,16,8"/>

        <!-- Loading indicator -->
        <TextBlock Grid.Row="1" Text="Đang tải..."
                   Style="{StaticResource TextBodyStyle}"
                   Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
                   HorizontalAlignment="Center" VerticalAlignment="Center"/>

        <!-- Error message -->
        <TextBlock Grid.Row="1" Text="{Binding ErrorMessage}"
                   Style="{StaticResource TextErrorStyle}"
                   Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"
                   Margin="16,8"/>

        <!-- Employee list -->
        <DataGrid Grid.Row="1" ItemsSource="{Binding Employees}"
                  AutoGenerateColumns="False" IsReadOnly="True"
                  Margin="16,0">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Họ tên" Binding="{Binding FullName}" Width="*"/>
                <DataGridTextColumn Header="Điện thoại" Binding="{Binding PhoneNumber}" Width="150"/>
                <DataGridTextColumn Header="Chức vụ" Binding="{Binding Role}" Width="120"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

---

## EmployeesServiceExtensions.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using DesktopLamour.Features.Employees.Data;
using DesktopLamour.Features.Employees.Domain;
using DesktopLamour.Features.Employees.ViewModels;

namespace DesktopLamour.Features.Employees;

public static class EmployeesServiceExtensions
{
    public static IServiceCollection AddEmployeesModule(this IServiceCollection services)
    {
        // Service (HttpClient)
        services.AddHttpClient<IEmployeeService, EmployeeService>();

        // Repository
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        // UseCases
        services.AddScoped<IGetEmployeesUseCase, GetEmployeesUseCase>();

        // ViewModel
        services.AddTransient<EmployeeListViewModel>();

        return services;
    }
}
```

---

## Rules

- All ViewModels are `partial class` inheriting `ObservableObject`
- All UseCase methods accept `CancellationToken ct = default`
- HttpClient is registered via `AddHttpClient<IService, Service>()`
- Never create `new HttpClient()` — always inject via DI
- DTO field names use `[JsonPropertyName]` for JSON mapping
- Namespace matches folder path exactly

See `docs/project-overview.md` for full project context.
