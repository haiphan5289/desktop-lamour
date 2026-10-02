// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// Tham số của báo cáo "Sổ kế toán chi tiết quỹ tiền mặt" — kết quả của CashLedgerReportFilterWindow.
public sealed class CashLedgerReportFilter
{
    public string   Period         { get; init; } = CashLedgerReportPeriods.ThisMonth;
    public DateTime FromDate       { get; init; }
    public DateTime ToDate         { get; init; }
    // Mã các TK được tick. Rỗng = mọi TK tiền mặt (111*).
    public IReadOnlyList<string> AccountCodes { get; init; } = Array.Empty<string>();
    public bool     MergeSimilar   { get; init; }
    public bool     OrderByCreated { get; init; }

    // Dòng phụ đề dưới tiêu đề báo cáo — đúng 1 tháng thì "Tháng 01 năm 2026" (như MISA), còn lại "Từ ngày … đến ngày …".
    public string Subtitle
    {
        get
        {
            var wholeMonth = FromDate.Day == 1
                && FromDate.Year == ToDate.Year && FromDate.Month == ToDate.Month
                && ToDate.Day == DateTime.DaysInMonth(ToDate.Year, ToDate.Month);
            return wholeMonth
                ? $"Tháng {FromDate:MM} năm {FromDate:yyyy}"
                : $"Từ ngày {FromDate:dd/MM/yyyy} đến ngày {ToDate:dd/MM/yyyy}";
        }
    }
}

public static class CashLedgerReportPeriods
{
    public const string Today       = "Hôm nay";
    public const string ThisWeek    = "Tuần này";
    public const string MonthToDate = "Đầu tháng đến hiện tại";
    public const string ThisMonth   = "Tháng này";
    public const string LastMonth   = "Tháng trước";
    public const string ThisQuarter = "Quý này";
    public const string ThisYear    = "Năm nay";
    public const string Custom      = "Tùy chọn";

    // "Tháng 1" … "Tháng 12" / "Quý I" … "Quý IV" của NĂM HIỆN TẠI (khớp danh sách Kỳ báo cáo của MISA).
    public static string Month(int month)     => $"Tháng {month}";
    public static string Quarter(int quarter) => $"Quý {new[] { "I", "II", "III", "IV" }[quarter - 1]}";

    public static readonly IReadOnlyList<string> All =
        new[] { Today, ThisWeek, MonthToDate, ThisMonth, LastMonth, ThisQuarter, ThisYear }
            .Concat(Enumerable.Range(1, 12).Select(Month))
            .Concat(Enumerable.Range(1, 4).Select(Quarter))
            .Append(Custom)
            .ToArray();

    // Khoảng ngày của 1 kỳ; null = "Tùy chọn" (giữ nguyên Từ/Đến đang nhập).
    public static (DateTime From, DateTime To)? Range(string period, DateTime today)
    {
        static DateTime EndOfMonth(int year, int month) => new(year, month, DateTime.DaysInMonth(year, month));

        switch (period)
        {
            case Today:       return (today, today);
            case ThisWeek:
                var monday = today.AddDays(-((7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7));
                return (monday, monday.AddDays(6));
            case MonthToDate: return (new DateTime(today.Year, today.Month, 1), today);
            case ThisMonth:   return (new DateTime(today.Year, today.Month, 1), EndOfMonth(today.Year, today.Month));
            case LastMonth:
                var last = today.AddMonths(-1);
                return (new DateTime(last.Year, last.Month, 1), EndOfMonth(last.Year, last.Month));
            case ThisQuarter:
                var q = (today.Month - 1) / 3 + 1;
                return (new DateTime(today.Year, (q - 1) * 3 + 1, 1), EndOfMonth(today.Year, q * 3));
            case ThisYear:    return (new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31));
        }

        for (var m = 1; m <= 12; m++)
            if (period == Month(m)) return (new DateTime(today.Year, m, 1), EndOfMonth(today.Year, m));
        for (var quarter = 1; quarter <= 4; quarter++)
            if (period == Quarter(quarter))
                return (new DateTime(today.Year, (quarter - 1) * 3 + 1, 1), EndOfMonth(today.Year, quarter * 3));

        return null;
    }
}
