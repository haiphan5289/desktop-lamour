// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

// Backs the per-column filter row on SalesOrderReportDetailView ("Sổ chi tiết bán hàng"), embedded
// directly in each header (no popup): date/numeric columns get an operator (=, ≤, ...) + typed value
// shown side by side; text columns just use a plain Contains-match string (see the ViewModel).
public enum FilterOperator { Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual }

public static class FilterOperatorSymbols
{
    public static string ToSymbol(this FilterOperator op) => op switch
    {
        FilterOperator.Equal          => "=",
        FilterOperator.NotEqual       => "≠",
        FilterOperator.Less           => "<",
        FilterOperator.LessOrEqual    => "≤",
        FilterOperator.Greater        => ">",
        FilterOperator.GreaterOrEqual => "≥",
        _ => "=",
    };

    public static string ToLabel(this FilterOperator op) => op switch
    {
        FilterOperator.Equal          => "= Bằng",
        FilterOperator.NotEqual       => "≠ Khác",
        FilterOperator.Less           => "< Nhỏ hơn",
        FilterOperator.LessOrEqual    => "≤ Nhỏ hơn hoặc bằng",
        FilterOperator.Greater        => "> Lớn hơn",
        FilterOperator.GreaterOrEqual => "≥ Lớn hơn hoặc bằng",
        _ => "= Bằng",
    };

    public static readonly FilterOperator[] All =
    {
        FilterOperator.Equal, FilterOperator.NotEqual, FilterOperator.Less,
        FilterOperator.LessOrEqual, FilterOperator.Greater, FilterOperator.GreaterOrEqual,
    };
}

// Numeric column filter — operator icon (=, ≤, ...) + typed number, e.g. "Số lượng", "Thành tiền".
public partial class NumericColumnFilter : ObservableObject
{
    [ObservableProperty] private FilterOperator _operator = FilterOperator.LessOrEqual;
    [ObservableProperty] private string         _valueText = string.Empty;

    public Action? Changed { get; set; }
    public string  OperatorSymbol => Operator.ToSymbol();

    partial void OnOperatorChanged(FilterOperator value)
    {
        OnPropertyChanged(nameof(OperatorSymbol));
        Changed?.Invoke();
    }

    partial void OnValueTextChanged(string value) => Changed?.Invoke();

    public bool Matches(decimal cellValue)
    {
        if (!decimal.TryParse(ValueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var target))
            return true;

        return Operator switch
        {
            FilterOperator.Equal          => cellValue == target,
            FilterOperator.NotEqual       => cellValue != target,
            FilterOperator.Less           => cellValue <  target,
            FilterOperator.LessOrEqual    => cellValue <= target,
            FilterOperator.Greater        => cellValue >  target,
            FilterOperator.GreaterOrEqual => cellValue >= target,
            _ => true,
        };
    }
}

// Date column filter — operator icon (=, ≤, ...) + picked date, e.g. "Ngày hạch toán".
public partial class DateColumnFilter : ObservableObject
{
    [ObservableProperty] private FilterOperator _operator = FilterOperator.Equal;
    [ObservableProperty] private DateTime?      _value;

    public Action? Changed { get; set; }
    public string  OperatorSymbol => Operator.ToSymbol();

    partial void OnOperatorChanged(FilterOperator value)
    {
        OnPropertyChanged(nameof(OperatorSymbol));
        Changed?.Invoke();
    }

    partial void OnValueChanged(DateTime? value) => Changed?.Invoke();

    public bool Matches(DateTime cellValue)
    {
        if (Value is not { } target) return true;

        var cmp = cellValue.Date.CompareTo(target.Date);
        return Operator switch
        {
            FilterOperator.Equal          => cmp == 0,
            FilterOperator.NotEqual       => cmp != 0,
            FilterOperator.Less           => cmp <  0,
            FilterOperator.LessOrEqual    => cmp <= 0,
            FilterOperator.Greater        => cmp >  0,
            FilterOperator.GreaterOrEqual => cmp >= 0,
            _ => true,
        };
    }
}
