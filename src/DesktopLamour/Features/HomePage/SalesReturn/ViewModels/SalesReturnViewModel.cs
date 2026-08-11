// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

public partial class SalesReturnViewModel : ViewModelBase
{
    public event Action? ReturnSaved;
    public event Action? RequestClose;

    private readonly ICreateSalesReturnUseCase      _createReturn;
    private readonly IUpdateSalesReturnUseCase      _updateReturn;
    private readonly IDeleteSalesReturnUseCase      _deleteReturn;
    private readonly IGetNextSalesReturnCodeUseCase _getNextCode;
    private readonly IGetCustomersUseCase           _getCustomers;
    private readonly IGetEmployeesUseCase           _getEmployees;
    private readonly IGetProductsUseCase            _getProducts;
    private readonly IGetWarehouseSettingsUseCase   _getWarehouses;
    private readonly Func<EmployeeFormWindow>       _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>       _customerFormWindowFactory;
    private readonly ILogger<SalesReturnViewModel>  _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Header ────────────────────────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;
    [ObservableProperty] private string?          _description;
    [ObservableProperty] private string?          _reference;

    // ── Return type ───────────────────────────────────────────────────────
    [ObservableProperty] private int    _returnType;  // 0=GiảmTrừCôngNợ, 1=TrảLạiTiềnMặt
    [ObservableProperty] private string _returnTypeLabel = "Giảm trừ công nợ";

    partial void OnReturnTypeChanged(int value)
        => ReturnTypeLabel = value == 1 ? "Trả lại tiền mặt" : "Giảm trừ công nợ";

    // ── Chứng từ ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "BTL00001";

    // ── Computed ──────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private decimal _totalDiscount;
    [ObservableProperty] private decimal _totalPayment;
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private SalesReturnResponseDto? _currentReturn;

    public bool HasExistingReturn => CurrentReturn is not null;

    public ObservableCollection<SalesReturnLineItem> Lines { get; } = new();

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<ISearchableItem> Products { get; } = new();
    public IReadOnlyList<ISearchableItem> Warehouses { get; private set; } = Array.Empty<ISearchableItem>();
    private readonly List<ISearchableItem> _allProducts = new();

    public IReadOnlyList<ReturnTypeItem> ReturnTypes { get; } = new[]
    {
        new ReturnTypeItem(0, "Giảm trừ công nợ"),
        new ReturnTypeItem(1, "Trả lại tiền mặt"),
    };

    private string _nextDocumentNumber = "BTL00001";

    public SalesReturnViewModel(
        ICreateSalesReturnUseCase      createReturn,
        IUpdateSalesReturnUseCase      updateReturn,
        IDeleteSalesReturnUseCase      deleteReturn,
        IGetNextSalesReturnCodeUseCase getNextCode,
        IGetCustomersUseCase           getCustomers,
        IGetEmployeesUseCase           getEmployees,
        IGetProductsUseCase            getProducts,
        IGetWarehouseSettingsUseCase   getWarehouses,
        Func<EmployeeFormWindow>       employeeFormWindowFactory,
        Func<CustomerFormWindow>       customerFormWindowFactory,
        ILogger<SalesReturnViewModel>  logger)
    {
        _createReturn              = createReturn;
        _updateReturn              = updateReturn;
        _deleteReturn              = deleteReturn;
        _getNextCode               = getNextCode;
        _getCustomers              = getCustomers;
        _getEmployees              = getEmployees;
        _getProducts               = getProducts;
        _getWarehouses             = getWarehouses;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _customerFormWindowFactory = customerFormWindowFactory;
        _logger                    = logger;

        Lines.CollectionChanged += (_, _) => RecalculateTotals();
    }

    public async Task InitializeAsync(SalesReturnResponseDto? returnDoc, CancellationToken ct = default)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            await LoadLookupsAsync(ct);

            if (returnDoc is null)
            {
                _nextDocumentNumber = await _getNextCode.ExecuteAsync(ct);
                CurrentReturn       = null;
                ClearForm();
            }
            else
            {
                CurrentReturn = returnDoc;
                PopulateFormFromCurrent();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SalesReturnViewModel");
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
        else _logger.LogWarning(warehouseTask.Exception, "Could not preload warehouses");

        if (customerTask.IsCompletedSuccessfully)
        {
            Customers = customerTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Customers));
        }
        else _logger.LogWarning(customerTask.Exception, "Could not preload customers");

        if (employeeTask.IsCompletedSuccessfully)
        {
            Employees = employeeTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
        }
        else _logger.LogWarning(employeeTask.Exception, "Could not preload employees");

        if (productTask.IsCompletedSuccessfully)
        {
            _allProducts.Clear();
            _allProducts.AddRange(productTask.Result.Where(p => p.IsActive).Cast<ISearchableItem>());
            ResetProductFilter();
        }
        else _logger.LogWarning(productTask.Exception, "Could not preload products");
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
            if (CurrentReturn is null)
            {
                var request = BuildCreateRequest();
                var result  = await _createReturn.ExecuteAsync(request, ct);
                _logger.LogInformation("SalesReturn created: {DocumentNumber}", result.DocumentNumber);
            }
            else
            {
                var request = BuildUpdateRequest();
                var result  = await _updateReturn.ExecuteAsync(CurrentReturn.Id, request, ct);
                _logger.LogInformation("SalesReturn updated: {Id}", result.Id);
            }

            StopDirtyTracking();
            ReturnSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save sales return");
            HasError     = true;
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Không thể ghi sổ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentReturn is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{CurrentReturn.DocumentNumber}'?\nTồn kho sẽ được trừ lại.",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _deleteReturn.ExecuteAsync(CurrentReturn.Id, ct);
            _logger.LogInformation("SalesReturn deleted: {Id}", CurrentReturn.Id);
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete sales return");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Cancel()
    {
        HasError = false;
        if (CurrentReturn is null)
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
        var line = new SalesReturnLineItem
        {
            ReturnAccount   = "5212",
            DebtAccount     = "131",
            DiscountAccount = "5211",
            Quantity        = 1,
        };
        line.SetSelectedWarehouseSilent(Warehouses.FirstOrDefault());
        line.PropertyChanged += (_, _) => RecalculateTotals();
        Lines.Add(line);
    }

    [RelayCommand]
    private void RemoveLine(SalesReturnLineItem line)
    {
        Lines.Remove(line);
        RecalculateTotals();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    partial void OnSelectedCustomerChanged(ISearchableItem? value)
    {
        if (value is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer c)
            Description = $"Thu hồi hàng {c.Name}";
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        SelectedEmployee = null;
        Description      = null;
        Reference        = null;
        ReturnType       = 0;
        AccountingDate   = DateTime.Today;
        DocumentDate     = DateTime.Today;
        DocumentNumber   = _nextDocumentNumber;
        Lines.Clear();
        RecalculateTotals();
    }

    private void PopulateFormFromCurrent()
    {
        if (CurrentReturn is null) return;

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentReturn.CustomerId);
        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == CurrentReturn.EmployeeId);
        Description      = CurrentReturn.Description;
        Reference        = CurrentReturn.Reference;
        ReturnType       = CurrentReturn.ReturnType;
        AccountingDate   = CurrentReturn.AccountingDate.ToLocalTime();
        DocumentDate     = CurrentReturn.DocumentDate.ToLocalTime();
        DocumentNumber   = CurrentReturn.DocumentNumber;

        Lines.Clear();
        foreach (var l in CurrentReturn.Lines)
        {
            var item = new SalesReturnLineItem
            {
                ProductId        = l.ProductId,
                ProductCode      = l.ProductCode,
                ProductName      = l.ProductName,
                ReturnAccount    = l.ReturnAccount,
                DebtAccount      = l.DebtAccount,
                DiscountAccount  = l.DiscountAccount,
                Unit             = l.Unit,
                Quantity         = l.Quantity,
                UnitPrice        = l.UnitPrice,
                DiscountRate     = l.DiscountRate,
                SalesOrderNumber = l.SalesOrderNumber,
            };
            item.SetSelectedProductSilent(_allProducts.FirstOrDefault(p => p.Id == l.ProductId));
            item.SetSelectedWarehouseSilent(Warehouses.FirstOrDefault(w => w.Id == l.WarehouseId));
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

    public void ResetProductFilter() => RefreshProducts(_allProducts);

    private void RefreshProducts(IEnumerable<ISearchableItem> items)
    {
        Products.Clear();
        foreach (var p in items) Products.Add(p);
    }

    private void RecalculateTotals()
    {
        TotalAmount   = Lines.Sum(l => l.Amount);
        TotalDiscount = Lines.Sum(l => l.DiscountAmount);
        TotalPayment  = TotalAmount - TotalDiscount;
        LineSummary   = $"Số dòng = {Lines.Count}";
    }

    private CreateSalesReturnRequestDto BuildCreateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)   ? null : Reference.Trim(),
        ReturnType     = ReturnType,
        Lines          = Lines.Select(ToLineDto).ToList(),
    };

    private UpdateSalesReturnRequestDto BuildUpdateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)   ? null : Reference.Trim(),
        ReturnType     = ReturnType,
        Lines          = Lines.Select(ToLineDto).ToList(),
    };

    private static SalesReturnLineDto ToLineDto(SalesReturnLineItem item) => new()
    {
        ProductId        = item.ProductId,
        WarehouseId      = item.WarehouseId,
        ProductCode      = item.ProductCode,
        ProductName      = item.ProductName,
        ReturnAccount    = item.ReturnAccount,
        DebtAccount      = item.DebtAccount,
        DiscountAccount  = item.DiscountAccount,
        Unit             = item.Unit,
        Quantity         = item.Quantity,
        UnitPrice        = item.UnitPrice,
        Amount           = item.Amount,
        DiscountRate     = item.DiscountRate,
        DiscountAmount   = item.DiscountAmount,
        SalesOrderNumber = item.SalesOrderNumber,
    };
}

public record ReturnTypeItem(int Value, string Label);
