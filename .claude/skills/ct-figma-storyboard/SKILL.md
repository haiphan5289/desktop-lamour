---
name: ct-figma-storyboard
description: Generate a Desktop Lamour WPF Window (not UserControl) for modal dialogs, login screen, or main shell. Full Window XAML with WindowStyle, SizeToContent, owner binding, and ViewModel DataContext setup via DI.
model: sonnet
effort: medium
---

# WPF Window Generator for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Generate full WPF Window XAML and code-behind for modal dialogs, login screen, or the main shell window.

## Input Format

```
WINDOW_NAME: <e.g. "Login" | "CreateEmployee" | "ConfirmInvoice">
PURPOSE: <e.g. "Login screen" | "Modal dialog to create employee" | "Main shell">
IS_DIALOG: <true | false>
SIZE: <e.g. "400x300" | "SizeToContent | "Maximized">
```

---

## Dialog Window Template (IS_DIALOG: true)

```xml
<!-- src/DesktopLamour/Features/[Module]/Views/[Name]Window.xaml -->
<Window x:Class="DesktopLamour.Features.[Module].Views.[Name]Window"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="[Title]"
        Width="480"
        Height="360"
        WindowStyle="SingleBorderWindow"
        ResizeMode="NoResize"
        WindowStartupLocation="CenterOwner"
        ShowInTaskbar="False">

    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>    <!-- Title row -->
            <RowDefinition Height="*"/>       <!-- Content row -->
            <RowDefinition Height="Auto"/>    <!-- Button row -->
        </Grid.RowDefinitions>

        <!-- Dialog title -->
        <TextBlock Grid.Row="0"
                   Text="[Dialog Title]"
                   Style="{StaticResource TextHeadingStyle}"
                   Margin="0,0,0,16"/>

        <!-- Form content -->
        <StackPanel Grid.Row="1">
            <TextBlock Text="Họ tên:" Style="{StaticResource TextBodyStyle}" Margin="0,0,0,4"/>
            <TextBox Style="{StaticResource TextBoxStyle}"
                     Text="{Binding FullName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                     Margin="0,0,0,12"/>

            <!-- Error message -->
            <TextBlock Text="{Binding ErrorMessage}"
                       Style="{StaticResource TextErrorStyle}"
                       Visibility="{Binding ErrorMessage,
                                    Converter={StaticResource StringToVisibilityConverter}}"/>
        </StackPanel>

        <!-- Action buttons -->
        <StackPanel Grid.Row="2" Orientation="Horizontal"
                    HorizontalAlignment="Right" Margin="0,16,0,0">
            <Button Content="Lưu"
                    Style="{StaticResource ButtonPrimaryStyle}"
                    Command="{Binding SaveCommand}"
                    IsDefault="True"
                    Margin="0,0,8,0"/>
            <Button Content="Huỷ"
                    Style="{StaticResource ButtonSecondaryStyle}"
                    IsCancel="True"/>
        </StackPanel>
    </Grid>
</Window>
```

```csharp
// src/DesktopLamour/Features/[Module]/Views/[Name]Window.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using DesktopLamour.Features.[Module].ViewModels;

namespace DesktopLamour.Features.[Module].Views;

public partial class [Name]Window : Window
{
    public [Name]Window([Name]ViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Close window when Save command succeeds
        // viewModel.CloseRequested += (_, _) => DialogResult = true;
    }
}
```

---

## Login Window Template

```xml
<!-- src/DesktopLamour/Features/Authentication/Views/LoginWindow.xaml -->
<Window x:Class="DesktopLamour.Features.Authentication.Views.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Desktop Lamour — Đăng nhập"
        Width="400"
        Height="340"
        WindowStyle="None"
        ResizeMode="NoResize"
        WindowStartupLocation="CenterScreen"
        AllowsTransparency="True">

    <Border Style="{StaticResource CardBorderStyle}" Margin="0">
        <Grid Margin="32">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Logo / App name -->
            <TextBlock Grid.Row="0" Text="Desktop Lamour"
                       Style="{StaticResource TextHeadingStyle}"
                       HorizontalAlignment="Center" Margin="0,0,0,24"/>

            <!-- Phone number -->
            <TextBlock Grid.Row="1" Text="Số điện thoại:"
                       Style="{StaticResource TextBodyStyle}" Margin="0,0,0,4"/>
            <TextBox Grid.Row="2"
                     Style="{StaticResource TextBoxStyle}"
                     Text="{Binding PhoneNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                     Margin="0,0,0,12"/>

            <!-- Error message -->
            <TextBlock Grid.Row="3"
                       Text="{Binding ErrorMessage}"
                       Style="{StaticResource TextErrorStyle}"
                       Visibility="{Binding ErrorMessage,
                                    Converter={StaticResource StringToVisibilityConverter}}"
                       Margin="0,0,0,12"/>

            <!-- Login button -->
            <Button Grid.Row="4"
                    Content="Đăng nhập"
                    Style="{StaticResource ButtonPrimaryStyle}"
                    Command="{Binding LoginCommand}"
                    IsDefault="True"/>
        </Grid>
    </Border>
</Window>
```

---

## Main Shell Window Template

```xml
<!-- src/DesktopLamour/MainWindow/MainWindow.xaml -->
<Window x:Class="DesktopLamour.MainWindow.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Desktop Lamour"
        Width="1280" Height="800"
        MinWidth="900" MinHeight="600"
        WindowStartupLocation="CenterScreen">

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="200"/>   <!-- Navigation sidebar -->
            <ColumnDefinition Width="*"/>     <!-- Main content area -->
        </Grid.ColumnDefinitions>

        <!-- Sidebar navigation -->
        <Border Grid.Column="0" Background="{StaticResource SidebarBackgroundBrush}">
            <StackPanel Margin="0,16">
                <Button Content="Nhân viên"
                        Style="{StaticResource NavButtonStyle}"
                        Command="{Binding NavigateToEmployeesCommand}"/>
                <Button Content="Kho hàng"
                        Style="{StaticResource NavButtonStyle}"
                        Command="{Binding NavigateToInventoryCommand}"/>
                <Button Content="Nhập hàng"
                        Style="{StaticResource NavButtonStyle}"
                        Command="{Binding NavigateToImportInvoicesCommand}"/>
                <Button Content="Xuất hàng"
                        Style="{StaticResource NavButtonStyle}"
                        Command="{Binding NavigateToExportInvoicesCommand}"/>
            </StackPanel>
        </Border>

        <!-- Main content — ViewModel-first navigation -->
        <ContentControl Grid.Column="1"
                        Content="{Binding CurrentViewModel}">
            <ContentControl.Resources>
                <!-- DataTemplates for each module ViewModel go here -->
                <!-- Add after each ViewModel is created -->
            </ContentControl.Resources>
        </ContentControl>
    </Grid>
</Window>
```

---

## DI Registration for Windows

```csharp
// Windows that need DI injection registered as Transient:
services.AddTransient<LoginWindow>();
services.AddTransient<CreateEmployeeWindow>();
services.AddTransient<LoginViewModel>();
services.AddTransient<CreateEmployeeViewModel>();
```

Opening a dialog from code:

```csharp
// In ViewModel or navigation service:
var dialog = _serviceProvider.GetRequiredService<CreateEmployeeWindow>();
dialog.Owner = Application.Current.MainWindow;
var result = dialog.ShowDialog();
if (result == true)
    await LoadEmployeesCommand.ExecuteAsync(null);
```

---

## Rules

- Windows receive ViewModel via constructor injection (DI)
- `IsDefault="True"` on the primary action button (activates on Enter)
- `IsCancel="True"` on Cancel button (closes dialog on Escape)
- `WindowStartupLocation="CenterOwner"` for dialogs, `CenterScreen` for main windows
- Never use `new SomeViewModel()` in Window constructor — inject via DI
- `ShowInTaskbar="False"` for modal dialogs

See `docs/project-overview.md` for module context.
