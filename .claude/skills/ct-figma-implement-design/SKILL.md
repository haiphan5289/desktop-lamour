---
name: ct-figma-implement-design
description: Translate UI wireframes or design specs into production WPF XAML for Desktop Lamour. Analyzes layout, maps to AppStyles style keys, generates UserControl XAML + ViewModel bindings. Use when implementing UI from a design or wireframe description.
model: sonnet
effort: high
---

# UI Design to WPF XAML Implementation

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

## Overview

Translate UI wireframes, screenshots, or design spec descriptions into production-ready WPF XAML UserControls for Desktop Lamour. Maps layout to AppStyles keys and wires bindings to the ViewModel.

## Required Workflow

Follow these steps in order.

### Step 1 — Layout Analysis

When the user provides a design (description, screenshot, or wireframe):

1. Identify the major sections: header, list/table, form, action buttons, footer
2. Identify container type needed: DataGrid | ListBox | ItemsControl | Form (StackPanel/Grid)
3. Identify state elements: loading indicator, error message, empty state
4. Identify user interactions: click/command, text input, selection, search

### Step 2 — Component Mapping

Map design elements to WPF controls + AppStyles keys:

| Design element | WPF control | Style key |
|---|---|---|
| Page title | TextBlock | `TextHeadingStyle` |
| Section title | TextBlock | `TextSubheadingStyle` |
| Body text | TextBlock | `TextBodyStyle` |
| Small label | TextBlock | `TextCaptionStyle` |
| Error text | TextBlock | `TextErrorStyle` |
| Primary button | Button | `ButtonPrimaryStyle` |
| Secondary button | Button | `ButtonSecondaryStyle` |
| Delete button | Button | `ButtonDangerStyle` |
| Text input | TextBox | `TextBoxStyle` |
| Dropdown | ComboBox | `ComboBoxStyle` |
| Data table | DataGrid | `DataGridStyle` |
| Card container | Border | `CardBorderStyle` |

### Step 3 — XAML Generation

Generate `[Name]View.xaml` UserControl with:
- Correct `x:Class` and namespace
- All style keys from AppStyles/AppTypography (no inline styles)
- Binding paths matching ViewModel `[ObservableProperty]` generated names
- Command bindings matching `[RelayCommand]` generated names
- Loading overlay if needed
- Error message binding

### Step 4 — ViewModel State

List the `[ObservableProperty]` fields and `[RelayCommand]` methods needed in the ViewModel:

```
OBSERVABLE PROPERTIES:
- _isLoading (bool)
- _errorMessage (string)
- _searchQuery (string) — if search input exists
- _selectedItem (T?) — if selection exists

OBSERVABLE COLLECTIONS:
- Items (ObservableCollection<T>) — if list exists

RELAY COMMANDS:
- LoadAsync — for initial data load
- SearchAsync — if search exists
- CreateAsync / SaveAsync — if form exists
- DeleteAsync — if delete button exists
```

### Step 5 — Binding Wiring

Verify every `{Binding ...}` path:
- Check `[ObservableProperty]` exists in ViewModel (use PascalCase, not underscore field)
- Check `Command="{Binding XxxCommand}"` — generated name is MethodName + "Command"
- Use `UpdateSourceTrigger=PropertyChanged` on all two-way TextBox bindings

---

## Standard View Template

```xml
<!-- src/DesktopLamour/Features/[Module]/Views/[Name]View.xaml -->
<UserControl x:Class="DesktopLamour.Features.[Module].Views.[Name]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>  <!-- Header row -->
            <RowDefinition Height="Auto"/>  <!-- Search/filter row -->
            <RowDefinition Height="*"/>     <!-- Content row -->
            <RowDefinition Height="Auto"/>  <!-- Action buttons row -->
        </Grid.RowDefinitions>

        <!-- Row 0: Header -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,16">
            <TextBlock Text="[Page Title]" Style="{StaticResource TextHeadingStyle}"/>
        </StackPanel>

        <!-- Row 1: Search bar -->
        <Grid Grid.Row="1" Margin="0,0,0,8">
            <TextBox Style="{StaticResource TextBoxStyle}"
                     Text="{Binding SearchQuery, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                     Width="300" HorizontalAlignment="Left"/>
        </Grid>

        <!-- Row 2: Loading / Error / Content -->
        <Grid Grid.Row="2">
            <!-- Loading -->
            <TextBlock Text="Đang tải..."
                       Style="{StaticResource TextBodyStyle}"
                       Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
                       HorizontalAlignment="Center" VerticalAlignment="Center"/>

            <!-- Error -->
            <TextBlock Text="{Binding ErrorMessage}"
                       Style="{StaticResource TextErrorStyle}"
                       Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}"
                       VerticalAlignment="Top" Margin="0,8"/>

            <!-- Content (DataGrid example) -->
            <DataGrid Visibility="{Binding IsLoading, Converter={StaticResource InverseBoolToVisibilityConverter}}"
                      ItemsSource="{Binding Items}"
                      Style="{StaticResource DataGridStyle}"
                      AutoGenerateColumns="False" IsReadOnly="True">
                <DataGrid.Columns>
                    <!-- Add columns here -->
                </DataGrid.Columns>
            </DataGrid>
        </Grid>

        <!-- Row 3: Action buttons -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right"
                    Margin="0,8,0,0">
            <Button Content="Thêm mới" Style="{StaticResource ButtonPrimaryStyle}"
                    Command="{Binding CreateCommand}"
                    Margin="0,0,8,0"/>
        </StackPanel>
    </Grid>
</UserControl>
```

---

## Form Layout Template

For create/edit forms:

```xml
<Grid Margin="16">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <!-- Add rows per form field -->
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="120"/>  <!-- Label column -->
        <ColumnDefinition Width="*"/>    <!-- Input column -->
    </Grid.ColumnDefinitions>

    <!-- Field 1 -->
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Họ tên:"
               Style="{StaticResource TextBodyStyle}"
               VerticalAlignment="Center" Margin="0,0,8,8"/>
    <TextBox Grid.Row="0" Grid.Column="1"
             Style="{StaticResource TextBoxStyle}"
             Text="{Binding FullName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
             Margin="0,0,0,8"/>

    <!-- Action buttons -->
    <StackPanel Grid.Row="99" Grid.Column="0" Grid.ColumnSpan="2"
                Orientation="Horizontal" HorizontalAlignment="Right"
                Margin="0,16,0,0">
        <Button Content="Lưu" Style="{StaticResource ButtonPrimaryStyle}"
                Command="{Binding SaveCommand}" Margin="0,0,8,0"/>
        <Button Content="Huỷ" Style="{StaticResource ButtonSecondaryStyle}"
                Command="{Binding CancelCommand}"/>
    </StackPanel>
</Grid>
```

---

## Rules

- Never use inline style attributes (FontSize, Background, Foreground)
- Always verify StaticResource key exists before using it
- Always use `UpdateSourceTrigger=PropertyChanged` on two-way TextBox bindings
- Use `RelativeSource AncestorType` for DataTemplate-to-ViewModel command bindings
- ViewModel must be `partial class` for CommunityToolkit.Mvvm source generators to work

See `docs/project-overview.md` for business domain context.
