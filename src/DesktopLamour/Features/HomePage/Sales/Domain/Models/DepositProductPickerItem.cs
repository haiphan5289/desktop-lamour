// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

// "Sản phẩm ảo" đại diện cho 1 khoản cọc còn số dư — cho phép chọn "Trừ cọc" ngay trong
// dropdown chọn sản phẩm ở Tab 1 (Mã hàng/Tên hàng), như chọn 1 sản phẩm bình thường.
public sealed class DepositProductPickerItem : ISearchableItem
{
    public DepositResponseDto Deposit { get; }

    public DepositProductPickerItem(DepositResponseDto deposit) => Deposit = deposit;

    // Id âm để không bao giờ trùng Id sản phẩm thật (luôn dương).
    public int    Id          => -Deposit.Id;
    public string Code        => Deposit.DocumentNumber;
    public string Name        => "Trừ cọc";
    public string DisplayText => $"{Deposit.DocumentNumber} — Trừ cọc (còn {Deposit.RemainingBalance:N0})";
}
