// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class CashLedgerStatusDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is string status ? status switch
        {
            "Draft"     => "Nháp",
            "Treo"      => "Treo",
            "Confirmed" => "Đã ghi số",
            _           => status,
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
