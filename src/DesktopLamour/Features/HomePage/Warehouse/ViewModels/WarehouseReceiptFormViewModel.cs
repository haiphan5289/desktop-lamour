// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseReceiptFormViewModel : ViewModelBase
{
    private readonly ICreateWarehouseReceiptUseCase       _createUseCase;
    private readonly IConfirmWarehouseReceiptUseCase      _confirmUseCase;
    private readonly IGetCustomersUseCase                 _getCustomers;
    private readonly IGetEmployeesUseCase                 _getEmployees;
    private readonly IGetProductsUseCase                  _getProducts;
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

    // 0-based index for ComboBox binding; maps to ReceiptType 1, 2, 3
    [ObservableProperty] private int _selectedReceiptTypeIndex;

    public int SelectedReceiptType => SelectedReceiptTypeIndex + 1;

    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Products  { get; private set; } = Array.Empty<ISearchableItem>();

    public ObservableCollection<WarehouseReceiptLineItem> Lines { get; } = new();

    public event Action<bool>? RequestClose;

    public WarehouseReceiptFormViewModel(
        ICreateWarehouseReceiptUseCase       createUseCase,
        IConfirmWarehouseReceiptUseCase      confirmUseCase,
        IGetCustomersUseCase                 getCustomers,
        IGetEmployeesUseCase                 getEmployees,
        IGetProductsUseCase                  getProducts,
        ILogger<WarehouseReceiptFormViewModel> logger)
    {
        _createUseCase  = createUseCase;
        _confirmUseCase = confirmUseCase;
        _getCustomers   = getCustomers;
        _getEmployees   = getEmployees;
        _getProducts    = getProducts;
        _logger         = logger;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            var employees = await _getEmployees.ExecuteAsync(ct);
            var products  = await _getProducts.ExecuteAsync(ct);

            Customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            Products  = products.Where(p => p.IsActive).Select(p => (ISearchableItem)new WarehouseProductItem(p)).ToList().AsReadOnly();

            OnPropertyChanged(nameof(Customers));
            OnPropertyChanged(nameof(Employees));
            OnPropertyChanged(nameof(Products));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload lookup data for WarehouseReceiptForm");
        }
    }

    [RelayCommand]
    private void AddLine()
    {
        var line = new WarehouseReceiptLineItem();
        line.PropertyChanged += (_, _) => RecalculateTotal();
        Lines.Add(line);
    }

    [RelayCommand]
    private void RemoveLine(WarehouseReceiptLineItem line)
    {
        Lines.Remove(line);
        RecalculateTotal();
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
            var request = new CreateWarehouseReceiptRequestDto
            {
                ReceiptType    = SelectedReceiptType,
                CustomerId     = SelectedCustomer?.Id,
                EmployeeId     = SelectedEmployee?.Id,
                AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
                Description    = string.IsNullOrWhiteSpace(Description)    ? null : Description.Trim(),
                DeliveryPerson = string.IsNullOrWhiteSpace(DeliveryPerson) ? null : DeliveryPerson.Trim(),
                Reference      = string.IsNullOrWhiteSpace(Reference)      ? null : Reference.Trim(),
                Lines          = Lines.Select(l => new CreateWarehouseReceiptLineDto
                {
                    ProductId     = l.SelectedProduct!.Id,
                    WarehouseId   = 1,  // default warehouse
                    Quantity      = l.Quantity,
                    UnitPrice     = l.UnitPrice,
                    Amount        = l.Amount,
                    DebitAccount  = l.DebitAccount,
                    CreditAccount = l.CreditAccount,
                }).ToList()
            };

            var result = await _createUseCase.ExecuteAsync(request, ct);
            await _confirmUseCase.ExecuteAsync(result.Id, ct);
            _logger.LogInformation("Warehouse receipt created and confirmed: {ReceiptNumber}", result.ReceiptNumber);
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
    private void Cancel() => RequestClose?.Invoke(false);
}
