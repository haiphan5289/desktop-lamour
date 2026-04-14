---
name: ct-bugfix-skill
description: Debug and fix C#/.NET WPF bugs with precision. Use WHENEVER you encounter crashes, memory leaks, state not updating, UI styling mismatches, threading errors, INotifyPropertyChanged issues, or View not rendering. This skill identifies root causes by verifying MVVM data flow, checking event handler leaks, validating AppDesignSystem component usage, and confirming Dispatcher correctness. Use even if you're just suspicious about state management, binding logic, or component styling.
model: sonnet
effort: high
---

# WPF Bug Fix Skill

## Overview

This skill provides a structured debugging workflow for identifying and fixing C#/.NET WPF bugs. It covers both general WPF debugging patterns (MVVM bindings, threading, memory, state management) and application-specific issues (MVVM architecture, AppDesignSystem, module patterns).

## When to Use This Skill

**Use this skill when:**
- Debugging crashes, memory leaks, or unexpected behavior
- Investigating UI glitches or state synchronization issues
- Fixing test failures or race conditions
- Troubleshooting data binding or `INotifyPropertyChanged` issues
- Verifying MVVM data flow in features
- Validating AppDesignSystem component usage
- Debugging `ObservableCollection<T>` or `[ObservableProperty]` update issues

## Core Debugging Workflow

### Step 1: Limit Scope (Read 3-4 Files Max)

Only read the specific files the user mentions. Never explore broadly.

**Good scope:**
- View (.xaml + .xaml.cs) that exhibits the issue
- Associated ViewModel
- Related UseCase or Repository

**Avoid exploring:**
- Entire modules
- All dependencies transitively
- "Nearby" files unless directly relevant

### Step 2: Identify Root Cause

State the root cause clearly and concisely. Ask yourself:

**For UI bugs:**
- Is the data binding correct? (`{Binding PropertyName, Mode=TwoWay}` pointing to correct property?)
- Are UI components using AppDesignSystem? (not raw WPF primitives like `TextBlock` without style)
- Is the XAML layout correct? (`Grid`, `StackPanel` — no code-behind manual sizing)
- Is there a threading issue? (UI updates on `Application.Current.Dispatcher.Invoke`?)
- Is the View lifecycle correct? (`OnLoaded` → `OnNavigatedTo` → `DataContext` set)?

**For state/data bugs:**
- Is the MVVM data flow correct? (View → ViewModel → UseCase → Repository)
- Are `[ObservableProperty]` properties notifying correctly? (partial class for source generators?)
- Is there an event handler leak? (`-=` unsubscription, `IDisposable` implemented?)
- Are `ObservableCollection<T>` updates on the UI thread? (use `Dispatcher` for background updates)
- Is the ViewModel's `DataContext` set correctly?

**For async/await bugs:**
- Is `CancellationToken` propagated through the call chain?
- Are UI updates after `await` running on the UI thread? (may need `.ConfigureAwait(false)` or explicit Dispatcher)
- Is `async void` used only for event handlers? (use `async Task` everywhere else)
- Are exceptions from `Task` being swallowed? (missing `await` or `.GetAwaiter().GetResult()`)

**For memory bugs:**
- Are event handlers unsubscribed? (`-=` in `IDisposable.Dispose`)
- Are `WeakReference` / `WeakEventManager` used where appropriate?
- Is there a circular DI dependency? (check `AddTransient` vs. `AddSingleton` lifetimes)
- Are large collections cleared on close/navigation?

**For AppDesignSystem bugs:**
- Is the component from AppDesignSystem? (`AppLabel`, `AppButton`, not raw `TextBlock`, `Button`)
- Is the `Style` applied correctly? (`Style="{StaticResource AppTypography.HeaderSection}"` with correct ResourceDictionary merge)
- Are colors using theme tokens? (not `Brushes.Red` directly, reference `ResourceDictionary` entry)
- Is  XAML layout using `Grid`/`StackPanel`? (not code-behind positioning)

**For WPF module bugs:**
- Are dependencies injected via constructor? (not `ServiceLocator` anti-pattern)
- Is the module's DI registration correct? (`AddTransient`, `AddScoped`, `AddSingleton` as appropriate)
- Is the feature following MVVM structure? (separate Views/ViewModels/Domain/Data layers)

### Step 3: Apply Minimal Fix

Only fix the root cause. Don't refactor surrounding code.

**Good fixes:**
- Add `partial` keyword to ViewModel class for `[ObservableProperty]` source generators
- Fix `Dispatcher.InvokeAsync` for UI thread update
- Change raw `TextBlock` to `AppLabel` with correct style
- Add `IDisposable` and event unsubscription
- Fix XAML `Binding` path to match property name

**Avoid:**
- Rewriting the entire ViewModel
- Restructuring directories
- Changing architectural patterns unnecessarily
- "Cleanup" of surrounding code

### Step 4: Verify the Full Path

Trace the fix end-to-end:

1. **User interaction** → View event / command binding
2. **View** → ViewModel `[RelayCommand]` invoked
3. **ViewModel** → UseCase `ExecuteAsync(input)`
4. **UseCase** → Repository call
5. **Repository** → Service / HTTP client
6. **Response** → UseCase output
7. **ViewModel** → `[ObservableProperty]` updated
8. **Binding** → WPF `INotifyPropertyChanged` notifies View
9. **View** → UI re-renders

Each arrow should be verified: is data flowing correctly? Are updates on the UI thread? Are errors handled?

### Step 5: Run Analyzers

```bash
dotnet build --warnaserror
# or run StyleCop / Roslyn analyzer checks
dotnet format --verify-no-changes
```

Fix any violations in the changed code. Don't modify unchanged files.

### Step 6: Verify the Fix

Run the specific scenario that triggers the bug:
- Open the affected View
- Perform the action
- Verify the expected behavior
- Check for crashes, memory leaks, or state issues
- Run relevant unit tests

### Step 7: Summarize Changes

Explain:
- **What was broken:** The root cause
- **Why it was broken:** The mechanism that caused the issue
- **How it's fixed:** The minimal change applied
- **How to verify:** Steps to confirm the fix works

## Common Patterns & Solutions

### Pattern 1: Missing `partial` Keyword for Source Generators

**Symptom:** `[ObservableProperty]` or `[RelayCommand]` not generating, undefined property

**Check:**
```csharp
// Good
public sealed partial class UserViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name = string.Empty;
}

// Bad — source generators won't work
public sealed class UserViewModel : ViewModelBase // Missing partial!
{
    [ObservableProperty]
    private string _name = string.Empty;
}
```

**Fix:** Add `partial` keyword to ViewModel class

### Pattern 2: UI Update on Background Thread

**Symptom:** `InvalidOperationException: The calling thread cannot access this object because a different thread owns it`

**Check:**
```csharp
// Good
Application.Current.Dispatcher.InvokeAsync(() =>
{
    Items.Add(newItem); // ObservableCollection updated on UI thread
});

// Bad
await Task.Run(() =>
{
    Items.Add(newItem); // Wrong thread!
});
```

**Fix:** Wrap `ObservableCollection` mutations in `Dispatcher.InvokeAsync` when called from background threads

### Pattern 3: Binding Not Updating

**Symptom:** Property changes but UI doesn't update

**Check:**
```csharp
// Good — CommunityToolkit.Mvvm generates OnXxxChanged notification
public sealed partial class UserViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty; // generates Title property with INPC
}

// Bad — manual property without notification
public sealed class UserViewModel
{
    public string Title { get; set; } = string.Empty; // No INPC!
}
```

**Fix:** Use `[ObservableProperty]` on a `partial` class, or manually call `OnPropertyChanged(nameof(Title))`

### Pattern 4: AppDesignSystem Component Missing

**Symptom:** Inconsistent styling, hardcoded colors, layout issues

**Check:**
```xml
<!-- Good -->
<local:AppLabel Text="{Binding Title}"
                Style="{StaticResource AppTypography.HeaderSection}" />

<!-- Bad -->
<TextBlock Text="{Binding Title}"
           FontSize="16"
           Foreground="Black" />  <!--Hardcoded styling!-->
```

**Fix:** Replace raw WPF controls with AppDesignSystem equivalents and apply correct `Style` from `ResourceDictionary`

### Pattern 5: Event Handler Memory Leak

**Symptom:** ViewModel not collected by GC, memory grows over time

**Check:**
```csharp
// Good — unsubscribe in Dispose
public sealed class UserViewModel : ViewModelBase, IDisposable
{
    public UserViewModel(IEventBus eventBus)
    {
        eventBus.SomeEvent += OnSomeEvent;
        _eventBus = eventBus;
    }

    public void Dispose()
    {
        _eventBus.SomeEvent -= OnSomeEvent; // Unsubscribe!
    }
}

// Bad
public sealed class UserViewModel : ViewModelBase
{
    public UserViewModel(IEventBus eventBus)
    {
        eventBus.SomeEvent += OnSomeEvent; // Leak — never unsubscribed
    }
}
```

**Fix:** Implement `IDisposable` and unsubscribe all event handlers

### Pattern 6: async void Outside Event Handlers

**Symptom:** Exceptions from async operations silently swallowed

**Check:**
```csharp
// Good
[RelayCommand]
private async Task LoadAsync(CancellationToken ct)
{
    await _repository.GetItemsAsync(ct);
}

// Bad
private async void Load() // async void outside event handler!
{
    await _repository.GetItemsAsync(); // Exception swallowed!
}
```

**Fix:** Use `async Task` + `[RelayCommand]` so the toolkit wires the command properly and exceptions surface

## Module-Specific Debugging Tips

### Features/ProductList, Features/OrderManagement

- Verify `[ObservableProperty]` is on `partial` ViewModel class
- Check `ObservableCollection<T>` is only modified on UI thread
- Verify DI registration in `ServiceCollectionExtensions`

### Features/Authentication

- Check token storage (use `ProtectedData` or OS credential store — never plain text)
- Verify redirect after login (`INavigationService.NavigateTo`)
- Check session invalidation on logout

## Debugging Checklist

Before marking a fix complete:

- [ ] Read only 3-4 relevant files
- [ ] Root cause stated clearly (one sentence)
- [ ] Fix is minimal (only the root cause addressed)
- [ ] Full path traced (trigger → logic → UI)
- [ ] Roslyn analyzers passes on changed files
- [ ] Fix verified in app
- [ ] Unit tests pass (if applicable)
- [ ] No retain cycles introduced
- [ ] No new threading issues
- [ ] AppDesignSystem components used (not WPF)
- [ ] XAML layout layout used (not XAML code-behind layout)
