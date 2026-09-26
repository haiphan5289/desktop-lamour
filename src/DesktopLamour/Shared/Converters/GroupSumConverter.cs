// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Shared.Helpers;

namespace DesktopLamour.Shared.Converters;

public class GroupSumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable items) return "";
        var rows = items.Cast<ReportDisplayRow>().ToList();

        return (parameter as string) switch
        {
            "Quantity"       => rows.Sum(r => r.QuantitySold).ToString(),
            "SalesAmount"    => MoneyFormat.Format(rows.Sum(r => r.SalesAmount)),
            "DiscountAmount" => MoneyFormat.Format(rows.Sum(r => r.DiscountAmount)),
            "ReturnQuantity" => rows.Sum(r => r.ReturnQuantity).ToString(),
            "ReturnValue"    => MoneyFormat.Format(rows.Sum(r => r.ReturnValue)),
            "DiscountValue"  => MoneyFormat.Format(rows.Sum(r => r.DiscountValue)),
            "NetRevenue"     => MoneyFormat.Format(rows.Sum(r => r.NetRevenue)),
            "CostAmount"     => MoneyFormat.Format(rows.Sum(r => r.CostAmount)),
            "GrossProfit"    => MoneyFormat.Format(rows.Sum(r => r.GrossProfit)),
            // Tính lại từ tổng Lãi gộp/Doanh thu thuần của cả nhóm — không cộng dồn % của từng
            // dòng lẻ (sẽ ra sai số học).
            "GrossProfitRate" => MoneyFormat.Format(GrossProfitRateFor(rows), "N2"),
            _ => "",
        };
    }

    private static decimal GrossProfitRateFor(List<ReportDisplayRow> rows)
    {
        var netRevenue = rows.Sum(r => r.NetRevenue);
        return netRevenue == 0 ? 0 : rows.Sum(r => r.GrossProfit) / netRevenue * 100;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
