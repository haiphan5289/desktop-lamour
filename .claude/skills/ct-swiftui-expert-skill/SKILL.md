---
name: ct-swiftui-expert-skill
description: Expert WPF XAML guidance for Desktop Lamour — DataTrigger, MultiTrigger, ControlTemplate customization, Converter implementations (BoolToVisibility, InverseBool, DecimalToString), value converter registration in App.xaml.
model: sonnet
effort: medium
---

# Expert WPF XAML Guidance for Desktop Lamour

> **Anti-Hallucination:** Verify every class name, interface, namespace, and file path against the codebase before generating code. See [lamour-anti-hallucination](.claude/skills/ct-anti-hallucination/SKILL.md).

Advanced WPF XAML patterns for Desktop Lamour — triggers, converters, control templates, and animations.

---

## Value Converters

### BoolToVisibilityConverter

```csharp
// src/DesktopLamour/Shared/Converters/BoolToVisibilityConverter.cs
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
```

### InverseBoolToVisibilityConverter

```csharp
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not Visibility.Visible;
}
```

### StringToVisibilityConverter (hide when empty)

```csharp
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

### DecimalToStringConverter (for formatted currency display)

```csharp
public class DecimalToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d.ToString("N0", new CultureInfo("vi-VN")); // e.g. 1.250.000
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (decimal.TryParse(value?.ToString()?.Replace(".", "").Replace(",", ""),
            out var result))
            return result;
        return 0m;
    }
}
```

---

## Registering Converters in App.xaml

```xml
<!-- src/DesktopLamour/App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/AppTypography.xaml"/>
            <ResourceDictionary Source="Shared/AppStyles.xaml"/>
        </ResourceDictionary.MergedDictionaries>

        <!-- Converters -->
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
        <converters:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibilityConverter"/>
        <converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter"/>
        <converters:DecimalToStringConverter x:Key="DecimalToStringConverter"/>
    </ResourceDictionary>
</Application.Resources>
```

Add the namespace to App.xaml root:
```xml
xmlns:converters="clr-namespace:DesktopLamour.Shared.Converters"
```

---

## DataTrigger Patterns

### Change style based on boolean property

```xml
<Style x:Key="StockTextStyle" TargetType="TextBlock"
       BasedOn="{StaticResource TextBodyStyle}">
    <Style.Triggers>
        <!-- Change foreground to red when IsLowStock is true -->
        <DataTrigger Binding="{Binding IsLowStock}" Value="True">
            <Setter Property="Foreground" Value="{StaticResource ErrorForegroundBrush}"/>
            <Setter Property="FontWeight" Value="Bold"/>
        </DataTrigger>
    </Style.Triggers>
</Style>
```

### Change appearance based on string value

```xml
<Style x:Key="InvoiceStatusStyle" TargetType="TextBlock"
       BasedOn="{StaticResource TextCaptionStyle}">
    <Style.Triggers>
        <DataTrigger Binding="{Binding Status}" Value="Confirmed">
            <Setter Property="Foreground" Value="{StaticResource SuccessForegroundBrush}"/>
        </DataTrigger>
        <DataTrigger Binding="{Binding Status}" Value="Cancelled">
            <Setter Property="Foreground" Value="{StaticResource ErrorForegroundBrush}"/>
        </DataTrigger>
    </Style.Triggers>
</Style>
```

---

## MultiTrigger — Multiple Conditions

```xml
<!-- Highlight row when selected AND confirmed -->
<Style x:Key="InvoiceRowStyle" TargetType="DataGridRow"
       BasedOn="{StaticResource DataGridRowStyle}">
    <Style.Triggers>
        <MultiDataTrigger>
            <MultiDataTrigger.Conditions>
                <Condition Binding="{Binding IsSelected, RelativeSource={RelativeSource Self}}"
                           Value="True"/>
                <Condition Binding="{Binding IsConfirmed}" Value="True"/>
            </MultiDataTrigger.Conditions>
            <Setter Property="Background" Value="{StaticResource SuccessBackgroundBrush}"/>
        </MultiDataTrigger>
    </Style.Triggers>
</Style>
```

---

## ControlTemplate Customization

### Custom Button with loading state

```xml
<Style x:Key="ButtonLoadingStyle" TargetType="Button"
       BasedOn="{StaticResource ButtonPrimaryStyle}">
    <Style.Triggers>
        <DataTrigger Binding="{Binding IsLoading}" Value="True">
            <Setter Property="IsEnabled" Value="False"/>
            <Setter Property="Content" Value="Đang xử lý..."/>
        </DataTrigger>
    </Style.Triggers>
</Style>
```

Usage:
```xml
<Button Style="{StaticResource ButtonLoadingStyle}"
        Content="Xác nhận"
        Command="{Binding ConfirmCommand}"/>
```

---

## CommandParameter with RelativeSource

### Passing typed parameter from DataGrid row

```xml
<DataGridTemplateColumn Header="Hành động" Width="100">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <Button Content="Sửa"
                        Style="{StaticResource ButtonGhostStyle}"
                        Command="{Binding DataContext.EditCommand,
                                  RelativeSource={RelativeSource AncestorType=DataGrid}}"
                        CommandParameter="{Binding}"
                        Margin="0,0,4,0"/>
                <Button Content="Xoá"
                        Style="{StaticResource ButtonDangerStyle}"
                        Command="{Binding DataContext.DeleteCommand,
                                  RelativeSource={RelativeSource AncestorType=DataGrid}}"
                        CommandParameter="{Binding Id}"/>
            </StackPanel>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

---

## IMultiValueConverter for Combined Bindings

```csharp
// Show stock as "X / Y" (current / max)
public class StockRatioConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is int current && values[1] is int max)
            return $"{current} / {max}";
        return "—";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource StockRatioConverter}">
            <Binding Path="CurrentStock"/>
            <Binding Path="MaxStock"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

---

## INotifyDataErrorInfo for Form Validation

For forms with inline validation, implement `INotifyDataErrorInfo` in ViewModel:

```csharp
public partial class CreateEmployeeViewModel : ObservableValidator // inherits INotifyDataErrorInfo
{
    [ObservableProperty]
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [MinLength(2, ErrorMessage = "Họ tên phải có ít nhất 2 ký tự")]
    private string _fullName = string.Empty;

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        ValidateAllProperties();
        if (HasErrors) return;
        // proceed with save
    }
}
```

```xml
<!-- Validation error template in XAML -->
<TextBox Style="{StaticResource TextBoxStyle}"
         Text="{Binding FullName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged,
                ValidatesOnNotifyDataErrors=True}"/>
```

See `docs/project-overview.md` for business domain context.
