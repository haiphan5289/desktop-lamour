---
description: "Add a UseCase execution method to an existing ViewModel in Desktop Lamour."
mode: "agent"
---

# Handle UseCase in ViewModel — Desktop Lamour

## Input

```
VIEWMODEL:   <[Name]ViewModel>
USECASE:     <I[Feature]UseCase>
INPUT_TYPE:  <[Feature]Input>
OUTPUT_TYPE: <[Feature]Output>
ON_SUCCESS:  <update ObservableCollection | navigate | show message>
```

## Template

```csharp
[RelayCommand]
private async Task [Execute][Feature]Async([InputType] input, CancellationToken ct = default)
{
    IsLoading = true;
    ErrorMessage = string.Empty;
    try
    {
        var result = await _[feature]UseCase.ExecuteAsync(input, ct);
        // TODO: update state e.g. Items = new ObservableCollection<T>(result)
    }
    catch (ValidationException ex)
    {
        ErrorMessage = ex.Message;
    }
    catch (Exception)
    {
        ErrorMessage = "An unexpected error occurred. Please try again.";
    }
    finally
    {
        IsLoading = false;
    }
}
```

## Rules

- IsLoading = true at start, IsLoading = false in finally — always
- ErrorMessage = string.Empty at start to clear previous errors
- Catch ValidationException separately for user-friendly messages
- Never expose raw exception messages to UI
- CancellationToken must be passed through to UseCase
