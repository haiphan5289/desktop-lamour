---
name: ct-cell
description: Generate a WPF DataTemplate for ListView/ItemsControl items using AppDesignSystem. Creates the XAML DataTemplate with AppDesignSystem controls (AppLabel, AppButton), ViewModel item class, and proper data binding. Use when creating a new reusable list item template.
---

# WPF Basic List Item DataTemplate Generator

Generate `DataTemplate` for `ListView` / `ItemsControl` using AppDesignSystem.

## Input Format

```
ITEM_NAME: <Name, e.g. "UserProfile">
FEATURE: <Module, e.g. "Features/UserManagement">
DATA_MODEL: <Data model type, e.g. "UserModel">
```

## ListView DataTemplate

```xml
<!-- [Name]DataTemplate.xaml (as a ResourceDictionary resource) -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:App.Features.[Feature].Views">

    <DataTemplate x:Key="[Name]DataTemplate" DataType="{x:Type local:[Name]ItemViewModel}">
        <Border Padding="16,8"
                BorderThickness="0,0,0,1"
                BorderBrush="{StaticResource AppColor.BorderThin}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <StackPanel Grid.Column="0" Spacing="4">
                    <local:AppLabel Text="{Binding Title}"
                                    Style="{StaticResource AppTypography.LabelSection}"/>
                    <local:AppLabel Text="{Binding Subtitle}"
                                    Style="{StaticResource AppTypography.BodyCaption}"
                                    Foreground="{StaticResource AppColor.TextSecondary}"/>
                </StackPanel>

                <local:AppButton Grid.Column="1"
                                 Content="Action"
                                 Style="{StaticResource AppButton.Secondary.Small}"
                                 Command="{Binding ActionCommand}"
                                 Visibility="{Binding ShowAction, Converter={StaticResource BoolToVisibilityConverter}}"/>
            </Grid>
        </Border>
    </DataTemplate>

</ResourceDictionary>
```

## ItemViewModel Class

```csharp
// [Name]ItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App.Features.[Feature].ViewModels;

public sealed partial class [Name]ItemViewModel : ObservableObject
{
    // #region Properties

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _subtitle;

    [ObservableProperty]
    private bool _showAction;

    // #region Commands

    [RelayCommand]
    private void Action()
    {
        // TODO: Handle item action
    }

    // #region Factory

    public static [Name]ItemViewModel From([DataModel] model)
        => new()
        {
            Title = model.Name,
            Subtitle = model.Description,
            ShowAction = model.IsActive
        };
}
```

## Using the DataTemplate in a View

```xml
<!-- [Name]ListView.xaml -->
<UserControl x:Class="App.Features.[Feature].Views.[Name]ListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/App;component/Features/[Feature]/Views/[Name]DataTemplate.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>

    <ListView ItemsSource="{Binding Items}"
              ItemTemplate="{StaticResource [Name]DataTemplate}"
              VirtualizingPanel.IsVirtualizing="True"
              VirtualizingPanel.VirtualizationMode="Recycling">
        <ListView.ItemContainerStyle>
            <Style TargetType="ListViewItem">
                <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
                <Setter Property="Padding" Value="0"/>
                <Setter Property="BorderThickness" Value="0"/>
            </Style>
        </ListView.ItemContainerStyle>
    </ListView>
</UserControl>
```

## Rules

- **ALWAYS** use `AppLabel`, `AppButton`, `AppImage` — never raw `TextBlock`, `Button`
- **ALWAYS** use XAML `Grid`/`StackPanel` for layout — no code-behind sizing
- Use `x:Key` on `DataTemplate` and reference it via `ItemTemplate="{StaticResource ...}"`
- Enable `VirtualizingPanel.IsVirtualizing="True"` for large lists
- `[Name]ItemViewModel` is a separate `ObservableObject` class in the same module's `ViewModels/` folder
- Reset / clear item state in the ViewModel when reusing (set properties to default in `From()` factory)
