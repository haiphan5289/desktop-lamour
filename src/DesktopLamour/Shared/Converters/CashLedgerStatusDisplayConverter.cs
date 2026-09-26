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
            // 2026-09-26: không còn "Nháp" (khớp Chứng từ bán hàng) — Draft cũ hiển thị là Treo.
            "Draft"     => "Treo",
            "Treo"      => "Treo",
            "Confirmed" => "Đã ghi sổ",
            _           => status,
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
