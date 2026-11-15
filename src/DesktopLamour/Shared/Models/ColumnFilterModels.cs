// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

// Một giá trị trong danh sách checkbox của ValueListColumnFilter — Value là khoá so khớp thật
// ("" = Trống), Display là chữ hiện trên popup ("(Trống)" thay cho chuỗi rỗng).
public partial class FilterValueOption : ObservableObject
{
    public string Value   { get; }
    public string Display { get; }

    [ObservableProperty] private bool _isChecked;

    public FilterValueOption(string value, string display, bool isChecked)
    {
        Value      = value;
        Display    = display;
        _isChecked = isChecked;
    }
}

// Lọc kiểu Excel/MISA AutoFilter: popup liệt kê MỌI giá trị khác nhau đang có trong cột (kèm ô tìm
// kiếm nhanh), tick/bỏ tick từng giá trị rồi bấm OK mới áp dụng — Cancel hoặc bấm ra ngoài popup thì
// trả lại đúng lựa chọn đã áp dụng lần gần nhất (không áp nửa chừng). Không tick bớt gì (mặc định) =
// không lọc cột này. Dùng chung cho mọi loại cột (chữ lẫn số) — ViewModel tự format giá trị hiển thị
// trước khi đưa vào SetSource (vd. số tiền format theo MoneyFormat.Format để khớp đúng chữ trên lưới).
public partial class ValueListColumnFilter : ObservableObject
{
    public ObservableCollection<FilterValueOption> Options { get; } = new();
    public ICollectionView FilteredOptions { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool   _isPopupOpen;
    [ObservableProperty] private bool   _isActive; // true = đang lọc bớt giá trị (khác "Tất cả")

    // Rỗng = "Tất cả" (không lọc); có phần tử = chỉ các Value này được hiện.
    private HashSet<string> _committed = new(StringComparer.Ordinal);

    public Action? Changed { get; set; }

    public ValueListColumnFilter()
    {
        FilteredOptions = CollectionViewSource.GetDefaultView(Options);
        FilteredOptions.Filter = MatchesSearch;
    }

    private bool MatchesSearch(object obj)
        => string.IsNullOrWhiteSpace(SearchText)
           || obj is FilterValueOption o && o.Display.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase);

    partial void OnSearchTextChanged(string value) => FilteredOptions.Refresh();

    // Tri-state cho checkbox "Tất cả" ở đầu popup.
    public bool? AllChecked
    {
        get
        {
            if (Options.Count == 0) return false;
            var checkedCount = Options.Count(o => o.IsChecked);
            return checkedCount switch
            {
                0 => false,
                var n when n == Options.Count => true,
                _ => null,
            };
        }
        set
        {
            var v = value == true;
            foreach (var o in Options) o.IsChecked = v;
            OnPropertyChanged();
        }
    }

    // Nạp lại danh sách giá trị khác nhau của cột (gọi mỗi khi dữ liệu báo cáo được tải lại). Giữ
    // nguyên lựa chọn đã áp dụng (_committed) nếu giá trị đó còn tồn tại trong tập mới; nếu lựa chọn
    // cũ không còn giá trị nào khớp (vd. đổi khoảng ngày khác hẳn) thì tự reset về "Tất cả" để tránh
    // lưới trống trơn mà không rõ lý do.
    public void SetSource(IEnumerable<string?> rawValues)
    {
        foreach (var o in Options) o.PropertyChanged -= OnOptionChanged;
        Options.Clear();

        var distinct = rawValues.Select(v => v?.Trim() ?? string.Empty)
            .Distinct()
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (_committed.Count > 0 && !distinct.Any(_committed.Contains))
            _committed.Clear();

        foreach (var v in distinct)
        {
            var display   = v.Length == 0 ? "(Trống)" : v;
            var isChecked = _committed.Count == 0 || _committed.Contains(v);
            var opt       = new FilterValueOption(v, display, isChecked);
            opt.PropertyChanged += OnOptionChanged;
            Options.Add(opt);
        }

        IsActive = _committed.Count > 0;
        OnPropertyChanged(nameof(AllChecked));
    }

    private void OnOptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterValueOption.IsChecked))
            OnPropertyChanged(nameof(AllChecked));
    }

    [RelayCommand]
    private void TogglePopup() => IsPopupOpen = !IsPopupOpen;

    [RelayCommand]
    private void Apply()
    {
        var checkedValues = Options.Where(o => o.IsChecked).Select(o => o.Value).ToHashSet();
        _committed = checkedValues.Count == Options.Count ? new HashSet<string>() : checkedValues;
        IsActive   = _committed.Count > 0;
        IsPopupOpen = false;
        Changed?.Invoke();
    }

    [RelayCommand]
    private void CancelPopup() => IsPopupOpen = false;

    // Đóng popup bằng mọi đường (OK / Hủy / bấm ra ngoài) đều phải đồng bộ lại checkbox theo đúng
    // _committed — Apply() đã cập nhật _committed trước khi set IsPopupOpen=false nên lần đồng bộ này
    // không đổi gì thêm; Cancel/bấm-ra-ngoài thì đây chính là bước "hoàn tác" các tick dở dang.
    partial void OnIsPopupOpenChanged(bool value)
    {
        if (value) return;
        foreach (var o in Options) o.IsChecked = _committed.Count == 0 || _committed.Contains(o.Value);
        SearchText = string.Empty;
    }

    public bool Matches(string? cellValue) => _committed.Count == 0 || _committed.Contains((cellValue ?? string.Empty).Trim());
}
