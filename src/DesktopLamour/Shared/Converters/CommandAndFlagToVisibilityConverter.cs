// CommandAndFlagToVisibilityConverter.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// MultiBinding cho từng nút của <c>DocumentToolbar</c>:
///  - values[0] = command (object). Null / unset → nút ẩn.
///  - values[1] = cờ Visibility do window truyền vào (mặc định Visible). Nếu là
///    <see cref="Visibility.Collapsed"/>/<see cref="Visibility.Hidden"/> → nút ẩn.
/// Nút chỉ Visible khi VỪA có command VỪA cờ cho phép.
/// </summary>
public class CommandAndFlagToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var hasCommand = values is { Length: > 0 }
                         && values[0] is not null
                         && values[0] != DependencyProperty.UnsetValue;

        var flagAllows = values is not { Length: > 1 }
                         || values[1] is not Visibility v
                         || v == Visibility.Visible;

        return hasCommand && flagAllows ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
