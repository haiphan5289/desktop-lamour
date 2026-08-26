// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;

public class SalesReturnLineItem : INotifyPropertyChanged
{
    private int              _productId;
    private string           _productCode       = "";
    private string           _productName       = "";
    private int              _warehouseId;
    private string           _warehouseName     = "";
    private ISearchableItem? _selectedWarehouse;
    private string           _returnAccount     = "5212";
    private string           _debtAccount       = "131";
    private string           _discountAccount   = "5211";
    private ISearchableItem? _selectedReturnAccount;
    private ISearchableItem? _selectedDebtAccount;
    private ISearchableItem? _selectedDiscountAccount;
    private string           _unit              = "";
    private int              _quantity;
    private decimal          _unitPrice;
    private decimal          _discountRate;
    private decimal          _amount;
    private decimal          _discountAmount;
    private string?          _salesOrderNumber;
    private ISearchableItem? _selectedProduct;

    private decimal          _taxRate;
    private decimal          _taxAmount;
    private string           _taxAccount        = "33311";
    private ISearchableItem? _selectedTaxAccount;

    private string           _costAccount       = "1561";
    private string           _cogsAccount       = "632";
    private ISearchableItem? _selectedCostAccount;
    private ISearchableItem? _selectedCogsAccount;
    private decimal          _costPrice;
    private decimal          _costAmount;

    private int?             _departmentId;
    private ISearchableItem? _selectedDepartment;

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

    public int WarehouseId
    {
        get => _warehouseId;
        set { _warehouseId = value; OnPropertyChanged(); }
    }

    public string WarehouseName
    {
        get => _warehouseName;
        set { _warehouseName = value; OnPropertyChanged(); }
    }

    public ISearchableItem? SelectedWarehouse
    {
        get => _selectedWarehouse;
        set
        {
            _selectedWarehouse = value;
            OnPropertyChanged();
            WarehouseId   = value?.Id ?? 0;
            WarehouseName = value?.Name ?? "";
        }
    }

    public void SetSelectedWarehouseSilent(ISearchableItem? warehouse) => SelectedWarehouse = warehouse;

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

    // Combo tra cứu danh mục tài khoản (AccountSetting) cho 3 cột TK trong DataGrid — chỉ ghi đè
    // ReturnAccount/DebtAccount/DiscountAccount (mã gửi lên BE, không đổi) khi tìm thấy item khớp;
    // set null (không tìm thấy mã trong danh mục) sẽ KHÔNG xoá mã hiện có, tránh mất dữ liệu.
    public ISearchableItem? SelectedReturnAccount
    {
        get => _selectedReturnAccount;
        set
        {
            _selectedReturnAccount = value;
            OnPropertyChanged();
            if (value is not null) ReturnAccount = value.Code;
        }
    }

    public void SetSelectedReturnAccountSilent(ISearchableItem? account) => SelectedReturnAccount = account;

    public ISearchableItem? SelectedDebtAccount
    {
        get => _selectedDebtAccount;
        set
        {
            _selectedDebtAccount = value;
            OnPropertyChanged();
            if (value is not null) DebtAccount = value.Code;
        }
    }

    public void SetSelectedDebtAccountSilent(ISearchableItem? account) => SelectedDebtAccount = account;

    public ISearchableItem? SelectedDiscountAccount
    {
        get => _selectedDiscountAccount;
        set
        {
            _selectedDiscountAccount = value;
            OnPropertyChanged();
            if (value is not null) DiscountAccount = value.Code;
        }
    }

    public void SetSelectedDiscountAccountSilent(ISearchableItem? account) => SelectedDiscountAccount = account;

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
                // Tab "2. Thuế"/"3. Giá vốn" — denormalize từ Product, giống hệt cách BE tự tính lại
                // khi ghi sổ (không cho user sửa tay % thuế/đơn giá vốn, tránh lệch dữ liệu).
                TaxRate     = SalesOrderTaxCalculator.ToPercent(p.VatRate);
                CostPrice   = p.CostPrice;
            }
        }
    }

    public void SetSelectedProductSilent(ISearchableItem? product)
    {
        _selectedProduct = product;
        OnPropertyChanged(nameof(SelectedProduct));
    }

    // Tab "2. Thuế" — TaxRate luôn lấy từ Product.VatRate (xem SelectedProduct), không cho gõ tay;
    // BE cũng tự tính lại, bỏ qua giá trị client gửi, giống hệt SalesOrder.
    public decimal TaxRate
    {
        get => _taxRate;
        set { _taxRate = value; OnPropertyChanged(); RecalculateAmount(); }
    }

    public decimal TaxAmount
    {
        get => _taxAmount;
        set { _taxAmount = value; OnPropertyChanged(); }
    }

    public string TaxAccount
    {
        get => _taxAccount;
        set { _taxAccount = value; OnPropertyChanged(); }
    }

    public ISearchableItem? SelectedTaxAccount
    {
        get => _selectedTaxAccount;
        set
        {
            _selectedTaxAccount = value;
            OnPropertyChanged();
            if (value is not null) TaxAccount = value.Code;
        }
    }

    public void SetSelectedTaxAccountSilent(ISearchableItem? account) => SelectedTaxAccount = account;

    // Tab "3. Giá vốn"
    public string CostAccount
    {
        get => _costAccount;
        set { _costAccount = value; OnPropertyChanged(); }
    }

    public string CogsAccount
    {
        get => _cogsAccount;
        set { _cogsAccount = value; OnPropertyChanged(); }
    }

    public ISearchableItem? SelectedCostAccount
    {
        get => _selectedCostAccount;
        set
        {
            _selectedCostAccount = value;
            OnPropertyChanged();
            if (value is not null) CostAccount = value.Code;
        }
    }

    public void SetSelectedCostAccountSilent(ISearchableItem? account) => SelectedCostAccount = account;

    public ISearchableItem? SelectedCogsAccount
    {
        get => _selectedCogsAccount;
        set
        {
            _selectedCogsAccount = value;
            OnPropertyChanged();
            if (value is not null) CogsAccount = value.Code;
        }
    }

    public void SetSelectedCogsAccountSilent(ISearchableItem? account) => SelectedCogsAccount = account;

    // Đơn giá vốn lấy từ Product.CostPrice (xem SelectedProduct) — không cho gõ tay, BE cũng tự
    // tính lại từ Product tại thời điểm ghi sổ.
    public decimal CostPrice
    {
        get => _costPrice;
        set { _costPrice = value; OnPropertyChanged(); RecalculateAmount(); }
    }

    public decimal CostAmount
    {
        get => _costAmount;
        set { _costAmount = value; OnPropertyChanged(); }
    }

    // Tab "4. Thống kê" — chỉ Đơn vị (Department); 6 field còn lại của MISA không có master data,
    // bỏ qua theo yêu cầu.
    public int? DepartmentId
    {
        get => _departmentId;
        set { _departmentId = value; OnPropertyChanged(); }
    }

    public ISearchableItem? SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            _selectedDepartment = value;
            OnPropertyChanged();
            DepartmentId = value?.Id;
        }
    }

    public void SetSelectedDepartmentSilent(ISearchableItem? department) => SelectedDepartment = department;

    private void RecalculateAmount()
    {
        Amount         = Quantity * UnitPrice;
        DiscountAmount = Amount * Math.Max(0, Math.Min(100, DiscountRate)) / 100m;
        TaxAmount      = (Amount - DiscountAmount) * TaxRate / 100m;
        CostAmount     = Quantity * CostPrice;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
