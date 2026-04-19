---
name: swiftui-design-system
description: Desktop Lamour XAML design system quick reference. AppStyles.xaml keys, AppTypography.xaml keys, color brush keys, spacing thickness keys, correct binding patterns (UpdateSourceTrigger=PropertyChanged), DataGrid column templates, loading overlay pattern.
model: haiku
effort: low
---

# Desktop Lamour XAML Design System Quick Reference

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Quick reference card for Desktop Lamour WPF styles, bindings, and patterns.

---

## Resource Dictionary Locations

| File | Contains |
|---|---|
| `src/DesktopLamour/Themes/AppTypography.xaml` | Text styles, font sizes, label styles |
| `src/DesktopLamour/Shared/AppStyles.xaml` | Button, input, DataGrid, card styles |
| `src/DesktopLamour/App.xaml` | Merged dictionaries, global converters |

---

## Typography Keys (from AppTypography.xaml)

Verify keys exist before use with: `Grep: pattern="x:Key=\"...\""` in Themes folder.

```xml
<!-- Headings -->
Style="{StaticResource TextHeadingStyle}"       <!-- Page/section title -->
Style="{StaticResource TextSubheadingStyle}"    <!-- Sub-section title -->

<!-- Body -->
Style="{StaticResource TextBodyStyle}"          <!-- Regular content -->
Style="{StaticResource TextCaptionStyle}"       <!-- Small labels, hints -->

<!-- State text -->
Style="{StaticResource TextErrorStyle}"         <!-- Validation errors -->
Style="{StaticResource TextSuccessStyle}"       <!-- Success messages -->
Style="{StaticResource TextMutedStyle}"         <!-- Disabled/secondary text -->
```

---

## Button Keys (from AppStyles.xaml)

```xml
Style="{StaticResource ButtonPrimaryStyle}"     <!-- Main CTA: Lưu, Xác nhận -->
Style="{StaticResource ButtonSecondaryStyle}"   <!-- Secondary: Huỷ, Quay lại -->
Style="{StaticResource ButtonDangerStyle}"      <!-- Destructive: Xoá -->
Style="{StaticResource ButtonGhostStyle}"       <!-- Text-only: Xem chi tiết -->
```

---

## Input Keys

```xml
Style="{StaticResource TextBoxStyle}"           <!-- Standard text input -->
Style="{StaticResource PasswordBoxStyle}"       <!-- Password input -->
Style="{StaticResource ComboBoxStyle}"          <!-- Dropdown select -->
```

---

## DataGrid Keys

```xml
Style="{StaticResource DataGridStyle}"
RowStyle="{StaticResource DataGridRowStyle}"
ColumnHeaderStyle="{StaticResource DataGridColumnHeaderStyle}"
```

---

## Card / Container Keys

```xml
Style="{StaticResource CardBorderStyle}"        <!-- White card with shadow -->
Style="{StaticResource SeparatorStyle}"         <!-- Horizontal divider -->
```

---

## Correct Binding Patterns

### Two-way TextBox (ALWAYS use UpdateSourceTrigger)

```xml
<TextBox Text="{Binding SearchQuery, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
         Style="{StaticResource TextBoxStyle}"/>
```

### Command binding

```xml
<!-- Method Load() → generated LoadCommand -->
<Button Command="{Binding LoadCommand}" Content="Tải"/>

<!-- With parameter -->
<Button Command="{Binding DeleteCommand}"
        CommandParameter="{Binding Id}"
        Content="Xoá"/>
```

### Command from DataTemplate → parent ViewModel

```xml
<Button Command="{Binding DataContext.DeleteEmployeeCommand,
                  RelativeSource={RelativeSource AncestorType=UserControl}}"
        CommandParameter="{Binding Id}"/>
```

### Visibility converters

```xml
<!-- Bool to Visibility -->
<TextBlock Visibility="{Binding IsLoading,
           Converter={StaticResource BoolToVisibilityConverter}}"/>

<!-- Inverse: hide when loading -->
<Grid Visibility="{Binding IsLoading,
      Converter={StaticResource InverseBoolToVisibilityConverter}}"/>

<!-- String to Visibility (hide when empty) -->
<TextBlock Text="{Binding ErrorMessage}"
           Visibility="{Binding ErrorMessage,
           Converter={StaticResource StringToVisibilityConverter}}"/>
```

---

## Loading Overlay Pattern

Standard loading overlay for any view:

```xml
<Grid>
    <!-- Main content -->
    <ContentPresenter Visibility="{Binding IsLoading,
                      Converter={StaticResource InverseBoolToVisibilityConverter}}"/>

    <!-- Loading overlay -->
    <Border Visibility="{Binding IsLoading,
            Converter={StaticResource BoolToVisibilityConverter}}"
            Background="#80FFFFFF">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <TextBlock Text="Đang tải..." Style="{StaticResource TextBodyStyle}"/>
        </StackPanel>
    </Border>

    <!-- Error message -->
    <TextBlock Text="{Binding ErrorMessage}"
               Style="{StaticResource TextErrorStyle}"
               Visibility="{Binding ErrorMessage,
               Converter={StaticResource StringToVisibilityConverter}}"
               Margin="16,8"/>
</Grid>
```

---

## DataGrid with Action Column Pattern

```xml
<DataGrid ItemsSource="{Binding Items}"
          AutoGenerateColumns="False" IsReadOnly="True"
          Style="{StaticResource DataGridStyle}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Tên" Binding="{Binding Name}" Width="*"/>
        <DataGridTextColumn Header="Số lượng" Binding="{Binding Stock}" Width="100"/>
        <DataGridTemplateColumn Header="" Width="80">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Button Content="Xoá"
                            Style="{StaticResource ButtonDangerStyle}"
                            Command="{Binding DataContext.DeleteCommand,
                                      RelativeSource={RelativeSource AncestorType=DataGrid}}"
                            CommandParameter="{Binding Id}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

---

## CommunityToolkit.Mvvm Generated Names

| Field declaration | Generated binding name |
|---|---|
| `_isLoading` | `IsLoading` |
| `_errorMessage` | `ErrorMessage` |
| `_searchQuery` | `SearchQuery` |
| `LoadAsync()` with `[RelayCommand]` | `LoadAsyncCommand` |
| `DeleteEmployee()` with `[RelayCommand]` | `DeleteEmployeeCommand` |

---

## Forbidden Patterns

```xml
<!-- FORBIDDEN -->
<TextBlock FontSize="16" FontWeight="Bold"/>       ← use StaticResource style
<Button Background="#007AFF" Foreground="White"/>  ← use StaticResource style
<TextBox Text="{Binding Name}"/>                   ← missing UpdateSourceTrigger
```
