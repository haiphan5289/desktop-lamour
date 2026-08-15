// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseReceiptFormViewModel : ViewModelBase
{
    private readonly ICreateWarehouseReceiptUseCase       _createUseCase;
    private readonly IConfirmWarehouseReceiptUseCase      _confirmUseCase;
    private readonly IGetCustomersUseCase                 _getCustomers;
    private readonly IGetSuppliersUseCase                 _getSuppliers;
    private readonly IGetEmployeesUseCase                 _getEmployees;
    private readonly IGetProductsUseCase                  _getProducts;
    private readonly Func<EmployeeFormWindow>             _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>             _customerFormWindowFactory;
    private readonly ILogger<WarehouseReceiptFormViewModel> _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage   = string.Empty;
    [ObservableProperty] private DateTime _accountingDate  = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate    = DateTime.Today;
    [ObservableProperty] private string   _description     = string.Empty;
    [ObservableProperty] private string   _deliveryPerson  = string.Empty;
    [ObservableProperty] private string   _reference       = string.Empty;
    [ObservableProperty] private decimal  _totalAmount;

    // 0-based index for ComboBox binding; maps to ReceiptType 1, 2, 3, 4
    [ObservableProperty] private int _selectedReceiptTypeIndex;

    public int SelectedReceiptType => SelectedReceiptTypeIndex + 1;

    [ObservableProperty] private ISearchableItem? _selectedObject;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    private IReadOnlyList<ISearchableItem> _customers = Array.Empty<ISearchableItem>();
    private IReadOnlyList<ISearchableItem> _suppliers = Array.Empty<ISearchableItem>();

    public IReadOnlyList<ISearchableItem> Objects  { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Products  { get; private set; } = Array.Empty<ISearchableItem>();

    public ObservableCollection<WarehouseReceiptLineItem> Lines { get; } = new();

    public event Action<bool>? RequestClose;

    public WarehouseReceiptFormViewModel(
        ICreateWarehouseReceiptUseCase       createUseCase,
        IConfirmWarehouseReceiptUseCase      confirmUseCase,
        IGetCustomersUseCase                 getCustomers,
        IGetSuppliersUseCase                 getSuppliers,
        IGetEmployeesUseCase                 getEmployees,
        IGetProductsUseCase                  getProducts,
        Func<EmployeeFormWindow>             employeeFormWindowFactory,
        Func<CustomerFormWindow>             customerFormWindowFactory,
        ILogger<WarehouseReceiptFormViewModel> logger)
    {
        _createUseCase             = createUseCase;
        _confirmUseCase            = confirmUseCase;
        _getCustomers              = getCustomers;
        _getSuppliers              = getSuppliers;
        _getEmployees              = getEmployees;
        _getProducts               = getProducts;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _customerFormWindowFactory = customerFormWindowFactory;
        _logger                    = logger;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            var suppliers = await _getSuppliers.ExecuteAsync(ct);
            var employees = await _getEmployees.ExecuteAsync(ct);
            var products  = await _getProducts.ExecuteAsync(ct);

            _customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            _suppliers = suppliers.Select(s => (ISearchableItem)new WarehouseObjectItem(s)).ToList().AsReadOnly();
            RebuildObjects();

            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            Products  = products.Where(p => p.IsActive).Select(p => (ISearchableItem)new WarehouseProductItem(p)).ToList().AsReadOnly();

            OnPropertyChanged(nameof(Employees));
            OnPropertyChanged(nameof(Products));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload lookup data for WarehouseReceiptForm");
        }

        BeginDirtyTracking();
    }

    private void RebuildObjects()
    {
        Objects = _customers.Concat(_suppliers).ToList().AsReadOnly();
        OnPropertyChanged(nameof(Objects));
    }

    [RelayCommand]
    private void AddLine()
    {
        var line = new WarehouseReceiptLineItem();
        line.PropertyChanged += (_, _) => { RecalculateTotal(); IsDirty = true; };
        Lines.Add(line);
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveLine(WarehouseReceiptLineItem line)
    {
        Lines.Remove(line);
        RecalculateTotal();
        IsDirty = true;
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
        var before = _customers.Select(c => c.Id).ToHashSet();
        var window = _customerFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            _customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            RebuildObjects();
            var newItem = _customers.FirstOrDefault(c => !before.Contains(c.Id));
            if (newItem is not null) SelectedObject = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload customers after add"); }
    }

    private void RecalculateTotal()
        => TotalAmount = Lines.Sum(l => l.Amount);

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (Lines.Count == 0)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng thêm ít nhất một dòng hàng hóa.";
            return;
        }

        if (Lines.Any(l => l.SelectedProduct is null))
        {
            HasError     = true;
            ErrorMessage = "Vui lòng chọn hàng hóa cho tất cả các dòng.";
            return;
        }

        if (Lines.Any(l => l.Quantity <= 0))
        {
            HasError     = true;
            ErrorMessage = "Số lượng phải lớn hơn 0 cho tất cả các dòng.";
            return;
        }

        IsLoading = true;
        try
        {
            var selectedCustomerId = (SelectedObject as WarehouseObjectItem)?.Type == WarehouseObjectType.Supplier
                ? null
                : SelectedObject?.Id;
            var selectedSupplierId = (SelectedObject as WarehouseObjectItem)?.Type == WarehouseObjectType.Supplier
                ? SelectedObject?.Id
                : null;

            var request = new CreateWarehouseReceiptRequestDto
            {
                ReceiptType    = SelectedReceiptType,
                CustomerId     = selectedCustomerId,
                SupplierId     = selectedSupplierId,
                EmployeeId     = SelectedEmployee?.Id,
                AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
                Description    = string.IsNullOrWhiteSpace(Description)    ? null : Description.Trim(),
                DeliveryPerson = string.IsNullOrWhiteSpace(DeliveryPerson) ? null : DeliveryPerson.Trim(),
                Reference      = string.IsNullOrWhiteSpace(Reference)      ? null : Reference.Trim(),
                Lines          = Lines.Select(l => new CreateWarehouseReceiptLineDto
                {
                    ProductId           = l.SelectedProduct!.Id,
                    WarehouseId         = 1,  // default warehouse
                    Quantity            = l.Quantity,
                    UnitPrice           = l.UnitPrice,
                    Amount              = l.Amount,
                    DebitAccount        = l.DebitAccount,
                    CreditAccount       = l.CreditAccount,
                    CostItem            = string.IsNullOrWhiteSpace(l.CostItem)            ? null : l.CostItem.Trim(),
                    CostObject          = string.IsNullOrWhiteSpace(l.CostObject)          ? null : l.CostObject.Trim(),
                    Project             = string.IsNullOrWhiteSpace(l.Project)             ? null : l.Project.Trim(),
                    PurchaseOrderNumber = string.IsNullOrWhiteSpace(l.PurchaseOrderNumber) ? null : l.PurchaseOrderNumber.Trim(),
                    SalesContractNumber = string.IsNullOrWhiteSpace(l.SalesContractNumber) ? null : l.SalesContractNumber.Trim(),
                    LoanContractNumber  = string.IsNullOrWhiteSpace(l.LoanContractNumber)  ? null : l.LoanContractNumber.Trim(),
                    StatisticsCode      = string.IsNullOrWhiteSpace(l.StatisticsCode)      ? null : l.StatisticsCode.Trim(),
                }).ToList()
            };

            var result = await _createUseCase.ExecuteAsync(request, ct);
            await _confirmUseCase.ExecuteAsync(result.Id, ct);
            _logger.LogInformation("Warehouse receipt created and confirmed: {ReceiptNumber}", result.ReceiptNumber);
            StopDirtyTracking();
            RequestClose?.Invoke(true);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create warehouse receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsDirty)
        {
            var r = MessageBox.Show(
                "Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;
        }
        StopDirtyTracking();
        RequestClose?.Invoke(false);
    }
}
