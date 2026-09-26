// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Shared.Helpers;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Converter HIỂN THỊ số (chỉ đọc) chuẩn toàn app — thay cho <c>StringFormat=N0</c>: luôn vi-VN
/// ("1.600.000") bất kể culture của binding, số âm trong ngoặc ("(1.458.000)"), 0 vẫn hiện "0".
/// ConverterParameter = chuỗi format (mặc định "N0", vd "N2" cho tỷ lệ %). Không dùng cho ô NHẬP
/// liệu — ô nhập giữ converter/StringFormat riêng để không chèn "(" ")" giữa lúc đang gõ.
/// </summary>
public sealed class MoneyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return string.Empty;
        if (value is not IConvertible convertible || value is string) return value;

        decimal d;
        try
        {
            d = System.Convert.ToDecimal(convertible, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return value;
        }

        return MoneyFormat.Format(d, parameter as string ?? "N0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
