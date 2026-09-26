// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLamour.Shared.Models;

// Backs the per-column filter row embedded directly in a DataGrid's column headers (no popup) —
// used across multiple "Sổ chi tiết"-style read-only report grids (Sales, Accounting, Warehouse,
// Deposits...). Date/numeric columns get an operator (=, ≤, ...) + typed value shown side by side;
// text columns just use a plain Contains-match string in the owning ViewModel.
// Moved here from Features/HomePage/Sales/Domain/Models (2026-08-22) — originally built only for
// SalesOrderReportDetailView ("Sổ chi tiết bán hàng"), now shared so other feature modules don't
// have to depend on the Sales module to reuse it.
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
        if (!TryParseFilterNumber(ValueText, out var target))
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
    // 2026-09-26: ô số trong lưới giờ hiển thị kiểu vi-VN ("1.500.000", "35,50") — ô lọc phải hiểu
    // đúng kiểu user nhìn thấy mà gõ lại: "1.500.000" (chấm = hàng nghìn), "35,5" (phẩy = thập phân),
    // đồng thời vẫn nhận kiểu cũ "1500000" / "35.5". Trước đây parse Invariant nên "1.500.000" bị
    // coi là không hợp lệ và bộ lọc tự bỏ qua.
    private static bool TryParseFilterNumber(string? text, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim().Replace(" ", "");
        if (t.StartsWith('(') && t.EndsWith(')')) t = "-" + t[1..^1];
        if (System.Text.RegularExpressions.Regex.IsMatch(t, @"^-?\d{1,3}(,\d{3})+$"))
            t = t.Replace(",", "");                         // kiểu cũ en-US "1,500,000"
        if (t.Contains(','))
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out value);
        if (System.Text.RegularExpressions.Regex.IsMatch(t, @"^-?\d{1,3}(\.\d{3})+$"))
            t = t.Replace(".", "");
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
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
