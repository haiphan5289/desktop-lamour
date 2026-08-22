// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Visible when the bound string equals the converter parameter, Collapsed otherwise.
/// </summary>
public sealed class StringEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && parameter is string p && s == p ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
