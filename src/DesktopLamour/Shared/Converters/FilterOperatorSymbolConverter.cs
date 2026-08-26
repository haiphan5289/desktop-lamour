// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Shared.Models;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(FilterOperator), typeof(string))]
public class FilterOperatorSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is FilterOperator op ? op.ToSymbol() : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
