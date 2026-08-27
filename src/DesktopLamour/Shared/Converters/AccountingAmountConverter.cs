// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

/// <summary>
/// Currency converter cho ô có thể mang giá trị âm (vd. "Thành tiền" dòng Trừ cọc trong
/// SalesOrderWindow): số âm hiển thị kiểu kế toán "(1.814.400)" thay vì "-1.814.400"; số dương
/// hiển thị bình thường; 0 hiển thị trống, giống <see cref="BlankPreserveConverter"/>. ConvertBack
/// chấp nhận cả "1814400", "-1814400" và "(1814400)" — user gõ số dương hay số âm trực tiếp đều
/// parse ra cùng kết quả, tuỳ logic phía sau (vd. Amount setter dòng Trừ cọc) có tự âm hoá hay không.
/// </summary>
public sealed class AccountingAmountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IConvertible convertible) return value;

        decimal d;
        try
        {
            d = System.Convert.ToDecimal(convertible, culture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return value;
        }

        if (d == 0m) return string.Empty;
        var format = parameter as string ?? "N0";
        return d < 0m
            ? $"({Math.Abs(d).ToString(format, culture)})"
            : d.ToString(format, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return System.Convert.ChangeType(0, underlyingType, culture);

        text = text.Trim();
        var isParenNegative = text.StartsWith('(') && text.EndsWith(')');
        if (isParenNegative) text = text[1..^1];

        try
        {
            var parsed = (decimal)System.Convert.ChangeType(text, typeof(decimal), culture);
            if (isParenNegative) parsed = -Math.Abs(parsed);
            return System.Convert.ChangeType(parsed, underlyingType, culture);
        }
        catch (FormatException)
        {
            return Binding.DoNothing;
        }
    }
}
