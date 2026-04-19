---
name: ct-alternative-approaches
description: Generate 3–5 alternative solutions for C#/.NET WPF problems in Desktop Lamour. Pros/cons, C#/XAML code examples, comparison matrix, decision framework. Use before committing to architecture or implementation strategy.
model: sonnet
effort: high
---

# WPF Alternative Approaches — Multiple Solution Analysis

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

Generates 3–5 alternative solutions for C#/WPF development problems in Desktop Lamour, with pros/cons, code examples, a comparison matrix, and a decision framework. Use before committing to an approach.

## When to Use

- Multiple viable architectural approaches exist
- Deciding between MVVM patterns (property-per-state vs. state object)
- Choosing between HttpClient lifetime strategies
- Evaluating navigation approaches (frame-based vs. region-based)
- Comparing async patterns (async/await vs. reactive)
- Deciding between DataGrid vs. ListView for a list screen

## Input Format

```
PROBLEM: <WPF/C# problem or design decision>
CONTEXT: <Module name, feature description>
COMPLEXITY_LEVEL: <Simple | Medium | Complex>
FOCUS_AREAS: <optional — e.g. performance, testability, XAML simplicity>
SOLUTION_COUNT: <3-5, optional>
```

## Analysis Structure

### 1. Problem Analysis

- Describe the core constraint or trade-off
- List the evaluation criteria (testability, performance, XAML verbosity, DI compatibility, maintainability)
- Note Desktop Lamour-specific constraints (WPF .NET 8, CommunityToolkit.Mvvm 8.3.2, MVVM + Clean Architecture)

### 2. Solutions (3–5 Alternatives)

Each solution includes:

```
## Solution N: [Approach Name]

### Core Concept
Brief description of the approach.

### C# / XAML Example
[Code snippet demonstrating the approach]

### Pros
- Advantage 1
- Advantage 2

### Cons
- Disadvantage 1
- Disadvantage 2

### Best For
- When to choose this approach

### Complexity Score
- Dev time: Short / Medium / Long
- Testability: Easy / Moderate / Hard
- XAML complexity: Low / Medium / High
```

### 3. Comparison Matrix

| Criteria | Solution A | Solution B | Solution C |
|---|---|---|---|
| Development Time | ... | ... | ... |
| Testability | ... | ... | ... |
| XAML Complexity | ... | ... | ... |
| DI Compatibility | ... | ... | ... |
| Maintainability | ... | ... | ... |
| Score (1-5) | ... | ... | ... |

### 4. Decision Framework

```
If testability is top priority → Solution X
If XAML simplicity is required → Solution Y
If performance for large lists → Solution Z
```

---

## Example

### Sample Input

```
PROBLEM: How to handle navigation between modules (Authentication, Employees, Inventory, etc.)
CONTEXT: MainWindow shell
COMPLEXITY_LEVEL: Medium
FOCUS_AREAS: Testability, XAML simplicity, DI compatibility
```

### Solution 1: Frame + Page Navigation

```csharp
// MainViewModel.cs
[ObservableProperty]
private Page _currentPage;

[RelayCommand]
private void NavigateTo(string pageName)
{
    CurrentPage = _serviceProvider.GetRequiredService<EmployeesPage>();
}
```

```xml
<!-- MainWindow.xaml -->
<Frame Content="{Binding CurrentPage}" NavigationUIVisibility="Hidden"/>
```

- Pros: Simple WPF-native, no extra libraries
- Cons: Pages carry back-stack, harder to inject ViewModels

### Solution 2: ContentControl + DataTemplate

```csharp
[ObservableProperty]
private ObservableObject _currentViewModel;

[RelayCommand]
private void ShowEmployees()
{
    CurrentViewModel = _serviceProvider.GetRequiredService<EmployeeListViewModel>();
}
```

```xml
<ContentControl Content="{Binding CurrentViewModel}">
    <ContentControl.Resources>
        <DataTemplate DataType="{x:Type vm:EmployeeListViewModel}">
            <views:EmployeeListView/>
        </DataTemplate>
    </ContentControl.Resources>
</ContentControl>
```

- Pros: Full ViewModel-first navigation, clean MVVM, easily testable
- Cons: Requires DataTemplate registration per view

### Solution 3: UserControl Visibility Toggling

```xml
<Grid>
    <views:EmployeeListView Visibility="{Binding IsEmployeesVisible, Converter={StaticResource BoolToVisibility}}"/>
    <views:InventoryView Visibility="{Binding IsInventoryVisible, Converter={StaticResource BoolToVisibility}}"/>
</Grid>
```

- Pros: Zero navigation infrastructure, instant switching
- Cons: All views loaded at startup, scales poorly beyond 3-4 screens

**Recommendation for Desktop Lamour:** Solution 2 (ContentControl + DataTemplate) — best testability, ViewModel-first, DI-compatible.
