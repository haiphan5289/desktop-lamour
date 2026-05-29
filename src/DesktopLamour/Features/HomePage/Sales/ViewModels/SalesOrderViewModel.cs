// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
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
    private readonly IGetNextSalesOrderCodeUseCase  _getNextCode;
    private readonly IGetCustomersUseCase           _getCustomers;
    private readonly IGetEmployeesUseCase           _getEmployees;
    private readonly IGetProductsUseCase            _getProducts;
    private readonly Func<EmployeeFormWindow>       _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>       _customerFormWindowFactory;
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
    [ObservableProperty] private string   _documentNumber = "BC00001";

    // ── Thông tin bổ sung (Tab 6) ─────────────────────────────────────────
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private string? _deliveryMethod;
    [ObservableProperty] private string? _paymentMethod;

    // ── Computed ──────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _totalAmount;    // Tổng tiền hàng (gross)
    [ObservableProperty] private decimal _totalDiscount;  // Tổng tiền chiết khấu
    [ObservableProperty] private decimal _totalPayment;   // Tổng tiền thanh toán
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private SalesOrderResponseDto? _currentOrder;

    public ObservableCollection<SalesOrderLineItem> Lines { get; } = new();

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<ISearchableItem> Products { get; } = new();
    private readonly List<ISearchableItem> _allProducts = new();

    private string _nextDocumentNumber = "BC00001";

    public SalesOrderViewModel(
        ICreateSalesOrderUseCase       createOrder,
        IUpdateSalesOrderUseCase       updateOrder,
        IDeleteSalesOrderUseCase       deleteOrder,
        IGetNextSalesOrderCodeUseCase  getNextCode,
        IGetCustomersUseCase           getCustomers,
        IGetEmployeesUseCase           getEmployees,
        IGetProductsUseCase            getProducts,
        Func<EmployeeFormWindow>       employeeFormWindowFactory,
        Func<CustomerFormWindow>       customerFormWindowFactory,
        ILogger<SalesOrderViewModel>   logger)
    {
        _createOrder                = createOrder;
        _updateOrder                = updateOrder;
        _deleteOrder                = deleteOrder;
        _getNextCode                = getNextCode;
        _getCustomers               = getCustomers;
        _getEmployees               = getEmployees;
        _getProducts                = getProducts;
        _employeeFormWindowFactory  = employeeFormWindowFactory;
        _customerFormWindowFactory  = customerFormWindowFactory;
        _logger                     = logger;

        Lines.CollectionChanged += (_, _) => RecalculateTotals();
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
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        var customerTask = _getCustomers.ExecuteAsync(ct);
        var employeeTask = _getEmployees.ExecuteAsync(ct);
        var productTask  = _getProducts.ExecuteAsync(ct);

        await Task.WhenAll(customerTask, employeeTask, productTask);

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

        if (Lines.Count == 0)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng nhập ít nhất một mặt hàng.";
            return;
        }

        IsBusy = true;
        try
        {
            if (CurrentOrder is null)
            {
                var request = BuildCreateRequest();
                var result  = await _createOrder.ExecuteAsync(request, ct);
                _logger.LogInformation("SalesOrder created: {DocumentNumber}", result.DocumentNumber);
            }
            else
            {
                var request = BuildUpdateRequest();
                var result  = await _updateOrder.ExecuteAsync(CurrentOrder.Id, request, ct);
                _logger.LogInformation("SalesOrder updated: {Id}", result.Id);
            }

            OrderSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save sales order");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
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
        line.PropertyChanged += (_, _) => RecalculateTotals();
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
            Description = $"Bán hàng {c.Name}";
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
                Amount            = l.Amount,
                ReceivableAccount = l.ReceivableAccount,
                RevenueAccount    = l.RevenueAccount,
            };
            item.SetSelectedProductSilent(_allProducts.FirstOrDefault(p => p.Id == l.ProductId));
            item.PropertyChanged += (_, _) => RecalculateTotals();
            Lines.Add(item);
        }

        RecalculateTotals();
    }

    public void FilterProductsByCode(string? text)
    {
        var filtered = string.IsNullOrWhiteSpace(text)
            ? _allProducts
            : _allProducts.Where(p => p.Code.Contains(text, StringComparison.OrdinalIgnoreCase));
        RefreshProducts(filtered);
    }

    public void FilterProductsByName(string? text)
    {
        var filtered = string.IsNullOrWhiteSpace(text)
            ? _allProducts
            : _allProducts.Where(p => p.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
        RefreshProducts(filtered);
    }

    public void ResetProductFilter()
    {
        RefreshProducts(_allProducts);
    }

    private void RefreshProducts(IEnumerable<ISearchableItem> items)
    {
        Products.Clear();
        foreach (var p in items) Products.Add(p);
    }

    private void RecalculateTotals()
    {
        var gross    = Lines.Sum(l => (decimal)l.Quantity * l.UnitPrice);
        TotalAmount   = gross;
        TotalDiscount = Lines.Sum(l => (decimal)l.Quantity * l.UnitPrice * Math.Max(0, Math.Min(100, l.DiscountRate)) / 100m);
        TotalPayment  = gross - TotalDiscount;
        LineSummary   = $"Số dòng = {Lines.Count}";
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
        Lines          = Lines.Select(ToLineDto).ToList(),
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
        Lines          = Lines.Select(ToLineDto).ToList(),
    };

    private static SalesOrderLineDto ToLineDto(SalesOrderLineItem item) => new()
    {
        ProductId         = item.ProductId,
        ProductCode       = item.ProductCode,
        ProductName       = item.ProductName,
        IsPromotion       = item.IsPromotion,
        Unit              = item.Unit,
        Quantity          = item.Quantity,
        UnitPrice         = item.UnitPrice,
        DiscountRate      = item.DiscountRate,
        Amount            = item.Amount,
        ReceivableAccount = item.ReceivableAccount,
        RevenueAccount    = item.RevenueAccount,
    };
}
