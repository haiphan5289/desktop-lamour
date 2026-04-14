---
name: ct-chotot-module-context
description: "Quick reference for WPF application module architecture, MVVM patterns, AppDesignSystem usage, and DI setup. Use when working on any module—understanding directory structure, DI configuration, interface patterns ([Feature]ViewModel, IView), UseCase/Repository patterns, or module-specific conventions. Provides quick patterns, file paths, localization usage, and logging guidance."
model: sonnet
effort: medium
argument-hint: "[module name or architecture pattern]"
---

# WPF Application Module Context

This skill provides quick reference for patterns and architecture you use frequently.

## How to Use This Skill

**With arguments:**
```
/ct-chotot-module-context Features/InsertAd
/ct-chotot-module-context MVVM architecture
/ct-chotot-module-context UseCase pattern
```

**Supported argument patterns:**
- **Module names**: `Features/InsertAd`, `Features/Orders`, `Features/Chat`, `Features/Feed`, any module
- **Architecture patterns**: `MVVM`, `NavigationService`, `UseCase`, `Repository`, `async/await`
- **Specific components**: `ViewModel`, `View`, `IView`, `IUseCase`
- **File context**: When selected text is provided via `{selectedText}`, reviews code patterns directly

---

**Your focus:** $ARGUMENTS

Provide quick reference and guidance specifically for: **$ARGUMENTS**

## MVVM Architecture

**2-Interface Pattern** (per module):

1. **`[Feature]ViewModel`** — ViewModel (inherits `ViewModelBase` from CommunityToolkit.Mvvm)
2. **`IView`** — View interface exposing bindable state and navigation triggers

**Data Flow:**
```
View triggers → ViewModel ([RelayCommand] / ICommand invoked)
             → UseCase (IUseCase<TIn, TOut>, await ExecuteAsync)
             → Repository → Service → HttpClient
             ← Task<TOut> result
             ← [ObservableProperty] updated
             ← WPF binding notifies View via INotifyPropertyChanged
```

**Navigation Pattern:**
- Navigation is handled by `INavigationService`, not Views
- Views don't reference other Views directly
- `INavigationService` is injected into ViewModel

## Module Structure (Example: Features/InsertAd)

```
Features/InsertAd/
├── Views/                 ← UserControls (.xaml + .xaml.cs)
├── ViewModels/            ← ViewModelBase subclasses
├── Domain/
│   ├── UseCases/
│   └── Models/
├── Data/
│   ├── Repositories/
│   └── Services/
└── Tests/
```

Common modules you work on:
- **CreateItem** — Item creation flow
- **ReviewItem** — Item review/publish
- **Categories** — Category selection
- **Location** — Location picker

## AppDesignSystem Component Usage

**ALWAYS use App components**, never raw WPF controls:
- `AppLabel` instead of `TextBlock`
- `AppButton` instead of `Button`
- `AppTextField` instead of `TextBox`
- `AppImage` instead of `Image`

**Styling Example:**
```xml
<local:AppLabel Text="{Binding Title}"
                Style="{StaticResource AppTypography.HeaderSection}"/>
```

**Layout: XAML ONLY** — no code-behind positioning, no manual `Canvas` coordinates.

## CommunityToolkit.Mvvm Patterns

**ViewModel with [ObservableProperty] and [RelayCommand]:**
```csharp
public sealed partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductRepository _repository;

    [ObservableProperty]
    private ObservableCollection<ProductItemViewModel> _items = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ProductListViewModel(IProductRepository repository)
        => _repository = repository;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _repository.GetProductsAsync(ct);
            Items = new ObservableCollection<ProductItemViewModel>(
                result.Select(ProductItemViewModel.From));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not load products.";
            // ILogger injected separately
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

## DI Registration

```csharp
// ServiceCollectionExtensions.cs
public static IServiceCollection AddProductListModule(this IServiceCollection services)
{
    services.AddTransient<ProductListViewModel>();
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddHttpClient<IProductService, ProductService>(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com");
    });
    return services;
}
```

## Common File Paths (Features/InsertAd)

- **Models**: `Features/InsertAd/Domain/Models/`
- **UseCases**: `Features/InsertAd/Domain/UseCases/`
- **ViewModels**: `Features/InsertAd/ViewModels/`
- **Services**: `Features/InsertAd/Data/Services/`
- **Tests**: `Features/InsertAd/Tests/`

## Localization Pattern

**Use `.resx` resource files:**
```csharp
// Properties/Resources.resx
var label = Properties.Resources.ProductList_Title;
```

```xml
<!-- XAML -->
<local:AppLabel Text="{x:Static properties:Resources.ProductList_Title}"/>
```

## Logging

Use `ILogger<T>` from `Microsoft.Extensions.Logging` — inject via constructor.
```csharp
_logger.LogInformation("Products loaded: {Count}", items.Count);
_logger.LogError(ex, "Failed to load products");
```

## File Headers

```csharp
// ProductListViewModel.cs
// Created by [Name] on [Date].
// Copyright © 2024 App. All rights reserved.
```

Use `time` MCP for the date, git config for your name.

## String Substitutions Supported

The skill can automatically detect and use:
- `{selectedText}` — Code snippet for pattern analysis and architectural review
- `{fileName}` — Current file name for module context detection
- `{filePath}` — Full path for accurate module identification

## When You're Stuck

- Check project memory at `~/.claude/projects/[project-name]/memory/`
- Refer to `AGENTS.md` for module-specific rules
- Look at examples in `Shared/AppDesignSystem/SampleApp`
- Use ruler files in `.ruler/` for detailed guidance on architecture, code style, testing
