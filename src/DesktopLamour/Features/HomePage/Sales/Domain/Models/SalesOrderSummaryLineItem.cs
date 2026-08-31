// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderSummaryLineItem
{
    public int     ProductId      { get; init; }
    public string  ProductCode    { get; init; } = "";
    public string  ProductName    { get; init; } = "";
    public string  Unit           { get; init; } = "";
    public int     CustomerId     { get; init; }
    public string  CustomerCode   { get; init; } = "";
    public string  CustomerName   { get; init; } = "";
    public int?    EmployeeId     { get; init; }
    public string  EmployeeCode   { get; init; } = "";
    public string  EmployeeName   { get; init; } = "";
    public int     QuantitySold   { get; init; }
    public decimal SalesAmount    { get; init; }
    public decimal DiscountAmount { get; init; }
    public int     ReturnQuantity { get; init; }
    public decimal ReturnValue    { get; init; }
    public decimal NetRevenue     { get; init; }
    public decimal CostAmount        { get; init; }
    public decimal GrossProfit       { get; init; }
    public decimal GrossProfitRate   { get; init; }
    public string  CustomerGroupName { get; init; } = "";

    public static SalesOrderSummaryLineItem FromDto(SalesOrderSummaryLineDto dto) => new()
    {
        ProductId      = dto.ProductId,
        ProductCode    = dto.ProductCode,
        ProductName    = dto.ProductName,
        Unit           = dto.Unit,
        CustomerId     = dto.CustomerId,
        CustomerCode   = dto.CustomerCode,
        CustomerName   = dto.CustomerName,
        EmployeeId     = dto.EmployeeId,
        EmployeeCode   = dto.EmployeeCode ?? "",
        EmployeeName   = dto.EmployeeName ?? "—",
        QuantitySold   = dto.QuantitySold,
        SalesAmount    = dto.SalesAmount,
        DiscountAmount = dto.DiscountAmount,
        ReturnQuantity = dto.ReturnQuantity,
        ReturnValue    = dto.ReturnValue,
        NetRevenue     = dto.NetRevenue,
        CostAmount        = dto.CostAmount,
        GrossProfit       = dto.GrossProfit,
        GrossProfitRate   = dto.GrossProfitRate,
        CustomerGroupName = dto.CustomerGroupName ?? "",
    };
}
