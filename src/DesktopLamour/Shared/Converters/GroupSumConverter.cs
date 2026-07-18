// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;

namespace DesktopLamour.Shared.Converters;

public class GroupSumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable items) return "";
        var rows = items.Cast<ReportDisplayRow>().ToList();
        var money = CultureInfo.GetCultureInfo("vi-VN");

        return (parameter as string) switch
        {
            "Quantity"       => rows.Sum(r => r.QuantitySold).ToString(),
            "SalesAmount"    => rows.Sum(r => r.SalesAmount).ToString("N0", money),
            "DiscountAmount" => rows.Sum(r => r.DiscountAmount).ToString("N0", money),
            "ReturnQuantity" => rows.Sum(r => r.ReturnQuantity).ToString(),
            "ReturnValue"    => rows.Sum(r => r.ReturnValue).ToString("N0", money),
            "DiscountValue"  => rows.Sum(r => r.DiscountValue).ToString("N0", money),
            "NetRevenue"     => rows.Sum(r => r.NetRevenue).ToString("N0", money),
            _ => "",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
