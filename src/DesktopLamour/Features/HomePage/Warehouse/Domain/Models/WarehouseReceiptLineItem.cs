// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Domain.Models;

public partial class WarehouseReceiptLineItem : ObservableObject
{
    [ObservableProperty] private ISearchableItem? _selectedProduct;
    [ObservableProperty] private decimal          _quantity    = 1;
    [ObservableProperty] private decimal          _unitPrice;
    [ObservableProperty] private decimal          _amount;
    [ObservableProperty] private string           _debitAccount  = "111";
    [ObservableProperty] private string           _creditAccount = "131";

    partial void OnQuantityChanged(decimal value)
        => Amount = value * UnitPrice;

    partial void OnUnitPriceChanged(decimal value)
        => Amount = Quantity * value;
}
