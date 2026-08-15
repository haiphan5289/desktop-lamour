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
        HoldCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<SalesOrderLineItem> Lines { get; } = new();

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<ISearchableItem> Products { get; } = new();
    public IReadOnlyList<ISearchableItem> Warehouses { get; private set; } = Array.Empty<ISearchableItem>();
    private readonly List<ISearchableItem> _allProducts = new();
    private List<ISearchableItem> _depositPickerItems = new();

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
        _employeeFormWindowFactory  = employeeFormWindowFactory;
        _customerFormWindowFactory  = customerFormWindowFactory;
        _printWindowFactory         = printWindowFactory;
        _logger                     = logger;

        Lines.CollectionChanged += (_, _) => OnLinesOrTotalsChanged();
    }

    // Recalc tổng tiền + re-evaluate PrintCommand (in được ngay khi đã có dữ liệu sản phẩm,
    // kể cả chứng từ chưa Ghi sổ) mỗi khi dòng thêm/bớt hoặc 1 field trên dòng đổi.
    private void OnLinesOrTotalsChanged()
    {
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
                _nextDocumentNumber = await _getNextCode.ExecuteAsync(ct);
                CurrentOrder        = null;
                ClearForm();
            }
            else
            {
                CurrentOrder = order;
                PopulateFormFromCurrent();
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

    [RelayCommand]
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

        if (Lines.Count(l => !l.IsDepositDeductionRow) == 0)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng nhập ít nhất một mặt hàng.";
            return;
        }

        // Dòng Trừ cọc chỉ được coi là "có ý định trừ" khi user đã chọn cọc VÀ nhập số tiền
        // (Amount != 0) — nếu dòng tự động thêm nhưng user bỏ trống thì bỏ qua, không lỗi.
        var depositLine = Lines.FirstOrDefault(l =>
            l.IsDepositDeductionRow && l.LinkedDeposit is not null && l.Amount != 0);

        if (depositLine is not null)
        {
            var deductAmount = Math.Abs(depositLine.Amount);
            if (deductAmount > depositLine.LinkedDeposit!.RemainingBalance)
            {
                HasError     = true;
                ErrorMessage = "Số tiền trừ cọc vượt quá số dư còn lại của cọc đã chọn.";
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

            if (depositLine is not null)
            {
                try
                {
                    await _createDepositDeduction.ExecuteAsync(new CreateDepositDeductionRequestDto
                    {
                        DepositId      = depositLine.LinkedDeposit!.Id,
                        SalesOrderId   = result.Id,
                        Amount         = Math.Abs(depositLine.Amount),
                        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
                        Description    = $"Trừ cọc thanh toán đơn {result.DocumentNumber}",
                    }, ct);
                    _logger.LogInformation("DepositDeduction created for SalesOrder {Id}", result.Id);
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

            ShowPrintPreview(result);

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

    private void ShowPrintPreview(SalesOrderResponseDto order)
    {
        var customer = SelectedCustomer as DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer;
        var printWindow = _printWindowFactory();
        printWindow.Initialize(order, customer?.Phone, customer?.Address);
        printWindow.ShowDialog();
    }

    [RelayCommand]
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

    [RelayCommand]
    private void Cancel()
    {
        HasError = false;
        if (CurrentOrder is null)
            ClearForm();
        else
            PopulateFormFromCurrent();
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
        var line = new SalesOrderLineItem
        {
            ReceivableAccount = "131",
            RevenueAccount    = "511",
            Quantity          = 1,
        };
        line.SetSelectedWarehouseSilent(Warehouses.FirstOrDefault());
        line.PropertyChanged += (_, _) => OnLinesOrTotalsChanged();
        Lines.Add(line);
    }

    [RelayCommand]
    private void RemoveLine(SalesOrderLineItem line)
    {
        Lines.Remove(line);
        RecalculateTotals();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    partial void OnSelectedCustomerChanged(ISearchableItem? value)
    {
        if (value is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer c)
        {
            Description = $"Bán hàng {c.Name}";
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
            AvailableDeposits   = Array.Empty<DepositResponseDto>();
            _depositPickerItems = new List<ISearchableItem>();
            OnPropertyChanged(nameof(AvailableDeposits));
            ResetProductFilter();
        }
    }

    private async Task LoadAvailableDepositsAsync(int customerId)
    {
        try
        {
            var deposits = await _getDepositsByCustomer.ExecuteAsync(customerId);
            AvailableDeposits = deposits.ToList().AsReadOnly();
            OnPropertyChanged(nameof(AvailableDeposits));

            // Mỗi cọc còn số dư trở thành 1 "sản phẩm ảo" chọn được trong dropdown Mã hàng/Tên hàng.
            _depositPickerItems = AvailableDeposits
                .Select(d => (ISearchableItem)new DepositProductPickerItem(d))
                .ToList();
            ResetProductFilter();
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
        SelectedEmployee = null;
        Description      = null;
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
        RecalculateTotals();
    }

    private string GenerateNextDocumentNumber() => _nextDocumentNumber;

    private void PopulateFormFromCurrent()
    {
        if (CurrentOrder is null) return;

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentOrder.CustomerId);
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
            item.PropertyChanged += (_, _) => OnLinesOrTotalsChanged();
            Lines.Add(item);
        }

        RecalculateTotals();
    }

    public void FilterProductsByCode(string? text)
    {
        var filteredProducts = string.IsNullOrWhiteSpace(text)
            ? _allProducts
            : _allProducts.Where(p => p.Code.Contains(text, StringComparison.OrdinalIgnoreCase));
        var filteredDeposits = string.IsNullOrWhiteSpace(text)
            ? _depositPickerItems
            : _depositPickerItems.Where(d => d.Code.Contains(text, StringComparison.OrdinalIgnoreCase));
        RefreshProducts(filteredDeposits.Concat(filteredProducts));
    }

    public void FilterProductsByName(string? text)
    {
        var filteredProducts = string.IsNullOrWhiteSpace(text)
            ? _allProducts
            : _allProducts.Where(p => p.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
        var filteredDeposits = string.IsNullOrWhiteSpace(text)
            ? _depositPickerItems
            : _depositPickerItems.Where(d => d.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
        RefreshProducts(filteredDeposits.Concat(filteredProducts));
    }

    // "Trừ cọc" luôn hiển thị ở đầu danh sách (trước sản phẩm thật) khi khách hàng có cọc khả dụng.
    public void ResetProductFilter()
    {
        RefreshProducts(_depositPickerItems.Concat(_allProducts));
    }

    private void RefreshProducts(IEnumerable<ISearchableItem> items)
    {
        Products.Clear();
        foreach (var p in items) Products.Add(p);
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
        LineSummary    = $"Số dòng = {Lines.Count(l => !l.IsDepositDeductionRow)}";
    }

    private CreateSalesOrderRequestDto BuildCreateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
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
        Lines          = Lines.Where(l => !l.IsDepositDeductionRow).Select(ToLineDto).ToList(),
    };

    private UpdateSalesOrderRequestDto BuildUpdateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
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
        Lines          = Lines.Where(l => !l.IsDepositDeductionRow).Select(ToLineDto).ToList(),
    };

    private static SalesOrderLineDto ToLineDto(SalesOrderLineItem item) => new()
    {
        ProductId         = item.ProductId,
        WarehouseId       = item.WarehouseId,
        ProductCode       = item.ProductCode,
        ProductName       = item.ProductName,
        IsPromotion       = item.IsPromotion,
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

    [RelayCommand(CanExecute = nameof(HasExistingOrder))]
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
    private bool CanPrint => Lines.Any(l => l.ProductId > 0);

    [RelayCommand(CanExecute = nameof(CanPrint))]
    private void Print()
    {
        if (!CanPrint) return;
        ShowPrintPreview(BuildPreviewOrderDto());
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
            CustomerName   = customer?.Name ?? "",
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
            Lines          = Lines.Where(l => !l.IsDepositDeductionRow).Select(ToLineDto).ToList(),
        };
    }
}
