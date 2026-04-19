// StringToVisibilityConverter.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Converts a string to Visibility.
/// Non-null, non-empty → Visible; null or empty → Collapsed.
/// </summary>
[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
