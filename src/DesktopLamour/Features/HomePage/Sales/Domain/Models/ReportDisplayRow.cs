// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public enum SummaryDimension { Product, Customer, Employee }

public class ReportDisplayRow
{
    public bool      IsGroupHeader  { get; init; }
    public string?   GroupKey       { get; set; }
    public string?   GroupLabel     { get; set; }
    public string    ProductCode    { get; init; } = "";
    public string    ProductName    { get; init; } = "";
    public string    Unit           { get; init; } = "";
    public int       QuantitySold   { get; init; }
    public decimal   SalesAmount    { get; init; }
    public decimal   DiscountAmount { get; init; }
    public int       ReturnQuantity { get; init; }
    public decimal   ReturnValue    { get; init; }
    public decimal   DiscountValue  { get; init; }
    public decimal   NetRevenue     { get; init; }

    /// <summary>
    /// "Mã hàng"/"Tên hàng" always show the row's OWN identity — whichever dimension this row
    /// represents (product, customer, or employee). For 2-dimension types the OUTER dimension is
    /// shown once in the group header, so every leaf row's identity here is the INNER dimension —
    /// no separate "Khách hàng"/"Nhân viên" column is needed at all.
    /// </summary>
    public static ReportDisplayRow Aggregate(
        IReadOnlyCollection<SalesOrderSummaryLineItem> items,
        SummaryDimension identityField, bool showUnit)
    {
        var first = items.First();
        var (code, name) = identityField switch
        {
            SummaryDimension.Product  => (first.ProductCode, first.ProductName),
            SummaryDimension.Customer => (first.CustomerCode, first.CustomerName),
            SummaryDimension.Employee => (first.EmployeeCode, first.EmployeeName),
            _ => ("", ""),
        };

        return new ReportDisplayRow
        {
            ProductCode  = code,
            ProductName  = name,
            Unit         = showUnit ? first.Unit : "",
            QuantitySold   = items.Sum(i => i.QuantitySold),
            SalesAmount    = items.Sum(i => i.SalesAmount),
            DiscountAmount = items.Sum(i => i.DiscountAmount),
            ReturnQuantity = items.Sum(i => i.ReturnQuantity),
            ReturnValue    = items.Sum(i => i.ReturnValue),
            NetRevenue     = items.Sum(i => i.NetRevenue),
        };
    }
}
