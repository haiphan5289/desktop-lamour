// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

// Drill-down filter passed when a summary report row is double-clicked — narrows the
// existing report filter down to the row's own dimension(s) (product/customer/employee).
public class SalesOrderDetailFilter
{
    public string   Title      { get; init; } = "";
    public int?     ProductId  { get; init; }
    public int?     EmployeeId { get; init; }
    public int?     CustomerId { get; init; }
    public string?  Unit       { get; init; }
    public string?  Category   { get; init; }
    public DateTime? FromDate  { get; init; }
    public DateTime? ToDate    { get; init; }

    // 2026-09-18: report type gốc đã drill-down từ đó (vd "Nhân viên") — SalesOrderReportDetailView
    // dùng để đổi tiêu đề/bộ cột cho khớp biến thể riêng của MISA (vd "SỔ CHI TIẾT BÁN HÀNG THEO
    // NHÂN VIÊN" khi drill từ report 1 chiều "Nhân viên"). Rỗng = màn "Sổ chi tiết bán hàng" chung
    // như cũ.
    public string?  SourceReportType { get; init; }
}
