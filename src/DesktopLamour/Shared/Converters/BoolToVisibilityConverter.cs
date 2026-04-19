// BoolToVisibilityConverter.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>Set to true to invert: false → Visible, true → Collapsed.</summary>
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = value is bool b && b;
        if (Invert) isVisible = !isVisible;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}
