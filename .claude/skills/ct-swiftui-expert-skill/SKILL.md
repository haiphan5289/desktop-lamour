---
name: ct-swiftui-expert-skill
description: "Expert guidance for WPF XAML development — building views with AppDesignSystem components, typography, MVVM + CommunityToolkit.Mvvm patterns, and design system compliance. Use when implementing WPF features, creating custom UserControls, optimizing performance, managing state with [ObservableProperty]/[RelayCommand], handling async operations, validating design system adherence (AppButton, AppLabel, AppTextField), or debugging data binding issues. Requires understanding of MVVM architecture, ICommand patterns, and AppDesignSystem ResourceDictionary tokens."
model: sonnet
effort: medium
argument-hint: "[component or pattern type]"
---

# WPF XAML Expert Skill

This skill is the definitive guide for WPF XAML + MVVM development using AppDesignSystem and CommunityToolkit.Mvvm.

## How to Use This Skill

**With arguments:**
```
/ct-swiftui-expert-skill AppButton
/ct-swiftui-expert-skill state management in WPF views
/ct-swiftui-expert-skill create a custom UserControl
/ct-swiftui-expert-skill data binding issues
```

**Supported argument patterns:**
- **Component name**: `AppButton`, `AppTextField`, `AppLabel`, etc.
- **Typography tokens**: `AppTypography.HeaderSection`, `AppTypography.BodyCaption`
- **Feature/issue**: `state management`, `compose views`, `handle errors`, `optimize performance`, `data binding`
- **File context**: When selected text is provided, the skill analyzes the code directly

---

**Your question:** $ARGUMENTS

Provide expert guidance specifically for: **$ARGUMENTS** in the context of WPF XAML development.

## Core Mandates

1. **AppDesignSystem Components Only**: Never use raw `TextBlock`, `Button`, or `TextBox` — always use `AppLabel`, `AppButton`, `AppTextField`.
2. **Semantic Typography**: Use `Style="{StaticResource AppTypography.BodySection}"` instead of `FontSize="14"`.
3. **Standardized Visual Feedback**: Use `AppProgressRing` for loading, `AppSnackBar` for notifications — not custom solutions.
4. **MVVM + CommunityToolkit.Mvvm**: View state MUST be managed by a `ViewModelBase` subclass with `[ObservableProperty]`.

## Key Component Mappings

| Feature | AppDesignSystem Usage | Notes |
| :--- | :--- | :--- |
| **Typography** | `<AppLabel Style="{StaticResource AppTypography.HeaderPage}"/>` | See Typography Reference below |
| **Buttons** | `<AppButton Style="{StaticResource AppButton.Primary.Medium}"/>` | Bind `Command="{Binding LoadCommand}"` |
| **Input** | `<AppTextField Text="{Binding SearchText, Mode=TwoWay}"/>` | `AppPasswordField` for passwords |
| **Loading** | `<AppProgressRing Visibility="{Binding IsLoading, Converter=...}"/>` | `IsIndeterminate="True"` |
| **Empty State** | `<AppEmptyState Message="{Binding EmptyMessage}"/>` | Show when `Items.Count == 0` |
| **List** | `<ListView ItemsSource="{Binding Items}" ItemTemplate="..."/>` | Enable `VirtualizingPanel` for large lists |

## Typography System

AppDesignSystem uses a semantic typography system via `ResourceDictionary` styles.

```xml
<!-- Common styles -->
<AppLabel Style="{StaticResource AppTypography.DisplayPage}"/>      <!-- 32 Bold -->
<AppLabel Style="{StaticResource AppTypography.HeaderSection}"/>    <!-- 16 SemiBold -->
<AppLabel Style="{StaticResource AppTypography.LabelPage}"/>        <!-- 16 SemiBold -->
<AppLabel Style="{StaticResource AppTypography.BodySection}"/>      <!-- 14 Regular -->
<AppLabel Style="{StaticResource AppTypography.BodyCaption}"/>      <!-- 12 Regular -->
<AppLabel Style="{StaticResource AppTypography.NoteSection}"/>      <!-- 12 Regular Italic -->
```

## ViewModel Pattern (CommunityToolkit.Mvvm)

Always use `[ObservableProperty]` and `[RelayCommand]` for a clean, unidirectional data flow.

```csharp
public sealed partial class ProductListViewModel : ViewModelBase
{
    private readonly IFetchProductsUseCase _useCase;

    [ObservableProperty]
    private ObservableCollection<ProductItemViewModel> _items = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ProductListViewModel(IFetchProductsUseCase useCase) => _useCase = useCase;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _useCase.ExecuteAsync(cancellationToken);
            Items = new ObservableCollection<ProductItemViewModel>(result.Select(ProductItemViewModel.From));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load. Please try again.";
        }
        finally { IsLoading = false; }
    }
}
```

## XAML Data Binding Best Practices

```xml
<!-- One-way binding (ViewModel → View) -->
<AppLabel Text="{Binding Title}"/>

<!-- Two-way binding (View ↔ ViewModel) -->
<AppTextField Text="{Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

<!-- Command binding -->
<AppButton Content="Load" Command="{Binding LoadCommand}"/>

<!-- Visibility binding with converter -->
<ProgressBar Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>

<!-- Conditional display -->
<AppLabel Text="{Binding ErrorMessage}"
          Visibility="{Binding ErrorMessage, Converter={StaticResource NullToCollapsedConverter}}"/>
```

## Large List Performance

```xml
<!-- Enable virtualization for lists with many items -->
<ListView ItemsSource="{Binding Items}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          ScrollViewer.CanContentScroll="True">
    <ListView.ItemTemplate>
        <DataTemplate>
            <!-- item content -->
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

## String Substitutions Supported

The skill can automatically detect and use:
- `{selectedText}` — Code snippet for review or refactoring
- `{fileName}` — Current file name for context-aware suggestions
- `{filePath}` — Full file path for architecture recommendations

## Resources

- AppDesignSystem ResourceDictionary: `/App;component/Shared/AppDesignSystem.xaml`
- Typography styles: `/App;component/Themes/AppTypography.xaml`
- Color resources: `/App;component/Themes/AppColors.xaml`
- Converters: `/App;component/Shared/AppConverters.xaml`

This skill is the definitive guide for SwiftUI development at ChoTot, grounded in the `AppDesignSystemSwiftUI` core package.

## How to Use This Skill

**With arguments:**
```
/ct-swiftui-expert-skill CAppButton
/ct-swiftui-expert-skill state management in SwiftUI views
/ct-swiftui-expert-skill create a custom component
```

**Supported argument patterns:**
- **Component name**: `CAppButton`, `CAppTextField`, `CDSPopup`, etc.
- **Typography tokens**: `displayPage`, `headerSection`, `bodySection`
- **Feature/issue**: `state management`, `compose views`, `handle errors`, `optimize performance`
- **File context**: When selected text is provided via `{selectedText}`, the skill analyzes the code directly

---

**Your question:** $ARGUMENTS

Provide expert guidance specifically for: **$ARGUMENTS** in the context of SwiftUI development at ChoTot.

## Core Mandates

1.  **CDS Components Only**: Never use native `Button`, `TextField`, or `Text` without CDS styling.
2.  **Semantic Typography**: Use `.cdsTextStyle(.bodySection)` instead of `.font(.system(...))`.
3.  **Standardized Popups**: Use `.cdsPopup()` or `.cdsBottomSheet()` instead of native `alert()` or `sheet()`.
4.  **MVVM-Combine**: View state MUST be managed by a `ViewModel` exposed via `AnyViewModel`.

## Key Component Mappings

| Feature | CDS Usage | Modifiers / Notes |
| :--- | :--- | :--- |
| **Typography** | `Text("...").cdsTextStyle(.headerPage)` | See [Typography Reference](#typography) |
| **Buttons** | `Button("...").cdsButtonStyle(.primary)` | `.cdsButtonLoading(true)` for loading |
| **Input** | `CAppTextField(text: $t, placeholder: "...")` | `CDSTextView` for multiline |
| **Popups** | `.cdsPopup(isPresented: $p, title: "...")` | Standard modal dialogs |
| **Bottom Sheet**| `.cdsBottomSheet(isPresented: $p) { ... }` | Sliding panel from bottom |

## Typography System

ChoTot uses a semantic typography system based on `DS.TypoToken`.

```swift
// Common tokens:
.cdsTextStyle(.displayPage)      // 32 Bold
.cdsTextStyle(.headerSection)    // 16 SemiBold
.cdsTextStyle(.labelPage)       // 16 SemiBold
.cdsTextStyle(.bodySection)      // 14 Regular
.cdsTextStyle(.noteSection)      // 12 Regular Italic
```

## ViewModel Pattern (PassthroughRelay)

Always use `PassthroughRelay` for inputs to ensure a reactive, unidirectional flow.

```swift
// In ViewModel
private let fetchDataStream = PassthroughRelay<Void>()

func trigger(_ input: Input) {
    switch input {
    case .fetchData: fetchDataStream.accept()
    }
}
```

## String Substitutions Supported

The skill can automatically detect and use:
- `{selectedText}` — Code snippet you've selected for review or refactoring
- `{fileName}` — Current file name for context-aware suggestions
- `{filePath}` — Full file path for architecture recommendations

## Resources

- [references/architecture.md](references/architecture.md): MVVM + Combine deep dive.
- [references/cds_components.md](references/cds_components.md): Comprehensive list of CDS SwiftUI components.
- [references/components_api.md](references/components_api.md): Detailed API for BottomSheets, Popups, and Inputs.
