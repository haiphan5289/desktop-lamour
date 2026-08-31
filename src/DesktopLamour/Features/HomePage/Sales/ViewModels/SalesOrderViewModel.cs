// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderViewModel : ViewModelBase
{
    // Số dòng trống nạp sẵn khi mở chứng từ mới — xem ClearForm().
    private const int InitialEmptyLineCount = 100;

    // Kho mặc định khi Thêm chứng từ mới — khớp Kho ngầm định "HH" dùng cho vật tư hàng hoá
    // (xem ProductFormViewModel.DefaultWarehouseCode) thay vì lấy đại kho đầu tiên trong danh sách.
    private const string DefaultWarehouseCode = "HH";

    // Set bởi SalesOrderWindow.Initialize khi popup mở từ Kho → Xuất Kho ("Phiếu Xuất") — dùng để
    // auto-fill Diễn giải mặc định "Xuất kho bán hàng" (ClearForm) và tắt auto-fill "Bán hàng {tên
    // KH}" theo khách hàng (OnSelectedCustomerChanged) vốn chỉ hợp lý khi mở từ module Bán hàng.
    public bool IsFromWarehouseExport { private get; set; }

    // Set bởi SalesOrderWindow.Initialize khi mở popup ở chế độ chỉ xem (double-click 1 dòng từ
    // "Sổ chi tiết bán hàng") — disable toàn bộ field nhập liệu + ẩn các nút hành động thay đổi
    // dữ liệu (Ghi sổ/Xóa/Treo/Hoàn), chỉ giữ In Hoá Đơn/Đóng.
    [ObservableProperty] private bool _isReadOnly;

    partial void OnIsReadOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(ShowHoldSection));
        OnPropertyChanged(nameof(HeaderSubtitle));
        EditCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        HoldCommand.NotifyCanExecuteChanged();
    }

    public bool IsEditable => !IsReadOnly;
    public bool ShowHoldSection => HasExistingOrder && !IsReadOnly;
    public string HeaderSubtitle => IsReadOnly
        ? "Xem chi tiết chứng từ (chỉ đọc)"
        : "Bán hàng hóa, dịch vụ trong nước chưa thu tiền";

    // ── Điều hướng Trước/Sau/Thêm/Sửa trong popup — gọi từ SalesOrderWindow.Initialize khi mở từ
    // 1 danh sách (SalesOrderListViewModel). Không set (mặc định rỗng) → CanNavigatePrev/Next luôn
    // false, nút Trước/Sau disable (mờ) chứ không ẩn hẳn khỏi toolbar.
    private IReadOnlyList<SalesOrderResponseDto> _siblingOrders = Array.Empty<SalesOrderResponseDto>();
    private int _siblingIndex = -1;

    // CanExecute cho NavigatePrev/NavigateNextCommand.
    public bool CanNavigatePrev => _siblingIndex > 0;
    public bool CanNavigateNext => _siblingIndex >= 0 && _siblingIndex < _siblingOrders.Count - 1;

    private void NotifyNavigationChanged()
    {
        NavigatePrevCommand.NotifyCanExecuteChanged();
        NavigateNextCommand.NotifyCanExecuteChanged();
    }

    public void SetSiblingContext(IReadOnlyList<SalesOrderResponseDto> siblings, int currentIndex)
    {
        _siblingOrders = siblings;
        _siblingIndex  = currentIndex;
        NotifyNavigationChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrev))]
    private async Task NavigatePrevAsync(CancellationToken ct = default)
    {
        if (_siblingIndex <= 0 || !ConfirmDiscardIfDirty()) return;
        _siblingIndex--;
        await InitializeAsync(_siblingOrders[_siblingIndex], ct);
        NotifyNavigationChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private async Task NavigateNextAsync(CancellationToken ct = default)
    {
        if (_siblingIndex < 0 || _siblingIndex >= _siblingOrders.Count - 1 || !ConfirmDiscardIfDirty()) return;
        _siblingIndex++;
        await InitializeAsync(_siblingOrders[_siblingIndex], ct);
        NotifyNavigationChanged();
    }

    [RelayCommand]
    private async Task AddNewAsync(CancellationToken ct = default)
    {
        if (!ConfirmDiscardIfDirty()) return;
        // Chứng từ mới chưa có trong danh sách anh/em — bỏ vị trí cũ, giữ nguyên tinh thần
        // Payment/ReceiptViewModel.AddNew() (đặt _currentIndex = -1) để Trước/Sau tự disable cho tới
        // khi chứng từ này được Ghi sổ và mở lại từ danh sách.
        _siblingIndex = -1;
        IsReadOnly    = false;
        await InitializeAsync(null, ct);
        NotifyNavigationChanged();
    }

    private bool CanEdit => IsReadOnly;

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit() => IsReadOnly = false;

    // Dùng chung cho Trước/Sau/Thêm — cảnh báo mất dữ liệu chưa lưu, khớp text đã dùng ở
    // SalesOrderWindow.OnClosing.
    private bool ConfirmDiscardIfDirty()
    {
        if (!IsDirty) return true;
        var r = MessageBox.Show(
            "Dữ liệu chưa lưu sẽ bị mất nếu tiếp tục. Bạn có chắc muốn tiếp tục?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        return r == MessageBoxResult.Yes;
    }

    public event Action? OrderSaved;
    public event Action? RequestClose;

    private readonly ICreateSalesOrderUseCase       _createOrder;
    private readonly IUpdateSalesOrderUseCase       _updateOrder;
    private readonly IDeleteSalesOrderUseCase       _deleteOrder;
    private readonly IHoldSalesOrderUseCase         _holdOrder;
    private readonly IGetNextSalesOrderCodeUseCase  _getNextCode;
    private readonly IGetCustomersUseCase           _getCustomers;
    private readonly IGetEmployeesUseCase           _getEmployees;
    private readonly IGetProductsUseCase            _getProducts;
    private readonly IGetWarehouseSettingsUseCase   _getWarehouses;
    private readonly IGetDepositsByCustomerUseCase  _getDepositsByCustomer;
    private readonly ICreateDepositDeductionUseCase _createDepositDeduction;
    private readonly IGetDepositDeductionsUseCase   _getDepositDeductions;
    private readonly Func<EmployeeFormWindow>       _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>       _customerFormWindowFactory;
    private readonly Func<SalesOrderPrintWindow>    _printWindowFactory;
    private readonly ILogger<SalesOrderViewModel>   _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Header — Thông tin chung ──────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    // "Tên Khách hàng" — mặc định = Customer.Name khi chọn "Mã số khách hàng", nhưng cho gõ đè
    // tuỳ ý (không đổi CustomerId/công nợ). Gửi lên BE làm CustomerNameOverride, in hoá đơn dùng
    // giá trị này thay Customer.Name thật. Xem AppSuggestTextBox — control tự viết riêng cho field
    // này, gợi ý theo tên nhưng không tự trả về tên cũ khi rời ô như AppSearchableComboBox.
    [ObservableProperty] private string?          _customerNameText;
    // "Địa chỉ" — cùng cơ chế CustomerNameText: mặc định = Customer.Address, cho sửa tự do, gửi
    // BE làm CustomerAddressOverride, in hoá đơn dùng giá trị này. Không cần gợi ý/search (chỉ là
    // AppTextField thường) — khác CustomerNameText.
    [ObservableProperty] private string?          _customerAddressText;
    [ObservableProperty] private string?          _description;
    [ObservableProperty] private string?          _reference;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    // ── Điều khoản thanh toán ─────────────────────────────────────────────
    [ObservableProperty] private string?   _paymentTerms;
    [ObservableProperty] private int?      _paymentDueDays;
    [ObservableProperty] private DateTime? _paymentDueDate;

    // ── Chứng từ ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "XK00001";
    // Placeholder ô "Số chứng từ" — mặc định "BH" (mở từ module Bán hàng), đổi thành "XK" khi
    // popup được mở từ luồng Kho → Xuất Kho (xem SalesOrderWindow.Initialize). Chỉ là gợi ý hiển
    // thị lúc ô còn trống — không ảnh hưởng số chứng từ thật (luôn sinh dạng "XK{5 digits}").
    [ObservableProperty] private string   _documentNumberPlaceholder = "BH00001";

    // ── Thông tin bổ sung (Tab 6) ─────────────────────────────────────────
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private string? _deliveryMethod;
    [ObservableProperty] private string? _paymentMethod;

    // ── Trừ cọc — hiển thị dưới dạng 1 dòng đặc biệt ở đầu Lines (Tab 1) ────
    public IReadOnlyList<DepositResponseDto> AvailableDeposits { get; private set; } = Array.Empty<DepositResponseDto>();

    // ── Computed ──────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _totalAmount;    // Tổng tiền hàng (gross)
    [ObservableProperty] private decimal _totalDiscount;  // Tổng tiền chiết khấu
    [ObservableProperty] private decimal _totalPayment;   // Tổng tiền thanh toán (chưa thuế)
    [ObservableProperty] private decimal _totalTaxAmount; // Tổng tiền thuế
    [ObservableProperty] private decimal _grandTotal;     // TotalPayment + TotalTaxAmount
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private SalesOrderResponseDto? _currentOrder;
    [ObservableProperty] private string _statusLabel = "📄 Ghi sổ";

    partial void OnCurrentOrderChanged(SalesOrderResponseDto? value)
    {
        StatusLabel = value?.Status switch { 1 => "⏸ Treo", _ => "📄 Ghi sổ" };
        OnPropertyChanged(nameof(HasExistingOrder));
        OnPropertyChanged(nameof(ShowHoldSection));
        HoldCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<SalesOrderLineItem> Lines { get; } = new();

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<ISearchableItem> Products { get; } = new();
    public IReadOnlyList<ISearchableItem> Warehouses { get; private set; } = Array.Empty<ISearchableItem>();
    private readonly List<ISearchableItem> _allProducts = new();

    private string _nextDocumentNumber = "XK00001";

    public SalesOrderViewModel(
        ICreateSalesOrderUseCase       createOrder,
        IUpdateSalesOrderUseCase       updateOrder,
        IDeleteSalesOrderUseCase       deleteOrder,
        IHoldSalesOrderUseCase         holdOrder,
        IGetNextSalesOrderCodeUseCase  getNextCode,
        IGetCustomersUseCase           getCustomers,
        IGetEmployeesUseCase           getEmployees,
        IGetProductsUseCase            getProducts,
        IGetWarehouseSettingsUseCase   getWarehouses,
        IGetDepositsByCustomerUseCase  getDepositsByCustomer,
        ICreateDepositDeductionUseCase createDepositDeduction,
        IGetDepositDeductionsUseCase   getDepositDeductions,
        Func<EmployeeFormWindow>       employeeFormWindowFactory,
        Func<CustomerFormWindow>       customerFormWindowFactory,
        Func<SalesOrderPrintWindow>    printWindowFactory,
        ILogger<SalesOrderViewModel>   logger)
    {
        _createOrder                = createOrder;
        _updateOrder                = updateOrder;
        _deleteOrder                = deleteOrder;
        _holdOrder                  = holdOrder;
        _getNextCode                = getNextCode;
        _getCustomers               = getCustomers;
        _getEmployees               = getEmployees;
        _getProducts                = getProducts;
        _getWarehouses              = getWarehouses;
        _getDepositsByCustomer      = getDepositsByCustomer;
        _createDepositDeduction     = createDepositDeduction;
        _getDepositDeductions       = getDepositDeductions;
        _employeeFormWindowFactory  = employeeFormWindowFactory;
        _customerFormWindowFactory  = customerFormWindowFactory;
        _printWindowFactory         = printWindowFactory;
        _logger                     = logger;

        Lines.CollectionChanged += (_, _) => OnLinesOrTotalsChanged();
    }

    // Recalc tổng tiền + re-evaluate PrintCommand (in được ngay khi đã có dữ liệu sản phẩm,
    // kể cả chứng từ chưa Ghi sổ) mỗi khi dòng thêm/bớt hoặc 1 field trên dòng đổi. Đồng thời xoá
    // banner lỗi cũ (ví dụ "không đủ tồn kho") — lỗi đó gắn với trạng thái Lines LÚC Ghi sổ thất
    // bại, sửa/xoá dòng xong mà banner còn hiển thị y nguyên (dù đã đúng) gây hiểu nhầm là chưa
    // sửa được. Ghi sổ lần sau (SaveAsync) tự validate lại và set lại HasError nếu vẫn còn lỗi.
    private void OnLinesOrTotalsChanged()
    {
        HasError     = false;
        ErrorMessage = string.Empty;
        RecalculateTotals();
        PrintCommand.NotifyCanExecuteChanged();
    }

    // ── Public init — called by SalesOrderWindow ──────────────────────────

    public async Task InitializeAsync(SalesOrderResponseDto? order, CancellationToken ct = default)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            await LoadLookupsAsync(ct);

            if (order is null)
            {
                _nextDocumentNumber = await _getNextCode.ExecuteAsync(IsFromWarehouseExport, ct);
                CurrentOrder        = null;
                ClearForm();
            }
            else
            {
                CurrentOrder = order;
                await PopulateFormFromCurrentAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SalesOrderViewModel");
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsBusy = false; }

        BeginDirtyTracking();
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        var customerTask  = _getCustomers.ExecuteAsync(ct);
        var employeeTask  = _getEmployees.ExecuteAsync(ct);
        var productTask   = _getProducts.ExecuteAsync(ct);
        var warehouseTask = _getWarehouses.ExecuteAsync(ct);

        await Task.WhenAll(customerTask, employeeTask, productTask, warehouseTask);

        if (warehouseTask.IsCompletedSuccessfully)
        {
            Warehouses = warehouseTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Warehouses));
        }
        else
            _logger.LogWarning(warehouseTask.Exception, "Could not preload warehouses for SalesOrderWindow");

        if (customerTask.IsCompletedSuccessfully)
        {
            Customers = customerTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Customers));
        }
        else
            _logger.LogWarning(customerTask.Exception, "Could not preload customers for SalesOrderWindow");

        if (employeeTask.IsCompletedSuccessfully)
        {
            Employees = employeeTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
        }
        else
            _logger.LogWarning(employeeTask.Exception, "Could not preload employees for SalesOrderWindow");

        if (productTask.IsCompletedSuccessfully)
        {
            _allProducts.Clear();
            _allProducts.AddRange(productTask.Result.Where(p => p.IsActive).Cast<ISearchableItem>());
            ResetProductFilter();
        }
        else
            _logger.LogWarning(productTask.Exception, "Could not preload products for SalesOrderWindow");
    }

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(IsEditable))]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (SelectedCustomer is null)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng chọn khách hàng.";
            return;
        }

        // Chứng từ hợp lệ nếu có ít nhất 1 mặt hàng thật HOẶC 1 dòng Trừ cọc có ý định trừ (chứng
        // từ "chỉ trừ cọc", không kèm sản phẩm — dùng khi cần số chứng từ dạng XK cho lần trừ đó
        // thay vì số TC từ màn Đặt Cọc/Trừ Cọc riêng). BE (CreateSalesOrderUseCase) đã bỏ ràng buộc
        // "ít nhất 1 dòng" tương ứng — validate ở đây là chốt chặn duy nhất.
        var hasProductLine = Lines.Any(l => !l.IsDepositDeductionRow && l.ProductId > 0);
        var hasDepositLine = Lines.Any(l => l.IsDepositDeductionRow && l.Amount != 0);
        if (!hasProductLine && !hasDepositLine)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng nhập ít nhất một mặt hàng hoặc chọn Trừ cọc.";
            return;
        }

        // Dòng Trừ cọc chỉ được coi là "có ý định trừ" khi user đã chọn "Trừ cọc" VÀ nhập số tiền
        // (Amount != 0) — nếu dòng tự động thêm nhưng user bỏ trống thì bỏ qua, không lỗi.
        var depositLine = Lines.FirstOrDefault(l => l.IsDepositDeductionRow && l.Amount != 0);

        // Validate tạm ở client theo AvailableDepositBalance chụp lúc chọn "Trừ cọc" — BE mới là
        // nơi quyết định thật (tự phân bổ FIFO qua nhiều Deposit, xem CreateDepositDeductionUseCase).
        if (depositLine is not null)
        {
            var deductAmount = Math.Abs(depositLine.Amount);
            if (deductAmount > depositLine.AvailableDepositBalance)
            {
                HasError     = true;
                ErrorMessage = "Số tiền trừ cọc vượt quá tổng số dư cọc còn lại của khách hàng.";
                return;
            }
        }

        IsBusy = true;
        try
        {
            SalesOrderResponseDto result;
            if (CurrentOrder is null)
            {
                var request = BuildCreateRequest();
                result = await _createOrder.ExecuteAsync(request, ct);
                _logger.LogInformation("SalesOrder created: {DocumentNumber}", result.DocumentNumber);
            }
            else
            {
                var request = BuildUpdateRequest();
                result = await _updateOrder.ExecuteAsync(CurrentOrder.Id, request, ct);
                _logger.LogInformation("SalesOrder updated: {Id}", result.Id);
            }

            var depositDeductionAmountForPrint = 0m;
            if (depositLine is not null)
            {
                try
                {
                    await _createDepositDeduction.ExecuteAsync(new CreateDepositDeductionRequestDto
                    {
                        SalesOrderId   = result.Id,
                        Amount         = Math.Abs(depositLine.Amount),
                        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
                        Description    = $"Trừ cọc thanh toán đơn {result.DocumentNumber}",
                    }, ct);
                    _logger.LogInformation("DepositDeduction created for SalesOrder {Id}", result.Id);
                    // Chỉ hiển thị dòng "Trừ Cọc" trên hóa đơn in nếu deduction thật sự đã lưu ở BE —
                    // nếu lỗi (catch bên dưới), hóa đơn không nên hiển thị 1 khoản trừ chưa từng tồn tại.
                    depositDeductionAmountForPrint = Math.Abs(depositLine.Amount);
                }
                catch (Exception depositEx)
                {
                    _logger.LogError(depositEx, "Failed to create deposit deduction for SalesOrder {Id}", result.Id);
                    MessageBox.Show(
                        $"Đơn hàng đã được ghi sổ nhưng trừ cọc thất bại: {depositEx.Message}",
                        "Lỗi trừ cọc", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            StopDirtyTracking();
            OrderSaved?.Invoke();
            IsBusy = false;

            ShowPrintPreview(result, depositDeductionAmountForPrint);

            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save sales order");
            HasError     = true;
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Không thể ghi sổ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    private void ShowPrintPreview(SalesOrderResponseDto order, decimal depositDeductionAmount = 0m)
    {
        var customer = SelectedCustomer as DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer;
        var printWindow = _printWindowFactory();
        // Tên khách hàng bị gõ đè khác tên thật (CustomerNameOverride có giá trị) → ẩn SĐT trên
        // hoá đơn, vì SĐT thật không còn khớp với tên đang hiển thị. Địa chỉ dùng order.CustomerAddress
        // (đã resolve override) thay vì lấy thẳng customer?.Address.
        var phone = string.IsNullOrWhiteSpace(order.CustomerNameOverride) ? customer?.Phone : null;
        printWindow.Initialize(order, phone, order.CustomerAddress, depositDeductionAmount);
        printWindow.ShowDialog();
    }

    [RelayCommand(CanExecute = nameof(IsEditable))]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentOrder is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{CurrentOrder.DocumentNumber}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _deleteOrder.ExecuteAsync(CurrentOrder.Id, ct);
            _logger.LogInformation("SalesOrder deleted: {Id}", CurrentOrder.Id);
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete sales order");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(IsEditable))]
    private async Task CancelAsync(CancellationToken ct = default)
    {
        HasError = false;
        if (CurrentOrder is null)
            ClearForm();
        else
            await PopulateFormFromCurrentAsync(ct);
    }

    [RelayCommand]
    private async Task AddEmployeeAsync(CancellationToken ct = default)
    {
        var before = Employees.Select(e => e.Id).ToHashSet();
        var window = _employeeFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var employees = await _getEmployees.ExecuteAsync(ct);
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
            var newItem = Employees.FirstOrDefault(e => !before.Contains(e.Id));
            if (newItem is not null) SelectedEmployee = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload employees after add"); }
    }

    [RelayCommand]
    private async Task AddCustomerAsync(CancellationToken ct = default)
    {
        var before = Customers.Select(c => c.Id).ToHashSet();
        var window = _customerFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            Customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Customers));
            var newItem = Customers.FirstOrDefault(c => !before.Contains(c.Id));
            if (newItem is not null) SelectedCustomer = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload customers after add"); }
    }

    [RelayCommand]
    private void AddLine()
    {
        var line = new SalesOrderLineItem();
        AttachLineHandlers(line);
        Lines.Add(line);
    }

    // Dòng mới phải thực sự rỗng (không Kho/TK/Số lượng mặc định hiển thị sẵn) — các field TK/Số
    // lượng tự điền khi user chọn 1 sản phẩm thật (xem SalesOrderLineItem.SelectedProduct). Riêng
    // Kho không có logic tự điền trong model đó (model không biết danh sách Warehouses), nên set
    // mặc định ở đây ngay khi ProductId chuyển từ 0 → có giá trị, để tránh warehouse_id rỗng khi
    // Ghi sổ (dòng Đặt cọc thì ProductId luôn = 0 nên không bị set nhầm). Dùng chung cho dòng mới
    // (AddLine) và dòng nạp lại từ chứng từ đã lưu (PopulateFormFromCurrentAsync) — cả 2 nơi đều
    // cần gợi ý Thành tiền khi user đổi 1 dòng sang chọn "Trừ cọc".
    private void AttachLineHandlers(SalesOrderLineItem line)
    {
        line.PropertyChanged += (_, e) =>
        {
            OnLinesOrTotalsChanged();

            if (e.PropertyName == nameof(SalesOrderLineItem.ProductId) && line.ProductId > 0)
            {
                if (line.IsDepositDeductionRow)
                    // Vừa chọn product "Trừ cọc" (mã SalesOrderLineItem.TruCocProductCode) cho dòng
                    // này — không auto-fill Kho (dòng này bị loại khỏi payload gửi BE), thay vào đó
                    // lấy tổng số dư cọc khả dụng của khách hàng đang chọn để gợi ý Thành tiền (bắt ở
                    // block AvailableDepositBalance bên dưới, giống cách TruCocPickerItem cũ làm qua
                    // constructor).
                    line.AvailableDepositBalance = AvailableDeposits.Sum(d => d.RemainingBalance);
                else if (line.WarehouseId == 0)
                    line.SetSelectedWarehouseSilent(
                        Warehouses.FirstOrDefault(w => w.Code == DefaultWarehouseCode) ?? Warehouses.FirstOrDefault());
            }

            // Vừa chọn "Trừ cọc" cho dòng này (Thành tiền còn 0, chưa gõ tay) — gợi ý sẵn số tiền
            // trừ = min(tổng số dư cọc khả dụng, tổng tiền chứng từ đang cần thanh toán). User vẫn
            // sửa lại được nếu muốn trừ ít hơn số gợi ý.
            if (e.PropertyName == nameof(SalesOrderLineItem.AvailableDepositBalance)
                && line.IsDepositDeductionRow && line.AvailableDepositBalance > 0 && line.Amount == 0)
            {
                var amountDue = TotalPayment + TotalTaxAmount;
                line.Amount = Math.Max(0, Math.Min(line.AvailableDepositBalance, amountDue));
            }
        };
    }

    [RelayCommand]
    private void RemoveLine(SalesOrderLineItem line)
    {
        if (line.IsLocked)
        {
            MessageBox.Show(
                "Dòng \"Trừ cọc\" này đã được ghi sổ, không thể xoá ở đây. Muốn hoàn lại khoản trừ cọc, hãy thực hiện qua màn Đặt Cọc/Trừ Cọc.",
                "Không thể xoá", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Lines.Remove(line);
        RecalculateTotals();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    partial void OnSelectedCustomerChanged(ISearchableItem? value)
    {
        if (value is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer c)
        {
            // CustomerNameText đổi → tự kích OnCustomerNameTextChanged → tự điền Diễn giải.
            CustomerNameText    = c.Name;
            CustomerAddressText = c.Address;
            if (c.SaleCareEmployeeId.HasValue)
            {
                var matched = Employees.FirstOrDefault(e => e.Id == c.SaleCareEmployeeId.Value);
                if (matched is not null)
                    SelectedEmployee = matched;
            }
        }

        if (value is not null)
            _ = LoadAvailableDepositsAsync(value.Id);
        else
        {
            AvailableDeposits = Array.Empty<DepositResponseDto>();
            OnPropertyChanged(nameof(AvailableDeposits));
        }
    }

    // "Tên Khách hàng" đổi (do chọn "Mã số khách hàng" HOẶC do user gõ đè tự do) → luôn đè lại
    // Diễn giải theo tên mới nhất, khớp hành vi cũ của OnSelectedCustomerChanged.
    partial void OnCustomerNameTextChanged(string? value)
    {
        if (IsFromWarehouseExport || string.IsNullOrWhiteSpace(value)) return;
        Description = $"Bán hàng {value}";
    }

    // User bấm chọn 1 gợi ý trong AppSuggestTextBox ("Tên Khách hàng") — đồng bộ lại "Mã số khách
    // hàng" (SelectedCustomer/CustomerId) về đúng khách hàng đó, không chỉ đổi text hiển thị.
    [RelayCommand]
    private void CustomerNameSuggestionPicked(ISearchableItem item)
    {
        if (SelectedCustomer?.Id != item.Id)
            SelectedCustomer = item;
    }

    private async Task LoadAvailableDepositsAsync(int customerId)
    {
        try
        {
            // Loại cọc do chính chứng từ đang sửa tạo ra (SourceSalesOrderId == CurrentOrder.Id) —
            // không cho 1 đơn tự trừ cọc của chính nó. CurrentOrder null khi tạo đơn mới → không lọc.
            var deposits = await _getDepositsByCustomer.ExecuteAsync(customerId, CurrentOrder?.Id);
            AvailableDeposits = deposits.ToList().AsReadOnly();
            OnPropertyChanged(nameof(AvailableDeposits));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load available deposits for customer {CustomerId}", customerId);
        }
    }

    partial void OnPaymentDueDaysChanged(int? value)
    {
        if (value.HasValue && value > 0)
            PaymentDueDate = DocumentDate.AddDays(value.Value);
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        CustomerNameText    = null;
        CustomerAddressText = null;
        SelectedEmployee = null;
        Description      = IsFromWarehouseExport ? "Xuất kho bán hàng" : null;
        Reference        = null;
        PaymentTerms     = null;
        PaymentDueDays   = null;
        PaymentDueDate   = null;
        AccountingDate   = DateTime.Today;
        DocumentDate     = DateTime.Today;
        DocumentNumber   = GenerateNextDocumentNumber();
        Notes            = null;
        DeliveryMethod   = null;
        PaymentMethod    = null;
        Lines.Clear();
        // Chứng từ mới: nạp sẵn N dòng trống để user gõ liền, không cần bấm "Thêm dòng" trước
        // (button footer đã bỏ — Thêm dòng giờ chỉ còn qua context menu chuột phải/Ctrl+Insert).
        // Dòng trống (ProductId=0) bị lọc bỏ khi Lưu/tính Số dòng, không gửi lên BE nếu bỏ trống.
        for (var i = 0; i < InitialEmptyLineCount; i++) AddLine();
        RecalculateTotals();
    }

    private string GenerateNextDocumentNumber() => _nextDocumentNumber;

    private async Task PopulateFormFromCurrentAsync(CancellationToken ct = default)
    {
        if (CurrentOrder is null) return;

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentOrder.CustomerId);
        // Đè lại sau SelectedCustomer — tránh bị OnSelectedCustomerChanged/OnCustomerNameTextChanged
        // tự điền đè mất giá trị override + Diễn giải đã lưu ở BE.
        CustomerNameText    = CurrentOrder.CustomerNameOverride ?? CurrentOrder.CustomerName;
        CustomerAddressText = CurrentOrder.CustomerAddressOverride ?? CurrentOrder.CustomerAddress;
        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == CurrentOrder.EmployeeId);
        Description      = CurrentOrder.Description;
        Reference        = CurrentOrder.Reference;
        PaymentTerms     = CurrentOrder.PaymentTerms;
        PaymentDueDays   = CurrentOrder.PaymentDueDays;
        PaymentDueDate   = CurrentOrder.PaymentDueDate?.ToLocalTime();
        AccountingDate   = CurrentOrder.AccountingDate.ToLocalTime();
        DocumentDate     = CurrentOrder.DocumentDate.ToLocalTime();
        DocumentNumber   = CurrentOrder.DocumentNumber;
        Notes            = CurrentOrder.Notes;
        DeliveryMethod   = CurrentOrder.DeliveryMethod;
        PaymentMethod    = CurrentOrder.PaymentMethod;

        Lines.Clear();
        foreach (var l in CurrentOrder.Lines)
        {
            var item = new SalesOrderLineItem
            {
                ProductId         = l.ProductId,
                ProductCode       = l.ProductCode,
                ProductName       = l.ProductName,
                IsPromotion       = l.IsPromotion,
                IsDepositProduct  = l.IsDepositProduct,
                Unit              = l.Unit,
                Quantity          = l.Quantity,
                UnitPrice         = l.UnitPrice,
                DiscountRate      = l.DiscountRate,
                TaxRate           = l.TaxRate,
                TaxAmount         = l.TaxAmount,
                ReceivableAccount = l.ReceivableAccount,
                RevenueAccount    = l.RevenueAccount,
            };
            // Nạp Thành tiền + cờ thủ công sau cùng — tránh bị Quantity/UnitPrice/DiscountRate
            // ở trên tự tính lại đè mất giá trị đã lưu từ BE.
            item.LoadAmount(l.Amount, l.IsAmountManual);
            item.SetSelectedProductSilent(_allProducts.FirstOrDefault(p => p.Id == l.ProductId));
            item.SetSelectedWarehouseSilent(Warehouses.FirstOrDefault(w => w.Id == l.WarehouseId));
            AttachLineHandlers(item);
            Lines.Add(item);
        }

        // Nạp lại "Trừ cọc" đã ghi sổ ở BE (DepositDeduction, không phải SalesOrderLine) — nếu
        // không, GrandTotal tính lại ở dưới sẽ là tổng TRƯỚC khi trừ cọc (khớp cách BE lưu
        // SalesOrder.GrandTotal — xem CreateSalesOrderUseCase — nhưng sai với số khách thực trả).
        // Dòng nạp lại này bị khoá (IsLocked) — không cho sửa/xoá để tránh gọi lại
        // CreateDepositDeductionUseCase lúc Lưu và tạo bản ghi trừ cọc trùng lặp.
        try
        {
            var deductions = await _getDepositDeductions.ExecuteAsync(
                customerId: null, employeeId: null, salesOrderId: CurrentOrder.Id,
                fromDate: null, toDate: null, ct);

            // 1 lần trừ cọc trên đơn này có thể đã được BE phân bổ FIFO qua nhiều Deposit → nhiều
            // DepositDeduction record. Gộp lại thành đúng 1 dòng hiển thị (khớp UI "1 chỗ trừ cọc").
            var deductionList = deductions.ToList();
            if (deductionList.Count > 0)
            {
                var lockedLine = new SalesOrderLineItem
                {
                    IsDepositDeductionRow = true,
                    IsLocked              = true,
                    ProductCode           = "",
                    ProductName           = "Trừ cọc",
                };
                lockedLine.LoadAmount(-deductionList.Sum(d => d.Amount), isAmountManual: false);
                Lines.Add(lockedLine);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not reload deposit deductions for SalesOrder {Id}", CurrentOrder.Id);
        }

        RecalculateTotals();
    }

    // AppSearchableComboBox tự lọc theo Code/Name khi user gõ (xem PopulateFiltered) nên Products
    // chỉ cần giữ đúng danh sách đầy đủ — không cần lọc lại mỗi keystroke như ComboBox cũ. Dòng "Trừ
    // cọc" chọn qua product thật (mã SalesOrderLineItem.TruCocProductCode) nằm sẵn trong _allProducts
    // như mọi sản phẩm khác — không còn entry ảo chèn riêng vào đầu danh sách.
    public void ResetProductFilter()
    {
        Products.Clear();
        foreach (var item in _allProducts)
            Products.Add(item);
    }

    private void RecalculateTotals()
    {
        var productLines = Lines.Where(l => !l.IsDepositDeductionRow);
        var gross      = productLines.Sum(l => (decimal)l.Quantity * l.UnitPrice);
        TotalAmount    = gross;
        TotalDiscount  = productLines.Sum(l => (decimal)l.Quantity * l.UnitPrice * Math.Max(0, Math.Min(100, l.DiscountRate)) / 100m);
        TotalPayment   = gross - TotalDiscount;
        TotalTaxAmount = productLines.Sum(l => l.TaxAmount);
        // Số tiền trừ cọc (đã lưu dạng âm) cộng thẳng vào Tổng thanh toán để phản ánh đúng
        // số tiền khách còn phải trả — không ảnh hưởng TotalAmount/TotalPayment/TotalTaxAmount
        // vốn phải khớp với cách BE tính GrandTotal của SalesOrder (chỉ từ dòng sản phẩm thật).
        var depositDeduction = Lines.Where(l => l.IsDepositDeductionRow).Sum(l => l.Amount);
        GrandTotal     = TotalPayment + TotalTaxAmount + depositDeduction;
        LineSummary    = $"Số dòng = {Lines.Count(l => !l.IsDepositDeductionRow && l.ProductId > 0)}";
    }

    // null = không override, hiển thị/in luôn theo Customer.Name thật (tự đổi theo nếu khách hàng
    // được đổi tên sau này). Chỉ gửi lên BE khi CustomerNameText thực sự khác tên thật đang chọn.
    private string? ResolveCustomerNameOverride()
    {
        var text = CustomerNameText?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (SelectedCustomer is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer c
            && string.Equals(text, c.Name, StringComparison.Ordinal))
            return null;
        return text;
    }

    // Cùng logic ResolveCustomerNameOverride nhưng cho Địa chỉ.
    private string? ResolveCustomerAddressOverride()
    {
        var text = CustomerAddressText?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (SelectedCustomer is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer c
            && string.Equals(text, c.Address, StringComparison.Ordinal))
            return null;
        return text;
    }

    private CreateSalesOrderRequestDto BuildCreateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        CustomerNameOverride    = ResolveCustomerNameOverride(),
        CustomerAddressOverride = ResolveCustomerAddressOverride(),
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description)    ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)      ? null : Reference.Trim(),
        PaymentTerms   = string.IsNullOrWhiteSpace(PaymentTerms)   ? null : PaymentTerms.Trim(),
        PaymentDueDays = PaymentDueDays,
        PaymentDueDate = PaymentDueDate.HasValue
            ? DateTime.SpecifyKind(PaymentDueDate.Value.Date, DateTimeKind.Unspecified)
            : null,
        Notes          = string.IsNullOrWhiteSpace(Notes)          ? null : Notes.Trim(),
        DeliveryMethod = string.IsNullOrWhiteSpace(DeliveryMethod) ? null : DeliveryMethod.Trim(),
        PaymentMethod  = string.IsNullOrWhiteSpace(PaymentMethod)  ? null : PaymentMethod.Trim(),
        Lines          = Lines.Where(l => !l.IsDepositDeductionRow && l.ProductId > 0).Select(ToLineDto).ToList(),
    };

    private UpdateSalesOrderRequestDto BuildUpdateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        CustomerNameOverride    = ResolveCustomerNameOverride(),
        CustomerAddressOverride = ResolveCustomerAddressOverride(),
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description)    ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)      ? null : Reference.Trim(),
        PaymentTerms   = string.IsNullOrWhiteSpace(PaymentTerms)   ? null : PaymentTerms.Trim(),
        PaymentDueDays = PaymentDueDays,
        PaymentDueDate = PaymentDueDate.HasValue
            ? DateTime.SpecifyKind(PaymentDueDate.Value.Date, DateTimeKind.Unspecified)
            : null,
        Notes          = string.IsNullOrWhiteSpace(Notes)          ? null : Notes.Trim(),
        DeliveryMethod = string.IsNullOrWhiteSpace(DeliveryMethod) ? null : DeliveryMethod.Trim(),
        PaymentMethod  = string.IsNullOrWhiteSpace(PaymentMethod)  ? null : PaymentMethod.Trim(),
        Lines          = Lines.Where(l => !l.IsDepositDeductionRow && l.ProductId > 0).Select(ToLineDto).ToList(),
    };

    private static SalesOrderLineDto ToLineDto(SalesOrderLineItem item) => new()
    {
        ProductId         = item.ProductId,
        WarehouseId       = item.WarehouseId,
        ProductCode       = item.ProductCode,
        ProductName       = item.ProductName,
        IsPromotion       = item.IsPromotion,
        IsDepositProduct  = item.IsDepositProduct,
        Unit              = item.Unit,
        Quantity          = item.Quantity,
        UnitPrice         = item.UnitPrice,
        DiscountRate      = item.DiscountRate,
        Amount            = item.Amount,
        IsAmountManual    = item.IsAmountManual,
        TaxRate           = item.TaxRate,
        TaxAmount         = item.TaxAmount,
        ReceivableAccount = item.ReceivableAccount,
        RevenueAccount    = item.RevenueAccount,
    };

    // ── Hold ──────────────────────────────────────────────────────────────────
    public bool HasExistingOrder => CurrentOrder is not null;

    // ShowHoldSection (= HasExistingOrder && !IsReadOnly) thay vì chỉ HasExistingOrder — Treo
    // không có ý nghĩa ở chế độ chỉ xem, disable (mờ) thay vì ẩn hẳn như trước.
    [RelayCommand(CanExecute = nameof(ShowHoldSection))]
    private async Task HoldAsync(CancellationToken ct = default)
    {
        if (CurrentOrder is null) return;
        IsBusy = true;
        try
        {
            var updated = await _holdOrder.ExecuteAsync(CurrentOrder.Id, ct);
            CurrentOrder = updated;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Treo đơn thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    // ── Print ─────────────────────────────────────────────────────────────────
    // In được ngay khi đã có ít nhất 1 dòng đã chọn sản phẩm — không bắt buộc phải Ghi sổ trước
    // (khác với Treo, vốn chỉ áp dụng cho chứng từ đã tồn tại trên BE).
    private bool CanPrint => Lines.Any(l => l.ProductId > 0) || Lines.Any(l => l.IsDepositDeductionRow && l.Amount != 0);

    [RelayCommand(CanExecute = nameof(CanPrint))]
    private void Print()
    {
        if (!CanPrint) return;
        var depositLine = Lines.FirstOrDefault(l => l.IsDepositDeductionRow && l.Amount != 0);
        ShowPrintPreview(BuildPreviewOrderDto(), depositLine is null ? 0m : Math.Abs(depositLine.Amount));
    }

    // Dựng SalesOrderResponseDto để in preview từ đúng dữ liệu đang hiển thị trên form (kể cả
    // chưa lưu) — không gọi BE. Nếu đã có CurrentOrder (đã Ghi sổ), vẫn ưu tiên dữ liệu form hiện
    // tại (có thể đang sửa dở) thay vì bản đã lưu cũ, chỉ giữ lại Id/CreatedAt/Status từ bản đã lưu.
    private SalesOrderResponseDto BuildPreviewOrderDto()
    {
        var customer = SelectedCustomer as DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer;
        var employee = SelectedEmployee as DesktopLamour.Features.HomePage.Employees.Domain.Models.Employee;

        return new SalesOrderResponseDto
        {
            Id             = CurrentOrder?.Id ?? 0,
            DocumentNumber = DocumentNumber.Trim(),
            AccountingDate = AccountingDate,
            DocumentDate   = DocumentDate,
            CustomerId     = customer?.Id ?? 0,
            // Ưu tiên CustomerNameText/CustomerAddressText (có thể đã bị user gõ đè) — không lấy
            // thẳng customer?.Name/Address. CustomerNameOverride cần có ở đây để ShowPrintPreview
            // biết có nên ẩn SĐT không, dù đây là DTO dựng tạm (chưa lưu BE).
            CustomerName   = string.IsNullOrWhiteSpace(CustomerNameText) ? (customer?.Name ?? "") : CustomerNameText,
            CustomerNameOverride    = ResolveCustomerNameOverride(),
            CustomerAddress = string.IsNullOrWhiteSpace(CustomerAddressText) ? (customer?.Address ?? "") : CustomerAddressText,
            CustomerAddressOverride = ResolveCustomerAddressOverride(),
            EmployeeId     = employee?.Id,
            EmployeeName   = employee?.Name,
            Description    = Description,
            Reference      = Reference,
            PaymentTerms   = PaymentTerms,
            PaymentDueDays = PaymentDueDays,
            PaymentDueDate = PaymentDueDate,
            Notes          = Notes,
            DeliveryMethod = DeliveryMethod,
            PaymentMethod  = PaymentMethod,
            TotalAmount    = TotalPayment,
            TotalTaxAmount = TotalTaxAmount,
            GrandTotal     = GrandTotal,
            CreatedAt      = CurrentOrder?.CreatedAt ?? DateTime.UtcNow,
            Status         = CurrentOrder?.Status ?? 0,
            Lines          = Lines.Where(l => !l.IsDepositDeductionRow && l.ProductId > 0).Select(ToLineDto).ToList(),
        };
    }
}
