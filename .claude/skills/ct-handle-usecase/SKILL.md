---
name: ct-handle-usecase
description: Add a UseCase execution method to an existing ViewModel in Desktop Lamour. Generates [RelayCommand] async method that calls UseCase.ExecuteAsync(), handles IsLoading = true/false, ErrorMessage, and updates ObservableCollection or property. Always try/catch/finally, always CancellationToken.
model: haiku
effort: low
---

# Add UseCase to Existing ViewModel

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Add a UseCase execution method to an existing ViewModel following Desktop Lamour's MVVM + Clean Architecture pattern.

## Input Format

```
USECASE_NAME: <e.g. GetInventoryItems>
VIEWMODEL_FILE: <e.g. InventoryListViewModel.cs>
INPUT_TYPE: <e.g. void | string | GetInventoryRequest>
OUTPUT_TYPE: <e.g. IEnumerable<InventoryItem> | InventoryItem>
```

## Pre-Generation Steps

1. Read the `VIEWMODEL_FILE` to see existing `[ObservableProperty]` fields and constructor
2. Verify `I[UseCaseName]UseCase` interface exists in Domain layer
3. Check if `IsLoading` and `ErrorMessage` already declared (don't redeclare)

---

## Generated Method Template

### For collection output (IEnumerable/List)

```csharp
// ADD to existing ViewModel class body

// 1. Add to constructor parameter list:
private readonly I[UseCaseName]UseCase _[useCaseName]UseCase;

// In constructor:
_[useCaseName]UseCase = [useCaseName]UseCase;

// 2. Add ObservableCollection (if not already declared):
public ObservableCollection<[OutputItem]> Items { get; } = new();

// 3. Add RelayCommand method:
[RelayCommand]
private async Task [UseCaseName]Async(CancellationToken ct = default)
{
    try
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        var result = await _[useCaseName]UseCase.ExecuteAsync(ct);

        Items.Clear();
        foreach (var item in result)
            Items.Add(item);
    }
    catch (OperationCanceledException)
    {
        // Cancellation is expected — do not show error
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
```

### For single-object output

```csharp
// ADD ObservableProperty for the result:
[ObservableProperty]
private [OutputType]? _result;

[RelayCommand]
private async Task [UseCaseName]Async(CancellationToken ct = default)
{
    try
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        Result = await _[useCaseName]UseCase.ExecuteAsync(ct);
    }
    catch (OperationCanceledException)
    {
        // Expected — do not show error
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
```

### For input-based UseCase

```csharp
[RelayCommand]
private async Task [UseCaseName]Async([InputType] input, CancellationToken ct = default)
{
    try
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        var result = await _[useCaseName]UseCase.ExecuteAsync(input, ct);
        // Update Items or Result accordingly
    }
    catch (OperationCanceledException) { }
    catch (Exception ex)
    {
        ErrorMessage = ex.Message;
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

## Critical Rules

### NEVER do these:

```csharp
// BAD: No cancellation token
private async Task LoadAsync()
{
    var result = await _useCase.ExecuteAsync(); // Missing CancellationToken
}

// BAD: Missing finally block
catch (Exception ex)
{
    ErrorMessage = ex.Message;
}
// IsLoading never set back to false if exception thrown!

// BAD: .Result or .Wait()
var result = _useCase.ExecuteAsync().Result; // Deadlock risk

// BAD: Accessing generated field name in XAML
// Field: _isLoading → binding must use IsLoading (generated property)
```

### ALWAYS do these:

```csharp
// GOOD: Full try/catch/finally with CancellationToken
[RelayCommand]
private async Task LoadAsync(CancellationToken ct = default)
{
    try
    {
        IsLoading = true;      // uses generated property
        ErrorMessage = string.Empty;
        // ... await useCase
    }
    catch (OperationCanceledException) { }
    catch (Exception ex)
    {
        ErrorMessage = ex.Message;
    }
    finally
    {
        IsLoading = false;  // ALWAYS in finally
    }
}
```

---

## Generated Property Name Reference

| Field declaration | Generated property name |
|---|---|
| `private bool _isLoading;` | `IsLoading` |
| `private string _errorMessage;` | `ErrorMessage` |
| `private Employee? _selectedEmployee;` | `SelectedEmployee` |
| `private string _searchQuery;` | `SearchQuery` |

---

## DI Registration Reminder

After adding the UseCase dependency to the ViewModel constructor, update DI registration:

```csharp
// In [Module]ServiceExtensions.cs
services.AddScoped<I[UseCaseName]UseCase, [UseCaseName]UseCase>();
services.AddTransient<[ViewModel]>(); // Re-register to pick up new constructor params
```
