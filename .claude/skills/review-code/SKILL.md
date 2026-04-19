---
name: review-code
description: C# WPF code review for Desktop Lamour — MVVM Clean Architecture compliance, CommunityToolkit.Mvvm patterns, XAML style compliance (AppStyles/AppTypography), DI registration correctness, async/await correctness, no business logic in View, interface usage. Use when asked to review C# or XAML files.
model: sonnet
effort: high
---

# C# WPF Code Review for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

Full code review for C# and XAML files in **Desktop Lamour** (WPF .NET 8, MVVM + Clean Architecture).

## Focus Areas

| Area | Invoke with |
|---|---|
| Architecture compliance | `FOCUS: Architecture` |
| ViewModel patterns (CommunityToolkit.Mvvm) | `FOCUS: ViewModel` |
| XAML style compliance | `FOCUS: XAML` |
| DI registration | `FOCUS: DI` |
| Async/await correctness | `FOCUS: Async` |
| Business rules compliance | `FOCUS: Business` |
| Full review | `FOCUS: All` |

---

## Architecture Checklist

```
LAYER SEPARATION
[ ] View (XAML + code-behind) contains ONLY: InitializeComponent, DataContext binding, event-to-command wiring
[ ] View does NOT contain: business logic, direct service calls, data transformation
[ ] ViewModel inherits ObservableObject (never INotifyPropertyChanged manually)
[ ] ViewModel does NOT reference any WPF/UI types (no UIElement, no Window)
[ ] UseCase has single responsibility — one ExecuteAsync method
[ ] UseCase injects IRepository interface, not concrete Repository
[ ] Repository injects IService interface, not concrete Service
[ ] Service injects HttpClient — no direct instantiation of HttpClient

NAMING CONVENTIONS
[ ] ViewModel: [Feature]ViewModel.cs
[ ] UseCase interface: I[Name]UseCase.cs
[ ] UseCase implementation: [Name]UseCase.cs
[ ] Repository interface: I[Name]Repository.cs
[ ] Repository: [Name]Repository.cs
[ ] Service interface: I[Name]Service.cs
[ ] Service: [Name]Service.cs
[ ] DTO: [Name]Dto.cs (in Data/DTOs/)

NAMESPACE
[ ] Namespace matches folder path: DesktopLamour.Features.[Module].[Layer]
[ ] No cross-module namespace references without abstraction
```

---

## ViewModel Patterns Checklist

```
COMMUNITYTOOOLKIT.MVVM
[ ] ViewModel class is partial: partial class [Name]ViewModel : ObservableObject
[ ] All observable fields use [ObservableProperty] attribute
[ ] All command methods use [RelayCommand] attribute
[ ] [ObservableProperty] field is private with underscore prefix: _isLoading
[ ] Generated property name (PascalCase) used in XAML bindings — NOT the field name
[ ] No manual PropertyChanged.Invoke or OnPropertyChanged() calls
[ ] No manual ICommand implementation — always [RelayCommand]

STATE MANAGEMENT
[ ] IsLoading properly set to true before async and false in finally
[ ] ErrorMessage cleared before each operation
[ ] ObservableCollection used for lists — not List<T> or IEnumerable<T>
[ ] All [RelayCommand] async methods accept CancellationToken ct = default
[ ] try/catch/finally pattern on all async commands
```

---

## XAML Style Checklist

```
STYLE USAGE
[ ] All TextBlock uses StaticResource style key — no inline FontSize/FontWeight
[ ] All Button uses StaticResource style key — no inline Background/Foreground
[ ] No hardcoded color values: #FF5733, Red, Colors.Blue
[ ] No hardcoded font sizes: FontSize="14"
[ ] Verify every StaticResource key exists in AppStyles.xaml or AppTypography.xaml

BINDING PATTERNS
[ ] Two-way TextBox bindings use UpdateSourceTrigger=PropertyChanged
[ ] Command bindings use {Binding XxxCommand} — not Click="..."
[ ] Boolean-to-Visibility uses StaticResource converter — not code-behind
[ ] No x:Name references in ViewModel — only ViewModel properties
[ ] DataContext set via DI — not new ViewModel() in code-behind

DATA GRID / LISTS
[ ] DataGrid columns have explicit Header and Binding
[ ] DataGrid AutoGenerateColumns="False" when columns are defined
[ ] Command parameters in DataTemplates use RelativeSource binding to reach parent ViewModel
```

---

## DI Registration Checklist

```
[ ] Every interface+implementation pair is registered in ServiceCollectionExtensions
[ ] HttpClient registered via AddHttpClient<IService, Service>() — not AddSingleton<HttpClient>()
[ ] ViewModel registered as Transient (new instance per view)
[ ] UseCase registered as Scoped or Transient
[ ] Repository registered as Scoped
[ ] No "new" instantiation of dependencies in ViewModels or Services
[ ] DI registration file exists for each module: [Module]ServiceExtensions.cs
```

---

## Async/Await Checklist

```
[ ] No .Result or .Wait() calls — always await
[ ] No async void (except event handlers) — always async Task
[ ] CancellationToken propagated through all layers
[ ] EnsureSuccessStatusCode() called on HTTP mutations (POST/PUT/DELETE)
[ ] HttpClient methods use *FromJsonAsync / *AsJsonAsync from System.Net.Http.Json
[ ] No blocking calls in UI thread (no Task.Run(() => ...GetResult()))
[ ] OperationCanceledException caught and handled gracefully
```

---

## Business Rules Checklist

```
[ ] Stock never goes negative (validation in UseCase, not ViewModel)
[ ] Invoice total = sum of line items (calculated in domain model or UseCase)
[ ] Export invoice cannot be confirmed if any line item stock < requested quantity
[ ] Import invoice increases stock on confirmation
[ ] Employee role checked before allowing admin-only operations
[ ] Confirmed invoices are immutable (UseCase throws if modification attempted)
```

---

## Key Rules — Always Apply

| Forbidden | Required |
|---|---|
| `FontSize="14"` in XAML | `Style="{StaticResource TextBodyStyle}"` |
| `Background="#FF0000"` | `Background="{StaticResource PrimaryBrush}"` |
| `var vm = new EmployeeViewModel()` in code-behind | Inject via DI |
| `_isLoading` in XAML binding | `IsLoading` (generated property) |
| `.Result` on async method | `await` |
| Manual `INotifyPropertyChanged` | `[ObservableProperty]` |
| `List<T>` for bindable data | `ObservableCollection<T>` |
| Business logic in XAML event handler | Move to UseCase |

See `docs/project-overview.md` for business rules and architecture context.
