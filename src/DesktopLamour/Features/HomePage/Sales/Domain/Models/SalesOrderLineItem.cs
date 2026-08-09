// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
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
    private bool             _isAmountManual;
    private decimal          _taxRate;
    private decimal          _taxAmount;
    private string           _receivableAccount = "131";
    private string           _revenueAccount    = "511";
    private ISearchableItem? _selectedProduct;
    private bool              _isDepositDeductionRow;
    private DepositResponseDto? _linkedDeposit;

    // Dòng ảo "Trừ cọc" — không phải sản phẩm thật, không gửi lên BE như 1 SalesOrderLine.
    public bool IsDepositDeductionRow
    {
        get => _isDepositDeductionRow;
        set { _isDepositDeductionRow = value; OnPropertyChanged(); }
    }

    // Cọc được chọn để trừ cho dòng này — chỉ có ý nghĩa khi IsDepositDeductionRow = true.
    // Hiển thị y hệt 1 dòng sản phẩm bình thường: Mã hàng = số chứng từ cọc, Tên hàng = "Trừ cọc".
    public DepositResponseDto? LinkedDeposit
    {
        get => _linkedDeposit;
        set
        {
            _linkedDeposit = value;
            OnPropertyChanged();
            if (value is not null)
            {
                ProductCode = value.DocumentNumber;
                ProductName = "Trừ cọc";
            }
        }
    }

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
        set { _quantity = value; OnPropertyChanged(); ResetManualAndRecalculate(); }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set { _unitPrice = value; OnPropertyChanged(); ResetManualAndRecalculate(); }
    }

    public decimal DiscountRate
    {
        get => _discountRate;
        set { _discountRate = value; OnPropertyChanged(); ResetManualAndRecalculate(); }
    }

    // Gõ tay trực tiếp vào ô Thành tiền (UI binding) → dòng chuyển sang chế độ thủ công,
    // BE sẽ dùng thẳng giá trị này thay vì tự tính Quantity×UnitPrice×(1-CK%). Đơn giá được
    // tính ngược lại từ Thành tiền + CK% hiện có để hiển thị đơn giá tương ứng cho user tham khảo.
    public decimal Amount
    {
        get => _amount;
        set
        {
            // Dòng Trừ cọc: user luôn gõ số dương, tự động lưu thành số âm để cộng dồn
            // đúng vào GrandTotal ở RecalculateTotals — không áp dụng logic thành tiền thủ công
            // của dòng sản phẩm (Quantity=0 nên back-calculate Đơn giá vô nghĩa với dòng này).
            if (_isDepositDeductionRow)
            {
                _amount = -Math.Abs(value);
                OnPropertyChanged();
                RecalculateTax();
                return;
            }

            _amount = value;
            OnPropertyChanged();
            SetIsAmountManual(true);
            TryBackCalculateUnitPrice();
            RecalculateTax();
        }
    }

    // Đơn giá = Thành tiền ÷ (SL × (1-CK%/100)). Không chia được (SL=0, CK%=100%) hoặc
    // Thành tiền vẫn = 0 (chưa nhập) thì giữ nguyên Đơn giá hiện có.
    private void TryBackCalculateUnitPrice()
    {
        var factor = Quantity * (1 - Math.Max(0, Math.Min(100, DiscountRate)) / 100m);
        if (Amount == 0m || factor == 0m) return;
        _unitPrice = Amount / factor;
        OnPropertyChanged(nameof(UnitPrice));
    }

    public bool IsAmountManual => _isAmountManual;

    private void SetIsAmountManual(bool value)
    {
        if (_isAmountManual == value) return;
        _isAmountManual = value;
        OnPropertyChanged(nameof(IsAmountManual));
    }

    // Nạp Thành tiền từ chứng từ đã lưu (BE) mà không kích hoạt chế độ thủ công ngoài ý muốn.
    public void LoadAmount(decimal amount, bool isAmountManual)
    {
        _amount = amount;
        OnPropertyChanged(nameof(Amount));
        SetIsAmountManual(isAmountManual);
        RecalculateTax();
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

            if (value is DepositProductPickerItem depositItem)
            {
                // Chọn "Trừ cọc" trong dropdown sản phẩm → biến dòng này thành dòng Trừ cọc.
                IsDepositDeductionRow = true;
                ProductId             = 0;
                Unit                  = "";
                Quantity              = 0;
                UnitPrice             = 0;
                DiscountRate          = 0;
                TaxRate               = 0;
                ReceivableAccount     = "";
                RevenueAccount        = "";
                LinkedDeposit         = depositItem.Deposit; // set sau cùng — tự gán ProductCode/ProductName
            }
            else if (value is Product p)
            {
                // Chọn lại 1 sản phẩm thật → khôi phục dòng về trạng thái bình thường.
                IsDepositDeductionRow = false;
                LinkedDeposit         = null;
                ProductId   = p.Id;
                ProductCode = p.Code;
                ProductName = p.Name;
                Unit        = p.Unit;
                Quantity    = Quantity == 0 ? 1 : Quantity;
                UnitPrice   = p.SellingPrice;
                ReceivableAccount = string.IsNullOrEmpty(ReceivableAccount) ? "131" : ReceivableAccount;
                RevenueAccount    = string.IsNullOrEmpty(RevenueAccount)    ? "511" : RevenueAccount;
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

    // Sửa Số lượng/Đơn giá/CK% luôn tắt chế độ Thành tiền thủ công và tính lại theo công thức.
    private void ResetManualAndRecalculate()
    {
        SetIsAmountManual(false);
        _amount = Quantity * UnitPrice * (1 - Math.Max(0, Math.Min(100, DiscountRate)) / 100m);
        OnPropertyChanged(nameof(Amount));
        RecalculateTax();
    }

    private void RecalculateTax() =>
        TaxAmount = Amount * Math.Max(0, TaxRate) / 100m;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
