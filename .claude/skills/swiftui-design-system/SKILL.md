---
name: swiftui-design-system
description: "AppDesignSystem tokens and components for WPF XAML. Use for colors, typography, spacing, buttons, inputs, cards — always use AppDesignSystem ResourceDictionary tokens, never raw values."
argument-hint: "[component or token type]"
---

# AppDesignSystem Skill

Complete reference for AppDesignSystem ResourceDictionary tokens and WPF components.

---

## Setup

- Merge `AppDesignSystem.xaml` in `App.xaml` (REQUIRED for all styles/components)
- Add `xmlns:local="clr-namespace:App.Shared.Controls"` in every XAML file
- Ensure `AppConverters.xaml` is merged for visibility/null converters

## Themes

`Default` (Light) | `Dark` | custom themes via `AppThemeManager.ApplyTheme(name)`

## Component Quick Reference

| Need | Use |
|------|-----|
| Text display | `<AppLabel Style="{StaticResource AppTypography.*}"/>` |
| Button | `<AppButton Style="{StaticResource AppButton.Primary.Medium}"/>` |
| Text input | `<AppTextField Text="{Binding Prop, Mode=TwoWay}"/>` |
| Image | `<AppImage Source="{Binding ImageUrl}"/>` |
| Loading indicator | `<ProgressBar IsIndeterminate="True" Visibility="{Binding IsLoading, Converter=...}"/>` |
| Empty state | `<AppEmptyState Message="{Binding EmptyMessage}"/>` |
| List | `<ListView ItemsSource="{Binding Items}" VirtualizingPanel.IsVirtualizing="True"/>` |

## Typography Tokens

```xml
<!-- Apply via Style attribute on AppLabel -->
AppTypography.DisplayPage       <!-- 32sp Bold -->
AppTypography.HeaderPage        <!-- 20sp SemiBold -->
AppTypography.HeaderSection     <!-- 16sp SemiBold -->
AppTypography.LabelPage         <!-- 16sp SemiBold -->
AppTypography.LabelSection      <!-- 14sp SemiBold -->
AppTypography.BodySection       <!-- 14sp Regular -->
AppTypography.BodyCaption       <!-- 12sp Regular -->
AppTypography.NoteSection       <!-- 12sp Regular Italic -->
```

## Color Tokens

```xml
<!-- Text -->
Foreground="{StaticResource AppColor.TextPrimary}"
Foreground="{StaticResource AppColor.TextSecondary}"
Foreground="{StaticResource AppColor.TextDisabled}"
Foreground="{StaticResource AppColor.TextError}"
Foreground="{StaticResource AppColor.TextBrand}"
Foreground="{StaticResource AppColor.TextInverted}"

<!-- Backgrounds -->
Background="{StaticResource AppColor.BackgroundPrimary}"
Background="{StaticResource AppColor.BackgroundSecondary}"
Background="{StaticResource AppColor.BackgroundBrand}"
Background="{StaticResource AppColor.BackgroundWarningLight}"
Background="{StaticResource AppColor.BackgroundErrorLight}"

<!-- Borders -->
BorderBrush="{StaticResource AppColor.BorderThin}"
BorderBrush="{StaticResource AppColor.BorderRegular}"
BorderBrush="{StaticResource AppColor.BorderBrand}"
```

## Spacing Tokens

```xml
<!-- Use as Margin/Padding values via StaticResource -->
Margin="{StaticResource AppSpacing.XSmall}"    <!-- 4 -->
Margin="{StaticResource AppSpacing.Small}"     <!-- 8 -->
Margin="{StaticResource AppSpacing.Medium}"    <!-- 12 -->
Margin="{StaticResource AppSpacing.Large}"     <!-- 16 -->
Margin="{StaticResource AppSpacing.XLarge}"    <!-- 20 -->
Margin="{StaticResource AppSpacing.XXLarge}"   <!-- 24 -->
```

## Button Styles

```xml
<!-- Primary buttons -->
AppButton.Primary.Small
AppButton.Primary.Medium
AppButton.Primary.Large

<!-- Secondary buttons -->
AppButton.Secondary.Small
AppButton.Secondary.Medium
AppButton.Secondary.Large

<!-- Tertiary (ghost) buttons -->
AppButton.Tertiary.Medium

<!-- Destructive -->
AppButton.Destructive.Medium

<!-- Icon-only -->
AppButton.Icon.Small
AppButton.Icon.Medium
```

## Forbidden (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `Brushes.Blue`, `Colors.Red` | `{StaticResource AppColor.TextBrand}` |
| `Foreground="#FF333333"` | `Foreground="{StaticResource AppColor.TextPrimary}"` |
| `Margin="16"` hardcoded | `Margin="{StaticResource AppSpacing.Large}"` |
| `FontSize="14"` | `Style="{StaticResource AppTypography.BodySection}"` |
| `<TextBlock/>`, `<Button/>` | `<AppLabel/>`, `<AppButton/>` |

## Gotchas

- `AppColor.BorderThin` is a `SolidColorBrush` — use `BorderBrush=`, not `Background=`
- `BoolToVisibilityConverter` is in `AppConverters.xaml` — merge it before use
- `AppButton` `Command` binding auto-disables when `CanExecute` returns false — no manual `IsEnabled` needed
- ResourceDictionary keys are **case-sensitive**: `AppColor.TextPrimary` ≠ `AppColor.textPrimary`
- Dark mode: merge `DarkTheme.xaml` instead of `DefaultTheme.xaml` — resource keys stay the same

## ResourceDictionary Merge Order

```xml
<!-- App.xaml — correct merge order -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/App;component/Themes/DefaultTheme.xaml"/>   <!-- 1. Theme colors -->
            <ResourceDictionary Source="/App;component/Themes/AppTypography.xaml"/>  <!-- 2. Typography -->
            <ResourceDictionary Source="/App;component/Shared/AppConverters.xaml"/>  <!-- 3. Converters -->
            <ResourceDictionary Source="/App;component/Shared/AppStyles.xaml"/>      <!-- 4. Component styles -->
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Complete reference for AppDesignSystem tokens and components (v3.0+).

**Last synced:** 2026-01-26

---

## Setup

- `DS.registerFonts()` in App init (REQUIRED for custom fonts)
- `import AppDesignSystemSwiftUI` in every SwiftUI file
- `@Environment(\.colorTheme) var theme` in every View
- `.environment(\.colorTheme, .pty)` at app root

## Themes

`.chotot` (Yellow) | `.job` (Blue) | `.pty` (Orange-red) | `.veh` (Yellow)

## Read Guide

| Task | File |
|------|------|
| Color tokens, theme colors | [references/colors.md](./references/colors.md) |
| Text styles, fonts | [references/typography.md](./references/typography.md) |
| Spacing, padding, radius, borders | [references/spacing.md](./references/spacing.md) |
| UI components (buttons, inputs...) | [references/components.md](./references/components.md) |
| Hex→token conversion (Figma) | [references/color-mapping.yaml](./references/color-mapping.yaml) |
| SwiftUI code review (Few-Shot) | [../review-code/references/review-code-swiftUI.md](../review-code/references/review-code-swiftUI.md) |

## Forbidden (ALWAYS APPLY)

| ❌ Forbidden | ✅ Required |
|-------------|-------------|
| `Color.blue` | `theme.text.textBrand` |
| `Color(hex: "...")` | `theme.*.*` |
| `.padding(16)` | `.padding(DS.Padding.paddingMedium)` |
| `Font.system(size:)` | `.cdsTextStyle(...)` |
| `theme.textPrimary` | `theme.text.textPrimary` |

## Gotchas

- `DS.StrokeLine.strokeDivider` (struct member) vs `.strokeDivide` (CGFloat extension) — different names!
- `DS.BorderRadius.radiusCard.value()` — needs `.value()` call for CGFloat
- Color sub-protocol access: `theme.text.textPrimary` NOT `theme.textPrimary`
- Dark mode NOT available yet. All themes light mode only.

## Source

> ⚠️ Path contains a DerivedData build hash — if DerivedData is cleared, regenerate via `dotnet restore` or build once.

**Package root:** `/Users/hai.phan/Library/Developer/Xcode/DerivedData/ChoTot-emrqzdagaqgtgleygywbvfeauazo/SourcePackages/checkouts/ct-ios-design-system-swiftui`

| Resource | Path |
|----------|------|
| README (overview + component status) | `…/README.md` |
| API Reference (authoritative component list) | `…/docs/API_REFERENCE.md` |
| Component Examples (copy-paste) | `…/docs/COMPONENT_EXAMPLES.md` |
| Best Practices | `…/docs/BEST_PRACTICES.md` |
| Integration Guide | `…/docs/INTEGRATION_GUIDE.md` |
| Troubleshooting | `…/docs/TROUBLESHOOTING.md` |
| Source code | `…/Sources/AppDesignSystemSwiftUI/Sources` |
| Demo App | `…/AppDesignSystemSwiftUIApp/` |
