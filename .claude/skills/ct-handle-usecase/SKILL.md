---
name: ct-handle-usecase
description: Add a UseCase execution method to an existing ViewModel following MVVM + Clean Architecture. Generates an async command method with success, loading (IsLoading), and error handling. Use when wiring a new IUseCase into an existing ViewModel with CommunityToolkit.Mvvm.
---

# ViewModel UseCase Execution Guide

Add a UseCase execution method to an existing ViewModel, following the MVVM + Clean Architecture pattern.

## Input Format

```
USECASE_NAME: <UseCaseName, e.g. "FetchUserProfile">
INPUT_TYPE: <Input type, e.g. "string" or "UserRequest">
OUTPUT_TYPE: <Output type, e.g. "UserModel" or "IReadOnlyList<OrderModel>">
VIEWMODEL_CLASS: <ViewModel class name, e.g. "CheckoutViewModel">
REPO_PROPERTY_NAME: <Repository property in the ViewModel, e.g. "_checkoutRepository">
```

## Generated Method Template

```csharp
// Add to existing partial ViewModel class
public sealed partial class [ViewModelClass] : ViewModelBase
{
    private readonly I[UseCaseName]UseCase _[useCaseName]UseCase;

    // Inject via constructor
    public [ViewModelClass](I[UseCaseName]UseCase [useCaseName]UseCase /*, other deps */)
    {
        _[useCaseName]UseCase = [useCaseName]UseCase;
    }

    [RelayCommand]
    private async Task Execute[UseCaseName]Async([InputType] input, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _[useCaseName]UseCase.ExecuteAsync(input, cancellationToken);
            // TODO: handle success
            // Items = new ObservableCollection<ItemViewModel>(result.Select(ItemViewModel.From));
        }
        catch (OperationCanceledException)
        {
            // Cancelled — no user-facing error needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute [UseCaseName]");
            ErrorMessage = "An error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

## UseCase Interface

```csharp
// Domain/UseCases/I[UseCaseName]UseCase.cs
public interface I[UseCaseName]UseCase
{
    Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken cancellationToken = default);
}

// Domain/UseCases/[UseCaseName]UseCase.cs
public sealed class [UseCaseName]UseCase : I[UseCaseName]UseCase
{
    private readonly I[Name]Repository _repository;

    public [UseCaseName]UseCase(I[Name]Repository repository)
        => _repository = repository;

    public async Task<[OutputType]> ExecuteAsync([InputType] input, CancellationToken cancellationToken = default)
        => await _repository.Get[Entity]Async(input, cancellationToken);
}
```

## ViewModel Properties Required

```csharp
public sealed partial class [ViewModelClass] : ViewModelBase
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ObservableCollection<[ItemViewModel]> _items = new();
}
```

## Critical Rules

### ❌ NEVER DO THESE:

```csharp
// ❌ WRONG: async void outside event handlers
private async void Execute[UseCaseName]() { }  // Exceptions are swallowed!

// ❌ WRONG: .Result or .GetAwaiter().GetResult() on async methods
var result = _useCase.ExecuteAsync(input).Result;  // Deadlock risk!

// ❌ WRONG: UI updates on non-UI thread without Dispatcher
await Task.Run(() => Items.Add(item));  // CrossThreadException!
```

### ✅ CORRECT PATTERNS:

```csharp
// ✅ Correct: [RelayCommand] + async Task — toolkit wires CancellationToken automatically
[RelayCommand]
private async Task Execute[UseCaseName]Async([InputType] input, CancellationToken cancellationToken)
{
    IsLoading = true;
    try
    {
        var result = await _useCase.ExecuteAsync(input, cancellationToken);
        Items = new ObservableCollection<ItemViewModel>(result.Select(ItemViewModel.From));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "...");
        ErrorMessage = "User-friendly message";
    }
    finally { IsLoading = false; }
}

// ✅ Correct: ObservableCollection from background thread via Dispatcher
await Application.Current.Dispatcher.InvokeAsync(() =>
{
    Items.Add(newItem);
});
```

## DI Registration

```csharp
// In ServiceCollectionExtensions or Program.cs
services.AddTransient<I[UseCaseName]UseCase, [UseCaseName]UseCase>();
services.AddTransient<[ViewModelClass]>();
```

## Architecture Notes

- UseCase is injected through the ViewModel constructor (not created inline)
- Use `[RelayCommand]` attribute — CommunityToolkit.Mvvm auto-generates the `ICommand`
- `[RelayCommand]` on `async Task` methods automatically supports `CancellationToken`
- `IsLoading` and `ErrorMessage` are `[ObservableProperty]` on the ViewModel
- ViewModel class must be `partial` for source generators to work
- UseCase naming convention: `{UseCaseName}UseCase`, interface: `I{UseCaseName}UseCase`
