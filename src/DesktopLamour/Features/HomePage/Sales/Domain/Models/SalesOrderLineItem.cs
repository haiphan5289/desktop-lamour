// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderLineItem : INotifyPropertyChanged
{
    private int              _productId;
    private string           _productCode       = "";
    private string           _productName       = "";
    private bool             _isPromotion;
    private string           _unit              = "";
    private int              _quantity;
    private decimal          _unitPrice;
    private decimal          _discountRate;
    private decimal          _amount;
    private decimal          _taxRate;
    private decimal          _taxAmount;
    private string           _receivableAccount = "131";
    private string           _revenueAccount    = "511";
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

    public bool IsPromotion
    {
        get => _isPromotion;
        set
        {
            _isPromotion = value;
            OnPropertyChanged();

            if (value)
            {
                // Hàng khuyến mại: đơn giá/CK/thuế luôn = 0 (BE cũng ép lại giá trị này khi Ghi sổ).
                UnitPrice    = 0m;
                DiscountRate = 0m;
                TaxRate      = 0m;
            }
            else if (_selectedProduct is Product p)
            {
                // Bỏ tick khuyến mại: khôi phục đơn giá/thuế như lúc mới chọn sản phẩm.
                UnitPrice = p.SellingPrice;
                TaxRate   = SalesOrderTaxCalculator.ToPercent(p.VatRate);
            }
        }
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

    public decimal TaxRate
    {
        get => _taxRate;
        set { _taxRate = value; OnPropertyChanged(); RecalculateTax(); }
    }

    public decimal TaxAmount
    {
        get => _taxAmount;
        set { _taxAmount = value; OnPropertyChanged(); }
    }

    public string ReceivableAccount
    {
        get => _receivableAccount;
        set { _receivableAccount = value; OnPropertyChanged(); }
    }

    public string RevenueAccount
    {
        get => _revenueAccount;
        set { _revenueAccount = value; OnPropertyChanged(); }
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
                TaxRate     = SalesOrderTaxCalculator.ToPercent(p.VatRate);
            }
        }
    }

    // Sets SelectedProduct without triggering auto-fill (used when loading from saved order)
    public void SetSelectedProductSilent(ISearchableItem? product)
    {
        _selectedProduct = product;
        OnPropertyChanged(nameof(SelectedProduct));
    }

    private void RecalculateAmount()
    {
        Amount = Quantity * UnitPrice * (1 - Math.Max(0, Math.Min(100, DiscountRate)) / 100m);
        RecalculateTax();
    }

    private void RecalculateTax() =>
        TaxAmount = Amount * Math.Max(0, TaxRate) / 100m;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
