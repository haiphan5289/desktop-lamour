// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public partial class WarehouseReceiptLineItem : ObservableObject
{
    [ObservableProperty] private ISearchableItem? _selectedProduct;
    [ObservableProperty] private decimal          _quantity;
    [ObservableProperty] private decimal          _unitPrice;
    [ObservableProperty] private decimal          _amount;
    [ObservableProperty] private string           _debitAccount  = string.Empty;
    [ObservableProperty] private string           _creditAccount = string.Empty;

    [ObservableProperty] private string _costItem            = string.Empty;
    [ObservableProperty] private string _costObject          = string.Empty;
    [ObservableProperty] private string _project              = string.Empty;
    [ObservableProperty] private string _purchaseOrderNumber = string.Empty;
    [ObservableProperty] private string _salesContractNumber = string.Empty;
    [ObservableProperty] private string _loanContractNumber  = string.Empty;
    [ObservableProperty] private string _statisticsCode      = string.Empty;

    // Dòng mới phải thực sự rỗng (không Số lượng/TK mặc định hiển thị sẵn) — Số lượng/TK Nợ/TK Có
    // chỉ tự điền khi user chọn 1 sản phẩm thật, giống pattern SelectedProduct của SalesOrderLineItem.
    partial void OnSelectedProductChanged(ISearchableItem? value)
    {
        if (value is WarehouseProductItem p)
        {
            UnitPrice     = p.CostPrice;
            Quantity      = 1;
            DebitAccount  = "111";
            CreditAccount = "131";
        }
    }

    partial void OnQuantityChanged(decimal value)
        => Amount = value * UnitPrice;

    partial void OnUnitPriceChanged(decimal value)
        => Amount = Quantity * value;
}
