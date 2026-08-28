// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class PaymentReasonDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is string reason ? reason switch
        {
            "ThuKhac"     => "Thu khác",
            "ThuTienHang" => "Thu tiền hàng",
            "ThuCongNo"   => "Thu công nợ",
            "ChiKhac"     => "Chi khác",
            "ChiMuaHang"  => "Chi mua hàng",
            "ChiTraNo"    => "Chi trả nợ",
            "ChiLuong"    => "Chi lương",
            _             => reason,
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
