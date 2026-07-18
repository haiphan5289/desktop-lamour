// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderReportFilter
{
    public string                ReportType    { get; init; } = SalesOrderReportTypes.ByProduct;
    public IReadOnlyList<int>    ProductIds    { get; init; } = Array.Empty<int>();
    public IReadOnlyList<string> ProductLabels { get; init; } = Array.Empty<string>();
    public int?      EmployeeId    { get; init; }
    public string?   EmployeeLabel { get; init; }
    public int?      CustomerId    { get; init; }
    public string?   CustomerLabel { get; init; }
    public string?   Unit          { get; init; }
    public string?   Category      { get; init; }
    public DateTime? FromDate      { get; init; }
    public DateTime? ToDate        { get; init; }

    public string Summary
    {
        get
        {
            var parts = new List<string> { $"Thống kê theo: {ReportType}" };

            if (ProductLabels.Count == 1)
                parts.Add($"Mặt hàng: {ProductLabels[0]}");
            else if (ProductLabels.Count > 1)
                parts.Add($"Mặt hàng: {ProductLabels.Count} sản phẩm");
            if (!string.IsNullOrWhiteSpace(Unit))
                parts.Add($"ĐVT: {Unit}");
            if (!string.IsNullOrWhiteSpace(Category))
                parts.Add($"Nhóm VTHH: {Category}");
            if (!string.IsNullOrWhiteSpace(EmployeeLabel))
                parts.Add($"Nhân viên: {EmployeeLabel}");
            if (!string.IsNullOrWhiteSpace(CustomerLabel))
                parts.Add($"Khách hàng: {CustomerLabel}");
            if (FromDate.HasValue)
                parts.Add($"Từ ngày {FromDate.Value:dd/MM/yyyy}");
            if (ToDate.HasValue)
                parts.Add($"đến ngày {ToDate.Value:dd/MM/yyyy}");

            return $"Đang lọc: {string.Join(" · ", parts)}";
        }
    }
}
