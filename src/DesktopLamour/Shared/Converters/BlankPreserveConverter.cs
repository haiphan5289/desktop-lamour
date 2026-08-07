// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Display-only counterpart to <see cref="BlankToZeroConverter"/>: a bound value of exactly 0 renders as
/// an empty cell instead of "0"/"0.00", so a never-entered field reads as "not entered" rather than
/// "entered as zero". Editing behaves exactly like <see cref="BlankToZeroConverter"/> — Backspace/Delete
/// down to an empty cell commits 0 immediately (which then redisplays as blank via Convert), so clearing
/// a cell always works. Formatting is done here (via ConverterParameter, e.g. "N0"/"N2") instead of
/// Binding.StringFormat, since StringFormat would re-render the blank-for-zero string back into text.
/// </summary>
public sealed class BlankPreserveConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not decimal d) return value;
        if (d == 0m) return string.Empty;
        var format = parameter as string ?? "N0";
        return d.ToString(format, culture);
    }

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
