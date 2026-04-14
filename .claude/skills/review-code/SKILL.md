---
name: review-code
description: "WPF C# code review — AppDesignSystem compliance, MVVM patterns, state management, color/typography ResourceDictionary tokens, and Roslyn/StyleCop analyzer rules. Use when asked to review WPF XAML or C# ViewModel/UseCase code."
argument-hint: "[file path or code to review] [focus area: DS Components | Color Tokens | Typography | State Management | MVVM | Analyzers | Full Review]"
---

# WPF Code Review Skill

Full code review for WPF and C# files in a **.NET desktop application**.

---

## When to Use

Invoke this skill when asked to:
- Review a WPF XAML or C# file
- Check AppDesignSystem compliance (components, styles, ResourceDictionary tokens)
- Audit MVVM architecture in WPF
- Verify Roslyn analyzer / StyleCop compliance
- Check async/await patterns and threading safety

---

## Focus Areas

| Area | What Is Checked |
|------|----------------|
| `DS Components` | `AppLabel`, `AppButton`, `AppTextField` vs raw WPF primitives (`TextBlock`, `Button`) |
| `Color Tokens` | `{StaticResource AppColor.*}` — no `Brushes.*` or hardcoded hex `#RRGGBB` |
| `Typography` | `Style="{StaticResource AppTypography.*}"` — no `FontSize="16"` hardcoded |
| `State Management` | `[ObservableProperty]`, `[RelayCommand]`, `partial` class, `ObservableCollection` correctness |
| `MVVM` | No business logic in XAML code-behind, proper ViewModel separation, `DataContext` set via DI |
| `Async / Threading` | `async Task` (not `async void`), `CancellationToken` propagated, UI updates on Dispatcher |
| `Memory Management` | Event handler unsubscription, `IDisposable` implemented, no GC rooting leaks |
| `Analyzers` | Roslyn / StyleCop.Analyzers rules, nullable reference types enabled, no `#nullable disable` |
| `Full Review` | All of the above combined |

---

## Key Rules (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `Brushes.Black`, `Colors.Red` | `{StaticResource AppColor.TextPrimary}` |
| `FontSize="16" FontWeight="SemiBold"` | `Style="{StaticResource AppTypography.HeaderSection}"` |
| Raw `<TextBlock/>`, `<Button/>` | `<AppLabel/>`, `<AppButton/>` |
| `async void Method()` (outside event handlers) | `[RelayCommand] async Task MethodAsync()` |
| `.Result`, `.GetAwaiter().GetResult()` | `await` |
| Manual `XAML code-behind layout`-equivalent code layout | XAML `Grid`/`StackPanel` layout |
| `public class ViewModel` (without `partial`) | `public sealed partial class ViewModel : ViewModelBase` |
| `Console.WriteLine(...)` | `_logger.LogInformation(...)` / `_logger.LogError(...)` |
| Force-cast `(Type)obj` without null check | `obj as Type` + null check / pattern matching |
| Event subscription without unsubscription | Implement `IDisposable`, unsubscribe in `Dispose()` |

## Anti-Hallucination Rule

> **NEVER suggest an `App*` component unless it exists in the AppDesignSystem ResourceDictionary.**
> If unsure — use raw WPF with AppDesignSystem styles instead.

**Verified components:** `AppLabel`, `AppButton`, `AppTextField`, `AppPasswordField`, `AppImage`, `AppProgressRing`, `AppEmptyState`, `AppSnackBar`, `AppCard`, `AppChip`, `AppBadge`, `AppAvatar`, `AppDialog`

**Do NOT exist in AppDesignSystem:** `AppTextBlock`, `AppStack`, `AppText`, `AppDivider`, `AppList`

---

## Analyzer Source

**File:** Project `.editorconfig` or `StyleCop.Analyzers` NuGet config
- `CS8600`, `CS8603` — nullable reference warnings → **error** (CI fails with `<Nullable>enable</Nullable>`)
- `CA1822` — Mark members as static → warning
- See full rules in project `.editorconfig`

---

## MVVM Pattern Checklist

```csharp
// ✅ Correct ViewModel
public sealed partial class ProductViewModel : ViewModelBase   // sealed + partial
{
    [ObservableProperty]
    private string _title = string.Empty;    // generates Title property with INPC

    [ObservableProperty]
    private bool _isLoading;                 // generates IsLoading

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)  // auto-generates LoadCommand
    {
        IsLoading = true;
        try { /* ... */ }
        finally { IsLoading = false; }
    }
}

// ❌ Wrong ViewModel
public class ProductViewModel  // Missing: sealed, partial
{
    public string Title { get; set; }  // No INPC notification!
    public async void Load() { }      // async void — exceptions swallowed!
}
```

Full code review for SwiftUI files in the **Chợ Tốt iOS** app.

**Last synced:** 2026-03-25

---

## When to Use

Invoke this skill when asked to:
- Review a SwiftUI file or directory
- Check CT Design System compliance
- Audit MVVM architecture in SwiftUI
- Verify Roslyn analyzers compliance

---

## Read Guide

| Task | File |
|------|------|
| Full review template, Few-Shot examples, Roslyn analyzers rules | [references/review-code-swiftUI.md](./references/review-code-swiftUI.md) |

> Always read `references/review-code-swiftUI.md` before performing any review.

---

## Focus Areas

| Area | What Is Checked |
|------|----------------|
| `DS Components` | `.cdsButtonStyle()`, `CAppTextField`, `.cdsTextStyle()` vs raw SwiftUI |
| `Color Tokens` | `theme.*.*` sub-protocol access, no raw `Color.*` |
| `Typography` | `.cdsTextStyle()`, no `Font.system()` |
| `Spacing Tokens` | `DS.Gap.*`, `DS.Padding.*`, `DS.BorderRadius.*`, no hardcoded values |
| `State Management` | `@State`, `@StateObject`, `@ObservedObject`, `@EnvironmentObject` correctness |
| `MVVM` | No business logic in View body, proper ViewModel separation |
| `Memory Management` | `[weak self]`, retain cycles in Combine/closures |
| `Roslyn analyzers All` | All rules from `.cslint.yml` |
| `Full Review` | All of the above combined |

---

## Key Rules (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `Color.blue` | `theme.text.textBrand` |
| `Color(hex: "...")` | `theme.*.*` |
| `.padding(16)` | `.padding(DS.Padding.paddingMedium)` |
| `Font.system(size:)` | `.cdsTextStyle(...)` |
| `theme.textPrimary` | `theme.text.textPrimary` (sub-protocol) |
| ViewModel created in `body` | `@StateObject` with `init(flow:)` |
| `@Environment(\.presentationMode)` | `@Environment(\.dismiss)` |
| `as!`, `try!`, `!` | `as?` + guard, do/catch, guard let |
| `Button(action: {}) { }` | `Button(action: {}, label: { })` |

## Anti-Hallucination Rule

> **NEVER suggest a `CDS*` component unless it is verified in [references/review-code-swiftUI.md](./references/review-code-swiftUI.md).**
> If unsure — use raw SwiftUI with CT tokens instead.

**Verified components:** `CAppTextField`, `CDSCard`, `CDSBottomSheet`, `CDSBadge`, `CDSChip`, `CDSTag`, `CDSAvatar`, `CDSAsyncImage`, `CDSPopupView`, `CDSEmptyState`, `CDSSkeleton`, `CDSToast`, `CDSSnackBarView`

**Do NOT exist:** `CDSDivider`, `CAppLabel`, `CDSText`, `CDSImage`, `CDSStack`, `CAppButton`

---

## Roslyn analyzers Source

**File:** `/Users/hai.phan/Desktop/haiphan/ct-ios-app--v3/.cslint.yml`
- `force_cast`, `force_try` → **error** (CI fails)
- `opening_brace`, `multiple_closures_with_trailing_closure` → default rules (always active)
- See full rules in [references/review-code-swiftUI.md](./references/review-code-swiftUI.md)
