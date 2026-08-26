// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

// 1 dòng trong popup "Thu tiền khách hàng hàng loạt" — bọc OutstandingSalesOrderDto + tick chọn.
public partial class OutstandingSalesOrderCheckItem : ObservableObject
{
    public OutstandingSalesOrderDto Order { get; }

    public int      SalesOrderId    => Order.SalesOrderId;
    public string   DocumentNumber  => Order.DocumentNumber;
    public DateTime AccountingDate  => Order.AccountingDate;
    public string   CustomerCode    => Order.CustomerCode;
    public string   CustomerName    => Order.CustomerName;
    public string?  Description     => Order.Description;
    public decimal  RemainingAmount => Order.RemainingAmount;
    public decimal  GrandTotal      => Order.GrandTotal;
    public string?  PaymentTerms    => Order.PaymentTerms;
    public DateTime? PaymentDueDate => Order.PaymentDueDate;

    [ObservableProperty] private bool _isSelected;

    public OutstandingSalesOrderCheckItem(OutstandingSalesOrderDto order) => Order = order;
}
