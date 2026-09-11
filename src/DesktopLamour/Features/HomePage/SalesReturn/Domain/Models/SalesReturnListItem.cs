// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;

public class SalesReturnListItem
{
    public int      Id             { get; init; }
    public string   DocumentNumber { get; init; } = "";
    public DateTime AccountingDate { get; init; }
    public DateTime DocumentDate   { get; init; }
    public string   CustomerName   { get; init; } = "";
    public string?  EmployeeName   { get; init; }
    public string?  Description    { get; init; }
    public decimal  TotalAmount    { get; init; }
    public decimal  TotalDiscount  { get; init; }
    // Response chứng từ trả lại không có sẵn tổng thuế ở cấp header (khác SalesOrderResponseDto) —
    // cộng dồn từ tax_amount của từng dòng để hiện cột "Tiền thuế GTGT" + hàng tổng cộng cuối lưới.
    public decimal  TotalTax       { get; init; }
    public decimal  TotalPayment   { get; init; }
    public string   ReturnTypeLabel { get; init; } = "";

    // "Draft" | "Held" | "Confirmed" — nguyên giá trị BE trả về, dùng để so sánh CanExecute (Sửa/
    // Xóa/Ghi sổ/Bỏ ghi) mà không cần switch chuỗi lặp lại ở nhiều nơi. "Held" ("Treo") thêm
    // 2026-09-10 — mirror SalesOrderListItem.
    public string  Status      { get; init; } = "Draft";
    public bool    IsDraft     => Status == "Draft";
    public bool    IsHeld      => Status == "Held";
    public bool    IsConfirmed => Status == "Confirmed";
    public string  StatusLabel => Status switch { "Held" => "⏸ Treo", "Confirmed" => "📄 Đã ghi sổ", _ => "↩️ Nháp" };

    // "Kiêm phiếu nhập" — tính client-side sau khi FromDto (không có trong DTO gốc), so khớp với
    // danh sách WarehouseReceipt hiện có (ReceiptType=ReturnedGoods + Reference=DocumentNumber),
    // giống hệt logic dedup đã có trong SalesReturnViewModel.EnsureWarehouseReceiptPrintedAsync.
    // Không thể "init" vì được set SAU khi item đã tạo — xem SalesReturnListViewModel.LoadSalesReturnsAsync.
    public bool   HasLinkedWarehouseReceipt      { get; set; }
    public string HasLinkedWarehouseReceiptLabel => HasLinkedWarehouseReceipt ? "Có" : "Chưa";

    public SalesReturnResponseDto Original { get; init; } = null!;

    public static SalesReturnListItem FromDto(SalesReturnResponseDto dto) => new()
    {
        Id              = dto.Id,
        DocumentNumber  = dto.DocumentNumber,
        AccountingDate  = dto.AccountingDate.ToLocalTime(),
        DocumentDate    = dto.DocumentDate.ToLocalTime(),
        CustomerName    = dto.CustomerName,
        EmployeeName    = dto.EmployeeName,
        Description     = dto.Description,
        TotalAmount     = dto.TotalAmount,
        TotalDiscount   = dto.TotalDiscount,
        TotalTax        = dto.Lines.Sum(l => l.TaxAmount),
        TotalPayment    = dto.TotalPayment,
        ReturnTypeLabel = dto.ReturnType == 1 ? "Trả lại tiền mặt" : "Giảm trừ công nợ",
        Status          = dto.Status,
        Original        = dto,
    };
}
