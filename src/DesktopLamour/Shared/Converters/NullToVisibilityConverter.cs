// NullToVisibilityConverter.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Converts a reference value to Visibility.
/// Non-null → Visible; null (or an unset binding) → Collapsed.
/// Dùng cho <c>DocumentToolbar</c>: nút chỉ hiện khi VM có bind command tương ứng.
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>Set true để đảo: null → Visible, non-null → Collapsed.</summary>
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hasValue = value is not null && value != DependencyProperty.UnsetValue;
        if (Invert) hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
