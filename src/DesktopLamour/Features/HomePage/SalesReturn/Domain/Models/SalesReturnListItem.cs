// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;

public class SalesReturnListItem
{
    public int      Id             { get; init; }
    public string   DocumentNumber { get; init; } = "";
    public DateTime DocumentDate   { get; init; }
    public string   CustomerName   { get; init; } = "";
    public string?  EmployeeName   { get; init; }
    public decimal  TotalAmount    { get; init; }
    public decimal  TotalDiscount  { get; init; }
    public decimal  TotalPayment   { get; init; }
    public string   ReturnTypeLabel { get; init; } = "";

    public SalesReturnResponseDto Original { get; init; } = null!;

    public static SalesReturnListItem FromDto(SalesReturnResponseDto dto) => new()
    {
        Id              = dto.Id,
        DocumentNumber  = dto.DocumentNumber,
        DocumentDate    = dto.DocumentDate.ToLocalTime(),
        CustomerName    = dto.CustomerName,
        EmployeeName    = dto.EmployeeName,
        TotalAmount     = dto.TotalAmount,
        TotalDiscount   = dto.TotalDiscount,
        TotalPayment    = dto.TotalPayment,
        ReturnTypeLabel = dto.ReturnType == 1 ? "Trả lại tiền mặt" : "Giảm trừ công nợ",
        Original        = dto,
    };
}
