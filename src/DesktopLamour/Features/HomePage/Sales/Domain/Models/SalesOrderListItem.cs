// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderListItem
{
    public int      Id             { get; init; }
    public string   DocumentNumber { get; init; } = "";
    public DateTime DocumentDate   { get; init; }
    public string   CustomerName   { get; init; } = "";
    public string?  EmployeeName   { get; init; }
    public decimal  TotalGross     { get; init; }
    public decimal  TotalDiscount  { get; init; }
    public decimal  TotalPayment   { get; init; }
    public string?  Notes          { get; init; }
    public int      Status         { get; init; }
    public string   StatusLabel    { get; init; } = "";

    public SalesOrderResponseDto Original { get; init; } = null!;

    public static SalesOrderListItem FromDto(SalesOrderResponseDto dto)
    {
        var gross    = dto.Lines.Sum(l => (decimal)l.Quantity * l.UnitPrice);
        var discount = dto.Lines.Sum(l => (decimal)l.Quantity * l.UnitPrice * l.DiscountRate / 100m);
        return new SalesOrderListItem
        {
            Id             = dto.Id,
            DocumentNumber = dto.DocumentNumber,
            DocumentDate   = dto.DocumentDate.ToLocalTime(),
            CustomerName   = dto.CustomerName,
            EmployeeName   = dto.EmployeeName,
            TotalGross     = gross,
            TotalDiscount  = discount,
            TotalPayment   = dto.TotalAmount,
            Notes          = dto.Notes,
            Status         = dto.Status,
            StatusLabel    = dto.Status switch { 1 => "⏸ Treo", _ => "📄 Ghi sổ" },
            Original       = dto,
        };
    }
}
