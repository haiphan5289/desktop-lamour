// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderReportLineItem
{
    public int      OrderId        { get; init; }
    public string   DocumentNumber { get; init; } = "";
    public DateTime AccountingDate { get; init; }
    public string   CustomerName   { get; init; } = "";
    public string   EmployeeName   { get; init; } = "";
    public string   ProductCode    { get; init; } = "";
    public string   ProductName    { get; init; } = "";
    public string   Unit           { get; init; } = "";
    public string?  Category       { get; init; }
    public int      Quantity       { get; init; }
    public decimal  UnitPrice      { get; init; }
    public decimal  DiscountRate   { get; init; }
    public decimal  Amount         { get; init; }
    public decimal  TaxRate        { get; init; }
    public decimal  TaxAmount      { get; init; }

    public decimal GrandTotal => Amount + TaxAmount;

    public static SalesOrderReportLineItem FromDto(SalesOrderReportLineDto dto) => new()
    {
        OrderId        = dto.OrderId,
        DocumentNumber = dto.DocumentNumber,
        AccountingDate = dto.AccountingDate.ToLocalTime(),
        CustomerName   = dto.CustomerName,
        EmployeeName   = dto.EmployeeName ?? "—",
        ProductCode    = dto.ProductCode,
        ProductName    = dto.ProductName,
        Unit           = dto.Unit,
        Category       = dto.Category,
        Quantity       = dto.Quantity,
        UnitPrice      = dto.UnitPrice,
        DiscountRate   = dto.DiscountRate,
        Amount         = dto.Amount,
        TaxRate        = dto.TaxRate,
        TaxAmount      = dto.TaxAmount,
    };
}
