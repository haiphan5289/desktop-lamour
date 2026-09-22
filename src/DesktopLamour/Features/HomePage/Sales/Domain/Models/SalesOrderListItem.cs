// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderListItem
{
    public int      Id             { get; init; }
    public string   DocumentNumber { get; init; } = "";
    public DateTime AccountingDate { get; init; }
    public DateTime DocumentDate   { get; init; }
    public string?  Description    { get; init; }
    public string   CustomerName   { get; init; } = "";
    public string?  EmployeeName   { get; init; }
    public decimal  TotalGross     { get; init; }
    public decimal  TotalDiscount  { get; init; }
    public decimal  TotalTax       { get; init; }
    public decimal  TotalPayment   { get; init; }
    public string?  Notes          { get; init; }
    public int      Status         { get; init; }
    public string   StatusLabel    { get; init; } = "";

    // 2026-09-13: thêm để dùng trong SalesOrderListViewModel.FilterItem (mirror
    // SalesReturnListItem.IsConfirmed/IsHeld) — dễ đọc hơn so sánh Status == 0 / is 1 or 2 lặp lại.
    public bool IsConfirmed => Status == 0;
    public bool IsHeld      => Status is 1 or 2;

    // 2026-09-14 (theo ảnh mẫu MISA — cột "Đã xuất hàng"/"Loại chứng từ"): app xuất kho NGAY lúc
    // Ghi sổ chứng từ bán hàng (không có bước xuất kho tách rời), nên "Đã xuất hàng" chỉ là nhãn
    // suy ra từ IsConfirmed, không phải field/API mới. "Loại chứng từ" luôn 1 câu cố định cho mọi
    // chứng từ bán hàng (giống HeaderSubtitle không đổi trong SalesOrderWindow), không có field
    // BE tương ứng.
    public string StockExportedLabel => IsConfirmed ? "Đã xuất" : "Chưa xuất";
    public string DocumentTypeLabel  => "Bán hàng hóa, dịch vụ trong nước chưa thu tiền";

    public SalesOrderResponseDto Original { get; init; } = null!;

    public static SalesOrderListItem FromDto(SalesOrderResponseDto dto)
    {
        var gross    = dto.Lines.Sum(l => (decimal)l.Quantity * l.UnitPrice);
        var discount = dto.Lines.Sum(l => (decimal)l.Quantity * l.UnitPrice * l.DiscountRate / 100m);
        return new SalesOrderListItem
        {
            Id             = dto.Id,
            DocumentNumber = dto.DocumentNumber,
            AccountingDate = dto.AccountingDate.ToLocalTime(),
            DocumentDate   = dto.DocumentDate.ToLocalTime(),
            Description    = dto.Description,
            CustomerName   = dto.CustomerName,
            EmployeeName   = dto.EmployeeName,
            TotalGross     = gross,
            TotalDiscount  = discount,
            TotalTax       = dto.TotalTaxAmount,
            // 2026-09-22: dùng GrandTotal (= TotalAmount + TotalTaxAmount) thay vì TotalAmount —
            // "Tổng tiền thanh toán" phải gồm thuế GTGT và tự động cấn trừ dòng Trừ cọc (Amount âm),
            // trước đây dùng TotalAmount khiến đơn có Trừ cọc hiện số âm sai (vd -41.600đ thay vì 0đ).
            TotalPayment   = dto.GrandTotal,
            Notes          = dto.Notes,
            Status         = dto.Status,
            // 2026-09-11: gộp "Nháp" (Draft=2) và "Treo" (Held=1) thành 1 label duy nhất "⏸ Treo" —
            // mirror SalesReturnListItem cùng ngày, theo yêu cầu sau khi xác nhận BE không còn phân
            // biệt nghiệp vụ giữa 2 giá trị này (UnconfirmSalesOrderUseCase giờ luôn gán Held).
            StatusLabel    = dto.Status switch { 0 => "📄 Ghi sổ", _ => "⏸ Treo" },
            Original       = dto,
        };
    }
}
