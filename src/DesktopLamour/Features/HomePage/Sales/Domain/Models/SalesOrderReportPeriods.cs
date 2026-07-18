// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public static class SalesOrderReportPeriods
{
    public const string Today      = "Hôm nay";
    public const string Yesterday  = "Hôm qua";
    public const string ThisWeek   = "Tuần này";
    public const string LastWeek   = "Tuần trước";
    public const string ThisMonth  = "Tháng này";
    public const string LastMonth  = "Tháng trước";
    public const string MonthToDate = "Đầu tháng đến hiện tại";
    public const string ThisQuarter = "Quý này";
    public const string ThisYear    = "Năm nay";
    public const string Custom      = "Tùy chỉnh";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Today, Yesterday, ThisWeek, LastWeek,
        ThisMonth, LastMonth, MonthToDate, ThisQuarter, ThisYear,
        Custom,
    };
}
