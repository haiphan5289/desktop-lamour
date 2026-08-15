// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public class WarehouseReceiptFlatItem
{
    public int      Id            { get; init; }
    public string   ReceiptNumber { get; init; } = string.Empty;
    public int      ReceiptType   { get; init; }
    public string   Status        { get; init; } = string.Empty;
    public string?  CustomerName  { get; init; }
    public string?  SupplierName  { get; init; }
    public string?  ObjectName    => CustomerName ?? SupplierName;
    public string?  EmployeeName  { get; init; }
    public DateTime DocumentDate  { get; init; }
    public decimal  TotalAmount   { get; init; }

    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
}
