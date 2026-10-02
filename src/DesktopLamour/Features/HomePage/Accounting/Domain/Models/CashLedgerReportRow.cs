// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// 1 dòng hiển thị trên lưới báo cáo "Sổ kế toán chi tiết quỹ tiền mặt". Dòng đầu là "Số tồn đầu kỳ"
// (IsOpening, không có ngày/số chứng từ); các dòng sau bọc 1 CashLedgerDetailRowDto.
public sealed class CashLedgerReportRow
{
    public bool      IsOpening      { get; init; }
    public DateTime? AccountingDate { get; init; }
    public DateTime? DocumentDate   { get; init; }
    public string?   ReceiptNumber  { get; init; }
    public string?   PaymentNumber  { get; init; }
    public string    Description    { get; init; } = "";
    public string    Account        { get; init; } = "";
    public string    CounterAccount { get; init; } = "";
    public decimal   DebitAmount    { get; init; }
    public decimal   CreditAmount   { get; init; }
    public decimal   Balance        { get; init; }
    public string?   PersonName     { get; init; }
    public string?   CategoryCode   { get; init; }
    public string?   CategoryName   { get; init; }
    public int?      ReceiptId      { get; init; }
    public int?      PaymentId      { get; init; }
    public bool      IsBulkReceipt  { get; init; }

    // Số chứng từ chỉ bấm được khi còn phiếu gốc.
    public bool CanOpenReceipt => ReceiptId is not null;
    public bool CanOpenPayment => PaymentId is not null;

    public static CashLedgerReportRow Opening(decimal balance) => new()
    {
        IsOpening   = true,
        Description = "Số tồn đầu kỳ",
        Account     = "111",
        Balance     = balance,
    };

    public static CashLedgerReportRow From(CashLedgerDetailRowDto dto) => new()
    {
        AccountingDate = dto.AccountingDate.ToLocalTime().Date,
        DocumentDate   = dto.DocumentDate.ToLocalTime().Date,
        ReceiptNumber  = dto.ReceiptNumber,
        PaymentNumber  = dto.PaymentNumber,
        Description    = dto.Description,
        Account        = dto.Account,
        CounterAccount = dto.CounterAccount,
        DebitAmount    = dto.DebitAmount,
        CreditAmount   = dto.CreditAmount,
        Balance        = dto.Balance,
        PersonName     = dto.PersonName,
        CategoryCode   = dto.CategoryCode,
        CategoryName   = dto.CategoryName,
        ReceiptId      = dto.ReceiptId,
        PaymentId      = dto.PaymentId,
        IsBulkReceipt  = dto.IsBulkReceipt,
    };
}

// 1 dòng tài khoản trong hộp "Chọn tham số" (Số tài khoản · Tên tài khoản · Bậc + ô tick).
public sealed class CashAccountCheckItem : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    private bool _isSelected = true;

    public string Code  { get; init; } = "";
    public string Name  { get; init; } = "";
    // Bậc: 111 = 1, 1111 = 2, …
    public int    Level => Math.Max(1, Code.Length - 2);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
