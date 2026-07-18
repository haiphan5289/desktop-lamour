// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;

namespace DesktopLamour.Shared.Converters;

public class GroupHeaderTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable items) return "";
        var rows = items.Cast<ReportDisplayRow>().ToList();
        if (rows.Count == 0) return "";
        var first = rows[0];
        return $"{first.GroupLabel} : {first.GroupKey} ({rows.Count})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
