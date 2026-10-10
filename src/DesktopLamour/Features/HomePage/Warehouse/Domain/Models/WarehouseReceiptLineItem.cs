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
    // Kho nhập của dòng — chọn tay được; tự điền kho ngầm định của sản phẩm (hoặc HH) khi chọn sản phẩm.
    [ObservableProperty] private ISearchableItem? _selectedWarehouse;
    [ObservableProperty] private string           _debitAccount  = string.Empty;
    [ObservableProperty] private string           _creditAccount = string.Empty;

    [ObservableProperty] private string _costItem            = string.Empty;
    [ObservableProperty] private string _costObject          = string.Empty;
    [ObservableProperty] private string _project              = string.Empty;
    [ObservableProperty] private string _purchaseOrderNumber = string.Empty;
    [ObservableProperty] private string _salesContractNumber = string.Empty;
    [ObservableProperty] private string _loanContractNumber  = string.Empty;
    [ObservableProperty] private string _statisticsCode      = string.Empty;

    // Do ViewModel gán (nó giữ danh sách kho) — cho biết kho mặc định của 1 sản phẩm vừa được chọn.
    public Func<WarehouseProductItem, ISearchableItem?>? DefaultWarehouseResolver { get; set; }

    // Do ViewModel gán — tra mã TK ra mục trong danh mục tài khoản, để ô chọn TK (AppSearchableComboBox,
    // cùng control với Phiếu thu/Phiếu chi) bind SelectedItem trong khi dữ liệu lưu vẫn là mã (chuỗi).
    public Func<string, ISearchableItem?>? AccountResolver { get; set; }

    public ISearchableItem? SelectedDebitAccount
    {
        get => AccountResolver?.Invoke(DebitAccount);
        set { if (value is not null) DebitAccount = value.Code; }
    }

    public ISearchableItem? SelectedCreditAccount
    {
        get => AccountResolver?.Invoke(CreditAccount);
        set { if (value is not null) CreditAccount = value.Code; }
    }

    partial void OnDebitAccountChanged(string value)  => OnPropertyChanged(nameof(SelectedDebitAccount));
    partial void OnCreditAccountChanged(string value) => OnPropertyChanged(nameof(SelectedCreditAccount));

    // Dòng mới phải thực sự rỗng (không Số lượng/TK mặc định hiển thị sẵn) — Số lượng/TK Nợ/TK Có
    // chỉ tự điền khi user chọn 1 sản phẩm thật, giống pattern SelectedProduct của SalesOrderLineItem.
    partial void OnSelectedProductChanged(ISearchableItem? value)
    {
        if (value is WarehouseProductItem p)
        {
            UnitPrice     = p.CostPrice;
            Quantity      = 1;
            DebitAccount  = "1561";
            CreditAccount = "1112";
            SelectedWarehouse = DefaultWarehouseResolver?.Invoke(p);
        }
    }

    partial void OnQuantityChanged(decimal value)
        => Amount = value * UnitPrice;

    partial void OnUnitPriceChanged(decimal value)
        => Amount = Quantity * value;
}
