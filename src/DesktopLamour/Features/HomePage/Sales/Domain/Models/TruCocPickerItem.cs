// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

// "Sản phẩm ảo" duy nhất đại diện cho hành động "Trừ cọc" trong dropdown chọn sản phẩm ở Tab 1
// (Mã hàng/Tên hàng) — không còn liệt kê/cho chọn từng Deposit riêng lẻ. User chỉ nhập 1 số tiền
// cần trừ; BE tự động phân bổ (FIFO — cọc cũ nhất trước) qua nhiều Deposit của khách hàng nếu cần
// (xem CreateDepositDeductionUseCase ở BE, deposits.md — "Trừ cọc tự động phân bổ FIFO"). DB vẫn
// giữ nhiều Deposit record riêng (không gộp), chỉ trải nghiệm người dùng gộp thành 1 chỗ duy nhất.
// TotalRemainingBalance = tổng số dư còn lại của khách hàng tại thời điểm load (đã loại sẵn cọc do
// chính chứng từ đang sửa tạo ra — self-sourced — qua BE param exclude_sales_order_id).
public sealed class TruCocPickerItem : ISearchableItem
{
    public decimal TotalRemainingBalance { get; }

    public TruCocPickerItem(decimal totalRemainingBalance) => TotalRemainingBalance = totalRemainingBalance;

    // Id âm cố định để không bao giờ trùng Id sản phẩm thật (luôn dương).
    public int    Id          => -1;
    public string Code        => "";
    public string Name        => "Trừ cọc";
    public string DisplayText => $"🔖 Trừ cọc — {TotalRemainingBalance:N0}";
}
