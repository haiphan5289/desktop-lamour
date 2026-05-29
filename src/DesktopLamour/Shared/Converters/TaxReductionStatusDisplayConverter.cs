// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;

namespace DesktopLamour.Shared.Converters;

[ValueConversion(typeof(TaxReductionStatus?), typeof(string))]
public class TaxReductionStatusDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value is TaxReductionStatus status ? status switch
        {
            TaxReductionStatus.CoGiamThue   => "Có giảm thuế",
            TaxReductionStatus.ChuaGiamThue => "Không giảm thuế",
            TaxReductionStatus.ChuaXacDinh  => "Chưa xác định",
            _                               => status.ToString(),
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
