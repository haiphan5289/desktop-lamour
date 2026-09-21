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

    // Dòng "Trừ cọc" (sản phẩm mã TruCocProductCode) — là 1 SalesOrderLine bình thường, gửi lên BE và
    // lưu cùng đơn như mọi sản phẩm (không còn liên quan số dư cọc/DepositDeduction). Khác dòng thường
    // ở chỗ: user gõ số dương, Amount tự lưu thành số ÂM để trừ vào tổng đơn.
    public bool IsDepositDeductionRow
    {
        get => _isDepositDeductionRow;
        set { _isDepositDeductionRow = value; OnPropertyChanged(); }
    }

    // Luôn false — còn giữ để khớp DataTrigger "DisabledWhenLocked" trong SalesOrderWindow.xaml.
    public bool IsLocked
    {
        get => _isLocked;
        set { _isLocked = value; OnPropertyChanged(); }
    }
    private bool _isLocked;

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
                // Raise DisplayAmount để ô nhập format nghìn SỐNG theo phím (giống Đơn giá/SL). An
                // toàn không lặp vô hạn vì DisplayAmount getter trả Math.Abs(_amount) — Convert(abs)
                // round-trip sạch với ConvertBack, không còn lật dấu làm text phân kỳ như trước.
                // Việc "footer chỉ đổi sau khi gõ xong" xử lý ở SalesOrderViewModel.AttachLineHandlers
                // (hoãn RecalculateTotals cho dòng Trừ cọc tới lúc CellEditEnding).
                OnPropertyChanged(nameof(DisplayAmount));
                OnPropertyChanged(nameof(IsNegativeAmount));
                // Thành tiền nhập tay → BE dùng thẳng số này (Quantity × Đơn giá của dòng này luôn = 0).
                SetIsAmountManual(true);
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

    // Ô NHẬP "Thành tiền" (CellEditingTemplate) bind vào đây. Với dòng Trừ cọc, getter trả về số
    // DƯƠNG (Math.Abs) — đúng số user gõ, không bao giờ bị chèn dấu "-" giữa lúc đang gõ; Amount
    // setter tự lật thành âm để cộng dồn vào GrandTotal. Dòng sản phẩm thường: DisplayAmount == Amount.
    // Ô HIỂN THỊ sau commit (CellTemplate) bind thẳng Amount + AccountingAmountConverter để hiện
    // "(1.814.400)" đỏ cho dòng Trừ cọc.
    public decimal DisplayAmount
    {
        get => _isDepositDeductionRow ? Math.Abs(_amount) : _amount;
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
                // Chọn product "Trừ cọc" (mã TruCocProductCode) → dòng chỉ có Thành tiền (số âm), không
                // kho/đơn giá/thuế — như sản phẩm cọc (IsDepositProduct), lưu như 1 dòng hàng thường.
                IsDepositDeductionRow = true;
                IsDepositProduct      = true;
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
            }
            else if (value is Product p2)
            {
                // Chọn lại 1 sản phẩm thật bình thường → khôi phục dòng về trạng thái bình thường.
                IsDepositDeductionRow   = false;
                IsDepositProduct        = p2.IsDepositProduct;
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
