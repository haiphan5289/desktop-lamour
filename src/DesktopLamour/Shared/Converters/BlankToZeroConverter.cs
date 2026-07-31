// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Treats a blank numeric cell as 0 instead of a binding validation error.
/// Without this, clearing a decimal/int DataGrid cell (e.g. Đơn giá) leaves the
/// binding stuck on an invalid value and the grid unusable until it's fixed.
/// </summary>
public sealed class BlankToZeroConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return System.Convert.ChangeType(0, underlyingType, culture);

        try
        {
            return System.Convert.ChangeType(text, underlyingType, culture);
        }
        catch (FormatException)
        {
            return Binding.DoNothing;
        }
    }
}
