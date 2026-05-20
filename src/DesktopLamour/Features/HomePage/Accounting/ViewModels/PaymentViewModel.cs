// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class PaymentViewModel : ViewModelBase
{
    public event Action? PaymentSaved;
    public event Action? RequestClose;
    private readonly IGetPaymentsUseCase      _getPayments;
    private readonly IGetPaymentByIdUseCase   _getPaymentById;
    private readonly ICreatePaymentUseCase    _createPayment;
    private readonly IUpdatePaymentUseCase    _updatePayment;
    private readonly IDeletePaymentUseCase    _deletePayment;
    private readonly IGetSuppliersUseCase     _getSuppliers;
    private readonly IGetEmployeesUseCase     _getEmployees;
    private readonly Func<EmployeeFormWindow> _employeeFormWindowFactory;
    private readonly ILogger<PaymentViewModel> _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private bool    _isEditing;

    // ── Header — Thông tin chung ──────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedSupplier;
    [ObservableProperty] private string  _payeeName             = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string  _selectedPaymentReason = "ChiKhac";
    [ObservableProperty] private ISearchableItem? _selectedPaymentEmployeeEmployee;
    [ObservableProperty] private string? _attachment;
    [ObservableProperty] private string? _reference;

    // ── Chứng từ ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "PC00001";

    // ── Computed ──────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string  _entrySummary = "Số dòng = 0";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private PaymentResponseDto? _currentPayment;
    [ObservableProperty] private IEnumerable<PaymentResponseDto> _paymentList = Enumerable.Empty<PaymentResponseDto>();

    public ObservableCollection<PaymentEntryItem> Entries { get; } = new();

    public IReadOnlyList<ISearchableItem> Suppliers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();

    public IReadOnlyList<string> PaymentReasons { get; } = new[]
    {
        "ChiKhac", "ChiMuaHang", "ChiTraNo"
    };

    public IReadOnlyList<string> AccountCodes { get; } = new[]
    {
        "Cash111", "Bank112", "Receivable131", "Payroll334"
    };

    private List<PaymentResponseDto> _receiptListCache = new();
    private int _currentIndex = -1;

    public PaymentViewModel(
        IGetPaymentsUseCase      getPayments,
        IGetPaymentByIdUseCase   getPaymentById,
        ICreatePaymentUseCase    createPayment,
        IUpdatePaymentUseCase    updatePayment,
        IDeletePaymentUseCase    deletePayment,
        IGetSuppliersUseCase     getSuppliers,
        IGetEmployeesUseCase     getEmployees,
        Func<EmployeeFormWindow> employeeFormWindowFactory,
        ILogger<PaymentViewModel> logger)
    {
        _getPayments               = getPayments;
        _getPaymentById            = getPaymentById;
        _createPayment             = createPayment;
        _updatePayment             = updatePayment;
        _deletePayment             = deletePayment;
        _getSuppliers              = getSuppliers;
        _getEmployees              = getEmployees;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _logger                    = logger;

        Entries.CollectionChanged += (_, _) => RecalculateTotals();
    }

    // ── Init ──────────────────────────────────────────────────────────────

    public async Task LoadAsync(CancellationToken ct = default)
    {
        await LoadLookupsAsync(ct);
        await LoadPaymentsAsync(ct);
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            var customers = await _getSuppliers.ExecuteAsync(ct);
            var employees = await _getEmployees.ExecuteAsync(ct);

            Suppliers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();

            OnPropertyChanged(nameof(Suppliers));
            OnPropertyChanged(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload lookup data for PaymentWindow");
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadAsync2(CancellationToken ct = default)
        => await LoadPaymentsAsync(ct);

    private async Task LoadPaymentsAsync(CancellationToken ct)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            var list = await _getPayments.ExecuteAsync(ct);
            _receiptListCache = list.ToList();
            PaymentList       = _receiptListCache;

            if (_receiptListCache.Count > 0)
            {
                _currentIndex  = 0;
                CurrentPayment = _receiptListCache[0];
                PopulateFormFromCurrent();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load receipts");
            HasError     = true;
            ErrorMessage = $"Không thể tải danh sách phiếu chi: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void AddNew()
    {
        CurrentPayment        = null;
        _currentIndex         = -1;
        IsEditing             = true;
        ClearForm();
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (SelectedSupplier is null)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng chọn đối tượng (nhà cung cấp).";
            return;
        }

        IsBusy = true;
        try
        {
            if (CurrentPayment is null)
            {
                // Create
                var request = BuildCreateRequest();
                var result  = await _createPayment.ExecuteAsync(request, ct);
                _logger.LogInformation("Payment created: {DocumentNumber}", result.DocumentNumber);
                await LoadPaymentsAsync(ct);
                NavigateToPayment(result.Id);
            }
            else
            {
                // Update
                var request = BuildUpdateRequest();
                var result  = await _updatePayment.ExecuteAsync(CurrentPayment.Id, request, ct);
                _logger.LogInformation("Payment updated: {Id}", result.Id);
                await LoadPaymentsAsync(ct);
                NavigateToPayment(result.Id);
            }

            IsEditing = false;
            PaymentSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentPayment is null) return;

        IsBusy = true;
        try
        {
            await _deletePayment.ExecuteAsync(CurrentPayment.Id, ct);
            _logger.LogInformation("Payment deleted: {Id}", CurrentPayment.Id);
            await LoadPaymentsAsync(ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void NavigatePrev()
    {
        if (_receiptListCache.Count == 0 || _currentIndex <= 0) return;
        _currentIndex--;
        CurrentPayment = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand]
    private void NavigateNext()
    {
        if (_receiptListCache.Count == 0 || _currentIndex >= _receiptListCache.Count - 1) return;
        _currentIndex++;
        CurrentPayment = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand]
    private void AddEntry()
    {
        var entry = new PaymentEntryItem
        {
            DebitAccount  = "Cash111",
            CreditAccount = "Receivable131",
        };
        entry.PropertyChanged += (_, _) => RecalculateTotals();
        Entries.Add(entry);
    }

    [RelayCommand]
    private void RemoveEntry(PaymentEntryItem entry)
    {
        Entries.Remove(entry);
        RecalculateTotals();
    }

    [RelayCommand]
    private async Task CancelAsync(CancellationToken ct = default)
    {
        IsEditing = false;
        HasError  = false;
        await LoadPaymentsAsync(ct);
    }

    [RelayCommand]
    private async Task AddPaymentEmployeeAsync(CancellationToken ct = default)
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
            if (newItem is not null) SelectedPaymentEmployeeEmployee = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload employees after add"); }
    }

    // ── Partial property change hooks ─────────────────────────────────────

    partial void OnSelectedSupplierChanged(ISearchableItem? value)
    {
        if (value is not null)
        {
            PayeeName = value.Name;
            
            // Auto-populate Address from Supplier
            if (value is DesktopLamour.Features.HomePage.Suppliers.Domain.Models.Supplier customer)
            {
                Address = customer.Address;
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void ClearForm()
    {
        SelectedSupplier          = null;
        PayeeName                 = string.Empty;
        Address                   = null;
        SelectedPaymentReason     = "ChiKhac";
        SelectedPaymentEmployeeEmployee = null;
        Attachment                = null;
        Reference                 = null;
        AccountingDate            = DateTime.Today;
        DocumentDate              = DateTime.Today;
        DocumentNumber            = GenerateNextDocumentNumber();
        Entries.Clear();
        RecalculateTotals();
    }

    private string GenerateNextDocumentNumber()
    {
        const string prefix = "PC";
        var maxNum = _receiptListCache
            .Select(p => p.DocumentNumber)
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"{prefix}{maxNum + 1:D5}";
    }

    private void PopulateFormFromCurrent()
    {
        if (CurrentPayment is null) return;

        SelectedSupplier          = Suppliers.FirstOrDefault(c => c.Id == CurrentPayment.SupplierId);
        PayeeName                 = CurrentPayment.PayeeName;
        Address                   = CurrentPayment.Address;
        SelectedPaymentReason     = CurrentPayment.PaymentReason;
        SelectedPaymentEmployeeEmployee = Employees.FirstOrDefault(e => e.Id == CurrentPayment.PaymentEmployeeId);
        Attachment                = CurrentPayment.Attachment;
        Reference                 = CurrentPayment.Reference;
        AccountingDate            = CurrentPayment.AccountingDate.ToLocalTime();
        DocumentDate              = CurrentPayment.DocumentDate.ToLocalTime();
        DocumentNumber            = CurrentPayment.DocumentNumber;

        Entries.Clear();
        foreach (var e in CurrentPayment.Entries)
        {
            var item = new PaymentEntryItem
            {
                Description   = e.Description,
                DebitAccount  = e.DebitAccount,
                CreditAccount = e.CreditAccount,
                Amount        = e.Amount,
                SubjectCode   = e.SubjectCode,
                SubjectName   = e.SubjectName,
                BankAccount   = e.BankAccount,
            };
            item.PropertyChanged += (_, _) => RecalculateTotals();
            Entries.Add(item);
        }

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalAmount   = Entries.Sum(e => e.Amount);
        EntrySummary  = $"Số dòng = {Entries.Count}";
    }

    private CreatePaymentRequestDto BuildCreateRequest() => new()
    {
        SupplierId          = SelectedSupplier!.Id,
        PayeeName           = PayeeName.Trim(),
        Address             = string.IsNullOrWhiteSpace(Address)         ? null : Address.Trim(),
        PaymentReason       = SelectedPaymentReason,
        PaymentEmployeeId = SelectedPaymentEmployeeEmployee?.Id,
        Attachment          = string.IsNullOrWhiteSpace(Attachment)      ? null : Attachment.Trim(),
        Reference           = string.IsNullOrWhiteSpace(Reference)       ? null : Reference.Trim(),
        AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date,    DateTimeKind.Unspecified),
        DocumentNumber      = DocumentNumber.Trim(),
        Entries             = Entries.Select(ToEntryDto).ToList(),
    };

    private UpdatePaymentRequestDto BuildUpdateRequest() => new()
    {
        SupplierId          = SelectedSupplier!.Id,
        PayeeName           = PayeeName.Trim(),
        Address             = string.IsNullOrWhiteSpace(Address)         ? null : Address.Trim(),
        PaymentReason       = SelectedPaymentReason,
        PaymentEmployeeId = SelectedPaymentEmployeeEmployee?.Id,
        Attachment          = string.IsNullOrWhiteSpace(Attachment)      ? null : Attachment.Trim(),
        Reference           = string.IsNullOrWhiteSpace(Reference)       ? null : Reference.Trim(),
        AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date,    DateTimeKind.Unspecified),
        DocumentNumber      = DocumentNumber.Trim(),
        Entries             = Entries.Select(ToEntryDto).ToList(),
    };

    private static PaymentEntryDto ToEntryDto(PaymentEntryItem item) => new()
    {
        Description   = item.Description,
        DebitAccount  = item.DebitAccount,
        CreditAccount = item.CreditAccount,
        Amount        = item.Amount,
        SubjectCode   = item.SubjectCode,
        SubjectName   = item.SubjectName,
        BankAccount   = item.BankAccount,
    };

    private void NavigateToPayment(int id)
    {
        var idx = _receiptListCache.FindIndex(r => r.Id == id);
        if (idx >= 0)
        {
            _currentIndex  = idx;
            CurrentPayment = _receiptListCache[idx];
            PopulateFormFromCurrent();
        }
    }
}
