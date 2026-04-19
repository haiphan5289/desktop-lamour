---
name: ct-theme
description: Guide for using AppStyles.xaml and AppTypography.xaml resource dictionaries in Desktop Lamour WPF. Lists available style keys for TextBlock, Button, TextBox, DataGrid, Border/Card. Rules — no hardcoded colors, no inline fonts, always use StaticResource. Use when styling any WPF control.
model: haiku
effort: low
---

# AppStyles & AppTypography Usage Guide for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

All WPF styles in Desktop Lamour are defined in two resource dictionaries:
- `src/DesktopLamour/Themes/AppTypography.xaml` — font styles, text block styles
- `src/DesktopLamour/Shared/AppStyles.xaml` — button, input, card, data grid styles

## Core Rule

**NEVER use inline styles.** All styling must reference `StaticResource`.

| Forbidden | Required |
|---|---|
| `FontSize="16"` | `Style="{StaticResource TextHeadingStyle}"` |
| `FontWeight="Bold"` | Use a typography style key |
| `Background="#FFFFFF"` | `Background="{StaticResource BackgroundPrimaryBrush}"` |
| `Foreground="Red"` | `Foreground="{StaticResource ErrorForegroundBrush}"` |
| `BorderBrush="#E0E0E0"` | `BorderBrush="{StaticResource BorderBrush}"` |

---

## Typography Style Keys (AppTypography.xaml)

Verify these keys exist by reading `src/DesktopLamour/Themes/AppTypography.xaml` before use.

### Text Styles

```xml
<!-- Headings -->
<TextBlock Style="{StaticResource TextHeadingStyle}" Text="Page Title"/>
<TextBlock Style="{StaticResource TextSubheadingStyle}" Text="Section Title"/>

<!-- Body text -->
<TextBlock Style="{StaticResource TextBodyStyle}" Text="Regular content"/>
<TextBlock Style="{StaticResource TextCaptionStyle}" Text="Small label"/>

<!-- Special states -->
<TextBlock Style="{StaticResource TextErrorStyle}" Text="{Binding ErrorMessage}"/>
<TextBlock Style="{StaticResource TextSuccessStyle}" Text="Saved successfully"/>
<TextBlock Style="{StaticResource TextMutedStyle}" Text="Optional hint"/>
```

### AppLabel Control

Desktop Lamour has a custom `AppLabel` control in `src/DesktopLamour/Shared/Controls/AppLabel.cs`. Use it for text that needs additional behavior.

---

## Button Style Keys (AppStyles.xaml)

Verify these keys exist before use:

```xml
<!-- Primary action (Xác nhận, Lưu) -->
<Button Content="Lưu" Style="{StaticResource ButtonPrimaryStyle}"
        Command="{Binding SaveCommand}"/>

<!-- Secondary action (Huỷ) -->
<Button Content="Huỷ" Style="{StaticResource ButtonSecondaryStyle}"
        Command="{Binding CancelCommand}"/>

<!-- Danger action (Xoá) -->
<Button Content="Xoá" Style="{StaticResource ButtonDangerStyle}"
        Command="{Binding DeleteCommand}"
        CommandParameter="{Binding Id}"/>

<!-- Ghost / text-only action -->
<Button Content="Xem chi tiết" Style="{StaticResource ButtonGhostStyle}"/>
```

---

## Input Style Keys

```xml
<!-- Standard text input -->
<TextBox Style="{StaticResource TextBoxStyle}"
         Text="{Binding SearchQuery, UpdateSourceTrigger=PropertyChanged}"
         Width="200"/>

<!-- ALWAYS use UpdateSourceTrigger=PropertyChanged for two-way bindings -->
<TextBox Style="{StaticResource TextBoxStyle}"
         Text="{Binding EmployeeName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

<!-- Password field -->
<PasswordBox Style="{StaticResource PasswordBoxStyle}"/>
```

---

## DataGrid Style Keys

```xml
<DataGrid Style="{StaticResource DataGridStyle}"
          RowStyle="{StaticResource DataGridRowStyle}"
          ColumnHeaderStyle="{StaticResource DataGridColumnHeaderStyle}"
          ItemsSource="{Binding Employees}"
          AutoGenerateColumns="False"
          IsReadOnly="True">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Họ tên"
                            Binding="{Binding FullName}"
                            Width="*"/>
    </DataGrid.Columns>
</DataGrid>
```

---

## Card / Border Style Keys

```xml
<!-- Card container -->
<Border Style="{StaticResource CardBorderStyle}" Margin="0,4">
    <StackPanel Margin="12,8">
        <TextBlock Text="{Binding Title}" Style="{StaticResource TextSubheadingStyle}"/>
        <TextBlock Text="{Binding Description}" Style="{StaticResource TextBodyStyle}"/>
    </StackPanel>
</Border>

<!-- Section separator -->
<Separator Style="{StaticResource SeparatorStyle}"/>
```

---

## Spacing / Margin Conventions

Use consistent margin values:

```xml
<!-- Page padding -->
<Grid Margin="16">

<!-- Between sections -->
<StackPanel Margin="0,0,0,16">

<!-- Between items in a form -->
<TextBox Margin="0,0,0,8"/>

<!-- Button row spacing -->
<StackPanel Orientation="Horizontal">
    <Button Style="{StaticResource ButtonPrimaryStyle}" Content="Lưu" Margin="0,0,8,0"/>
    <Button Style="{StaticResource ButtonSecondaryStyle}" Content="Huỷ"/>
</StackPanel>
```

---

## Adding New Styles

When adding a new style:

1. Add to `AppStyles.xaml` (non-typography) or `AppTypography.xaml` (text/font related)
2. Use a descriptive `x:Key` with consistent suffix (`Style`, `Brush`)
3. Never add inline styles in individual XAML files
4. Announce the new key in the PR description so other developers can use it

---

## Verifying a Style Key Exists

Before using any `StaticResource` key:

```
Grep: pattern="x:Key=\"ButtonPrimaryStyle\"" path="src/DesktopLamour/Themes"
Grep: pattern="x:Key=\"ButtonPrimaryStyle\"" path="src/DesktopLamour/Shared"
```

If the key is not found, either use an existing similar key or create a new one following the naming convention.
