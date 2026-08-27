// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public class SalesOrderLineItem : INotifyPropertyChanged
{
    // Mã sản phẩm THẬT trong catalog đại diện cho dòng "Trừ cọc" (thay cho entry ảo TruCocPickerItem
    // trước đây) — chọn đúng product này trong picker sẽ biến dòng thành dòng Trừ cọc, y hệt hành vi
    // cũ. Xác nhận với người dùng 2026-08-27: dùng thẳng mã product cụ thể, không cần thêm cờ catalog
    // mới (IsDepositDeductionProduct) hay đổi BE — chỉ 1 product duy nhất đóng vai trò này.
    public const string TruCocProductCode = "36";

    private int              _productId;
    private string           _productCode       = "";
    private string           _productName       = "";
    private int              _warehouseId;
    private string           _warehouseName     = "";
    private ISearchableItem? _selectedWarehouse;
    private bool             _isPromotion;
    private bool             _isDepositProduct;
    private string           _unit              = "";
    private int              _quantity;
    private decimal          _unitPrice;
    private decimal          _discountRate;
    private decimal          _amount;
    private bool             _isAmountManual;
    private decimal          _taxRate;
    private decimal          _taxAmount;
    private string           _receivableAccount = "";
    private string           _revenueAccount    = "";
    private ISearchableItem? _selectedProduct;
    private bool              _isDepositDeductionRow;
    private decimal           _availableDepositBalance;

    // Dòng ảo "Trừ cọc" — không phải sản phẩm thật, không gửi lên BE như 1 SalesOrderLine.
    public bool IsDepositDeductionRow
    {
        get => _isDepositDeductionRow;
        set { _isDepositDeductionRow = value; OnPropertyChanged(); }
    }

    // Dòng "Trừ cọc" nạp lại từ 1 DepositDeduction đã ghi sổ ở BE (xem
    // SalesOrderViewModel.PopulateFormFromCurrentAsync) — chỉ hiển thị cho đúng tổng thanh toán,
    // không cho sửa/xóa để tránh gọi lại CreateDepositDeductionUseCase và tạo bản ghi trùng lặp.
    // Đổi khoản trừ cọc phải làm qua màn Đặt Cọc/Trừ Cọc riêng.
    public bool IsLocked
    {
        get => _isLocked;
        set { _isLocked = value; OnPropertyChanged(); }
    }
    private bool _isLocked;

    // Tổng số dư cọc khả dụng của khách hàng tại thời điểm chọn "Trừ cọc" — chỉ có ý nghĩa khi
    // IsDepositDeductionRow = true. Dùng để gợi ý/validate số tiền trừ ở client; BE mới là nơi
    // quyết định thật (tự phân bổ FIFO qua nhiều Deposit khi Ghi sổ — xem CreateDepositDeductionUseCase).
    public decimal AvailableDepositBalance
    {
        get => _availableDepositBalance;
        set { _availableDepositBalance = value; OnPropertyChanged(); }
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

    // Nạp Kho từ chứng từ đã lưu (BE) không qua setter công khai — không có side-effect gì khác
    // ngoài WarehouseId/WarehouseName nên dùng chung logic với SelectedWarehouse, chỉ tách tên
    // để rõ ràng khi gọi từ chỗ nạp dữ liệu (giống SetSelectedProductSilent).
    public void SetSelectedWarehouseSilent(ISearchableItem? warehouse) => SelectedWarehouse = warehouse;

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

    // Denormalized từ Product.IsDepositProduct lúc chọn sản phẩm — dùng để ẩn Đơn giá/CK/Thuế
    // suất trên hóa đơn in (xem SalesOrderPrintWindow), set trong SelectedProduct setter bên dưới.
    public bool IsDepositProduct
    {
        get => _isDepositProduct;
        set { _isDepositProduct = value; OnPropertyChanged(); }
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
                // User gõ số dương hay số âm đều ra cùng 1 kết quả âm (Math.Abs rồi tự phủ định) —
                // AccountingAmountConverter trên grid hiển thị lại dạng "(1.814.400)" sau khi commit.
                _amount = -Math.Abs(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayAmount));
                OnPropertyChanged(nameof(IsNegativeAmount));
                RecalculateTax();
                return;
            }

            _amount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayAmount));
            OnPropertyChanged(nameof(IsNegativeAmount));
            SetIsAmountManual(true);
            TryBackCalculateUnitPrice();
            RecalculateTax();
        }
    }

    // Ô "Thành tiền" trên grid bind vào đây thay vì thẳng Amount — tách riêng để dành chỗ cho hành vi
    // UI-only sau này nếu cần. Hiện tại DisplayAmount == Amount (giữ dấu âm thật của dòng Trừ cọc để
    // AccountingAmountConverter format dạng ngoặc "(xxx)" — không còn ẩn dấu âm bằng Math.Abs như
    // trước). Set lại đi qua Amount setter ở trên nên dòng Trừ cọc vẫn tự âm hoá đúng.
    public decimal DisplayAmount
    {
        get => _amount;
        set => Amount = value;
    }

    // Dùng để tô đỏ ô "Thành tiền" trên grid khi Amount âm (dòng Trừ cọc sau khi commit) — kiểu kế
    // toán quen thuộc, khớp với format ngoặc của AccountingAmountConverter.
    public bool IsNegativeAmount => _amount < 0m;

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
        OnPropertyChanged(nameof(DisplayAmount));
        OnPropertyChanged(nameof(IsNegativeAmount));
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

            if (value is Product p && p.Code == TruCocProductCode)
            {
                // Chọn product "Trừ cọc" thật (mã TruCocProductCode) → biến dòng này thành dòng Trừ
                // cọc, y hệt hành vi entry ảo trước đây. Không gắn với 1 Deposit cụ thể — BE tự phân
                // bổ FIFO qua nhiều Deposit khi Ghi sổ (xem CreateDepositDeductionUseCase).
                IsDepositDeductionRow = true;
                IsDepositProduct      = false;
                ProductId             = p.Id;
                ProductCode           = p.Code;
                ProductName           = p.Name;
                Unit                  = "";
                Quantity              = 0;
                UnitPrice             = 0;
                DiscountRate          = 0;
                TaxRate               = 0;
                ReceivableAccount     = "";
                RevenueAccount        = "";
                // AvailableDepositBalance được SalesOrderViewModel.AttachLineHandlers gán ngay sau
                // khi bắt PropertyChanged(ProductId) ở trên (ViewModel mới có tổng số dư cọc khách
                // hàng) — bắt AvailableDepositBalance đổi để gợi ý sẵn Thành tiền, giống cách cũ.
            }
            else if (value is Product p2)
            {
                // Chọn lại 1 sản phẩm thật bình thường → khôi phục dòng về trạng thái bình thường.
                IsDepositDeductionRow   = false;
                IsDepositProduct        = p2.IsDepositProduct;
                AvailableDepositBalance = 0;
                ProductId   = p2.Id;
                ProductCode = p2.Code;
                ProductName = p2.Name;
                Unit        = p2.Unit;
                Quantity    = Quantity == 0 ? 1 : Quantity;
                UnitPrice   = p2.SellingPrice;
                ReceivableAccount = string.IsNullOrEmpty(ReceivableAccount) ? "131" : ReceivableAccount;
                RevenueAccount    = string.IsNullOrEmpty(RevenueAccount)    ? "511" : RevenueAccount;
                TaxRate     = SalesOrderTaxCalculator.ToPercent(p2.VatRate);
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
        OnPropertyChanged(nameof(DisplayAmount));
        OnPropertyChanged(nameof(IsNegativeAmount));
        RecalculateTax();
    }

    private void RecalculateTax() =>
        TaxAmount = Amount * Math.Max(0, TaxRate) / 100m;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
