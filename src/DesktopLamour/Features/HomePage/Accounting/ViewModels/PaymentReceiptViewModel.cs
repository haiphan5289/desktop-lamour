// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class PaymentReceiptViewModel : ViewModelBase
{
    private readonly ICreatePaymentReceiptUseCase     _createUseCase;
    private readonly IGetCustomersUseCase             _getCustomers;
    private readonly IGetEmployeesUseCase             _getEmployees;
    private readonly ILogger<PaymentReceiptViewModel> _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage          = string.Empty;
    [ObservableProperty] private string   _selectedPaymentMethod = "Cash";
    [ObservableProperty] private string   _currency              = "VND";
    [ObservableProperty] private decimal  _exchangeRate          = 1m;
    [ObservableProperty] private DateTime _collectionDate        = DateTime.Today;
    [ObservableProperty] private string   _description           = string.Empty;
    [ObservableProperty] private decimal  _totalAmount;

    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    // Exposed for AppSearchableComboBox bindings
    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();

    public ObservableCollection<PaymentReceiptLineItem> Lines { get; } = new();

    public event Action<bool>? RequestClose;

    public PaymentReceiptViewModel(
        ICreatePaymentReceiptUseCase     createUseCase,
        IGetCustomersUseCase             getCustomers,
        IGetEmployeesUseCase             getEmployees,
        ILogger<PaymentReceiptViewModel> logger)
    {
        _createUseCase = createUseCase;
        _getCustomers  = getCustomers;
        _getEmployees  = getEmployees;
        _logger        = logger;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            var employees = await _getEmployees.ExecuteAsync(ct);

            Customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();

            OnPropertyChanged(nameof(Customers));
            OnPropertyChanged(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload customers/employees for PaymentReceipt");
        }
    }

    [RelayCommand]
    private void AddLine() => Lines.Add(new PaymentReceiptLineItem());

    [RelayCommand]
    private void RemoveLine(PaymentReceiptLineItem line) => Lines.Remove(line);

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

        if (TotalAmount <= 0)
        {
            HasError     = true;
            ErrorMessage = "Số tiền phải lớn hơn 0.";
            return;
        }

        IsLoading = true;
        try
        {
            var request = new CreatePaymentReceiptRequestDto
            {
                CustomerId     = SelectedCustomer.Id,
                EmployeeId     = SelectedEmployee?.Id,
                CollectionDate = CollectionDate,
                Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                TotalAmount    = TotalAmount,
                PaymentMethod  = SelectedPaymentMethod,
                Currency       = Currency,
                ExchangeRate   = ExchangeRate,
                Lines          = Lines.Select(l => new CreatePaymentReceiptLineDtoRequest
                {
                    DocumentDate   = l.DocumentDate,
                    DocumentNumber = l.DocumentNumber,
                    InvoiceNumber  = l.InvoiceNumber,
                    Description    = l.Description,
                    DueDate        = l.DueDate,
                    AmountDue      = l.AmountDue,
                    AmountPaid     = l.AmountPaid,
                }).ToList()
            };

            var result = await _createUseCase.ExecuteAsync(request, ct);
            _logger.LogInformation("Payment receipt created: {ReceiptNumber}", result.ReceiptNumber);
            RequestClose?.Invoke(true);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);
}
