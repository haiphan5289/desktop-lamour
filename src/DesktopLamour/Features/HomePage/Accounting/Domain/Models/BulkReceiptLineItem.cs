// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// 1 dòng hạch toán trong popup xác nhận "Phiếu thu tiền khách hàng hàng loạt" — Amount mặc định =
// RemainingAmount lúc chọn, cho sửa để thu 1 phần. TK Nợ/TK Có cố định cho cả phiếu (chọn 1 lần ở
// popup tìm kiếm — "Phương thức thanh toán"), không cho sửa riêng từng dòng.
public partial class BulkReceiptLineItem : ObservableObject
{
    public int      SalesOrderId   { get; }
    public string   DocumentNumber { get; }
    public DateTime AccountingDate { get; }
    // 2026-09-26: cột "Ngày chứng từ" phải là NGÀY CHỨNG TỪ của hóa đơn (khớp MISA), trước đây bind
    // nhầm AccountingDate (ngày hạch toán) — 2 ngày thường trùng nên khó thấy.
    public DateTime DocumentDate   { get; }
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
        DocumentDate    = source.Order.DocumentDate;
        CustomerId      = source.Order.CustomerId;
        CustomerCode    = source.CustomerCode;
        CustomerName    = source.CustomerName;
        MaxAmount       = source.RemainingAmount;
        GrandTotal      = source.GrandTotal;
        PaymentTerms    = source.PaymentTerms;
        PaymentDueDate  = source.PaymentDueDate;
        _amount         = source.RemainingAmount;
    }

    // 2026-09-26: dựng lại dòng khi Sửa 1 phiếu thu hàng loạt ĐÃ LƯU — order lấy từ
    // IGetSalesOrdersByIdsUseCase (không lọc còn nợ/ngày, xem BE doc comment), currentAmount lấy từ
    // ReceiptEntryDto.Amount của chính phiếu đang sửa. order.RemainingAmount lúc này ĐÃ trừ luôn phần
    // currentAmount (BE tính trên toàn bộ ReceiptEntry trỏ tới đơn, không phân biệt phiếu nào) — cộng
    // lại currentAmount mới ra đúng "Số chưa thu" tối đa cho phép sửa (phần còn nợ THẬT + phần phiếu
    // này đang giữ chỗ).
    public BulkReceiptLineItem(OutstandingSalesOrderDto order, decimal currentAmount)
    {
        SalesOrderId    = order.SalesOrderId;
        DocumentNumber  = order.DocumentNumber;
        AccountingDate  = order.AccountingDate;
        DocumentDate    = order.DocumentDate;
        CustomerId      = order.CustomerId;
        CustomerCode    = order.CustomerCode;
        CustomerName    = order.CustomerName;
        MaxAmount       = order.RemainingAmount + currentAmount;
        GrandTotal      = order.GrandTotal;
        PaymentTerms    = order.PaymentTerms;
        PaymentDueDate  = order.PaymentDueDate;
        _amount         = currentAmount;
    }
}
