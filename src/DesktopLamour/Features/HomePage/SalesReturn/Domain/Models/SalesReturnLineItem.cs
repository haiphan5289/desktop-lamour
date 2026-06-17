// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;

public class SalesReturnLineItem : INotifyPropertyChanged
{
    private int              _productId;
    private string           _productCode       = "";
    private string           _productName       = "";
    private string           _returnAccount     = "5212";
    private string           _debtAccount       = "131";
    private string           _discountAccount   = "5211";
    private string           _unit              = "";
    private int              _quantity;
    private decimal          _unitPrice;
    private decimal          _discountRate;
    private decimal          _amount;
    private decimal          _discountAmount;
    private string?          _salesOrderNumber;
    private ISearchableItem? _selectedProduct;

    public int ProductId
    {
        get => _productId;
        set { _productId = value; OnPropertyChanged(); }
    }

    public string ProductCode
    {
        get => _productCode;
        set { _productCode = value; OnPropertyChanged(); }
    }

    public string ProductName
    {
        get => _productName;
        set { _productName = value; OnPropertyChanged(); }
    }

    public string ReturnAccount
    {
        get => _returnAccount;
        set { _returnAccount = value; OnPropertyChanged(); }
    }

    public string DebtAccount
    {
        get => _debtAccount;
        set { _debtAccount = value; OnPropertyChanged(); }
    }

    public string DiscountAccount
    {
        get => _discountAccount;
        set { _discountAccount = value; OnPropertyChanged(); }
    }

    public string Unit
    {
        get => _unit;
        set { _unit = value; OnPropertyChanged(); }
    }

    public int Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); RecalculateAmount(); }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set { _unitPrice = value; OnPropertyChanged(); RecalculateAmount(); }
    }

    public decimal DiscountRate
    {
        get => _discountRate;
        set { _discountRate = value; OnPropertyChanged(); RecalculateAmount(); }
    }

    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
    }

    public decimal DiscountAmount
    {
        get => _discountAmount;
        set { _discountAmount = value; OnPropertyChanged(); }
    }

    public string? SalesOrderNumber
    {
        get => _salesOrderNumber;
        set { _salesOrderNumber = value; OnPropertyChanged(); }
    }

    public ISearchableItem? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged();

            if (value is Product p)
            {
                ProductId   = p.Id;
                ProductCode = p.Code;
                ProductName = p.Name;
                Unit        = p.Unit;
                UnitPrice   = p.SellingPrice;
            }
        }
    }

    public void SetSelectedProductSilent(ISearchableItem? product)
    {
        _selectedProduct = product;
        OnPropertyChanged(nameof(SelectedProduct));
    }

    private void RecalculateAmount()
    {
        Amount         = Quantity * UnitPrice;
        DiscountAmount = Amount * Math.Max(0, Math.Min(100, DiscountRate)) / 100m;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
