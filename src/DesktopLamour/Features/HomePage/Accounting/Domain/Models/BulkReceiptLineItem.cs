// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// 1 dòng hạch toán trong popup xác nhận "Phiếu thu tiền khách hàng hàng loạt" — Amount mặc định =
// RemainingAmount lúc chọn, cho sửa để thu 1 phần.
public partial class BulkReceiptLineItem : ObservableObject
{
    public int      SalesOrderId   { get; }
    public string   DocumentNumber { get; }
    public int      CustomerId     { get; }
    public string   CustomerCode   { get; }
    public string   CustomerName   { get; }
    public decimal  MaxAmount      { get; }

    [ObservableProperty] private decimal _amount;

    public BulkReceiptLineItem(OutstandingSalesOrderCheckItem source)
    {
        SalesOrderId   = source.SalesOrderId;
        DocumentNumber = source.DocumentNumber;
        CustomerId     = source.Order.CustomerId;
        CustomerCode   = source.CustomerCode;
        CustomerName   = source.CustomerName;
        MaxAmount      = source.RemainingAmount;
        _amount        = source.RemainingAmount;
    }
}
