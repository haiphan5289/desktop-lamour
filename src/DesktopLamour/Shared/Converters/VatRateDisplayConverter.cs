// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(VatRateType?), typeof(string))]
public class VatRateDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is VatRateType rate ? rate switch
        {
            VatRateType.Zero  => "0%",
            VatRateType.Five  => "5%",
            VatRateType.Eight => "8%",
            VatRateType.Ten   => "10%",
            VatRateType.KCT   => "KCT",
            VatRateType.KKKNT => "KKKNT",
            VatRateType.KHAC  => "KHAC",
            _                 => rate.ToString(),
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
