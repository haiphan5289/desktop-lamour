// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// 1 dòng hạch toán trong popup xác nhận "Phiếu thu tiền khách hàng hàng loạt" — Amount mặc định =
// RemainingAmount lúc chọn, cho sửa để thu 1 phần. TK Nợ/TK Có cố định cho cả phiếu (chọn 1 lần ở
// popup tìm kiếm — "Phương thức thanh toán"), không cho sửa riêng từng dòng.
public partial class BulkReceiptLineItem : ObservableObject
{
    public int      SalesOrderId   { get; }
    public string   DocumentNumber { get; }
    public DateTime AccountingDate { get; }
    public int      CustomerId     { get; }
    public string   CustomerCode   { get; }
    public string   CustomerName   { get; }
    public decimal  MaxAmount      { get; }
    public decimal  GrandTotal     { get; }
    public string?  PaymentTerms   { get; }
    public DateTime? PaymentDueDate { get; }

    // Gán 1 lần lúc Initialize() từ TK Nợ/TK Có chọn ở popup tìm kiếm — chỉ để hiển thị trên grid
    // (khớp ảnh mẫu MISA có cột TK Nợ/TK Có), không phải field cho sửa riêng từng dòng.
    public string DebitAccountDisplay  { get; set; } = "";
    public string CreditAccountDisplay { get; set; } = "";

    [ObservableProperty] private decimal _amount;

    public BulkReceiptLineItem(OutstandingSalesOrderCheckItem source)
    {
        SalesOrderId    = source.SalesOrderId;
        DocumentNumber  = source.DocumentNumber;
        AccountingDate  = source.AccountingDate;
        CustomerId      = source.Order.CustomerId;
        CustomerCode    = source.CustomerCode;
        CustomerName    = source.CustomerName;
        MaxAmount       = source.RemainingAmount;
        GrandTotal      = source.GrandTotal;
        PaymentTerms    = source.PaymentTerms;
        PaymentDueDate  = source.PaymentDueDate;
        _amount         = source.RemainingAmount;
    }
}
