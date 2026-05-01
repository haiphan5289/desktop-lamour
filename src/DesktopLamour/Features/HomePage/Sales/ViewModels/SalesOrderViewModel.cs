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
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderViewModel : ViewModelBase
{
    public event Action? OrderSaved;
    public event Action? RequestClose;

    private readonly IGetSalesOrdersUseCase    _getOrders;
    private readonly ICreateSalesOrderUseCase  _createOrder;
    private readonly IUpdateSalesOrderUseCase  _updateOrder;
    private readonly IDeleteSalesOrderUseCase  _deleteOrder;
    private readonly IGetCustomersUseCase      _getCustomers;
    private readonly IGetEmployeesUseCase      _getEmployees;
    private readonly IGetProductsUseCase       _getProducts;
    private readonly ILogger<SalesOrderViewModel> _logger;

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
    public IReadOnlyList<ISearchableItem> Products  { get; private set; } = Array.Empty<ISearchableItem>();

    // cached document numbers for generating the next number in new-order mode
    private List<string> _orderNumberCache = new();

    public SalesOrderViewModel(
        IGetSalesOrdersUseCase    getOrders,
        ICreateSalesOrderUseCase  createOrder,
        IUpdateSalesOrderUseCase  updateOrder,
        IDeleteSalesOrderUseCase  deleteOrder,
        IGetCustomersUseCase      getCustomers,
        IGetEmployeesUseCase      getEmployees,
        IGetProductsUseCase       getProducts,
        ILogger<SalesOrderViewModel> logger)
    {
        _getOrders    = getOrders;
        _createOrder  = createOrder;
        _updateOrder  = updateOrder;
        _deleteOrder  = deleteOrder;
        _getCustomers = getCustomers;
        _getEmployees = getEmployees;
        _getProducts  = getProducts;
        _logger       = logger;

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
                var allOrders     = await _getOrders.ExecuteAsync(ct);
                _orderNumberCache = allOrders.Select(o => o.DocumentNumber).ToList();
                CurrentOrder      = null;
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
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            Customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Customers));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload customers for SalesOrderWindow");
        }

        try
        {
            var employees = await _getEmployees.ExecuteAsync(ct);
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload employees for SalesOrderWindow");
        }

        try
        {
            var products = await _getProducts.ExecuteAsync(ct);
            Products = products
                .Where(p => p.IsActive)
                .Cast<ISearchableItem>()
                .ToList()
                .AsReadOnly();
            OnPropertyChanged(nameof(Products));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload products for SalesOrderWindow");
        }
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

    private string GenerateNextDocumentNumber()
    {
        const string prefix = "BC";
        var maxNum = _orderNumberCache
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"{prefix}{maxNum + 1:D5}";
    }

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
            item.SetSelectedProductSilent(Products.FirstOrDefault(p => p.Id == l.ProductId));
            item.PropertyChanged += (_, _) => RecalculateTotals();
            Lines.Add(item);
        }

        RecalculateTotals();
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
