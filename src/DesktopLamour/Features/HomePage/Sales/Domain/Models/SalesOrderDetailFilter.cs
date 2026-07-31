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
}
