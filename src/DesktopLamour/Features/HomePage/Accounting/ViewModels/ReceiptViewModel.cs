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

public partial class ReceiptViewModel : ViewModelBase
{
    public event Action? ReceiptSaved;
    public event Action? RequestClose;
    private readonly IGetReceiptsUseCase      _getReceipts;
    private readonly IGetReceiptByIdUseCase   _getReceiptById;
    private readonly ICreateReceiptUseCase    _createReceipt;
    private readonly IUpdateReceiptUseCase    _updateReceipt;
    private readonly IDeleteReceiptUseCase    _deleteReceipt;
    private readonly IGetCustomersUseCase     _getCustomers;
    private readonly IGetEmployeesUseCase     _getEmployees;
    private readonly ILogger<ReceiptViewModel> _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private bool    _isEditing;

    // ── Header — Thông tin chung ──────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private string  _payerName             = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string  _selectedPaymentReason = "ThuKhac";
    [ObservableProperty] private ISearchableItem? _selectedCollectorEmployee;
    [ObservableProperty] private string? _attachment;
    [ObservableProperty] private string? _reference;

    // ── Chứng từ ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "PT00067";

    // ── Computed ──────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string  _entrySummary = "Số dòng = 0";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private ReceiptResponseDto? _currentReceipt;
    [ObservableProperty] private IEnumerable<ReceiptResponseDto> _receiptList = Enumerable.Empty<ReceiptResponseDto>();

    public ObservableCollection<ReceiptEntryItem> Entries { get; } = new();

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();

    public IReadOnlyList<string> PaymentReasons { get; } = new[]
    {
        "ThuKhac", "ThuTienHang", "ThuCongNo"
    };

    public IReadOnlyList<string> AccountCodes { get; } = new[]
    {
        "Cash111", "Bank112", "Receivable131", "Payroll334"
    };

    private List<ReceiptResponseDto> _receiptListCache = new();
    private int _currentIndex = -1;

    public ReceiptViewModel(
        IGetReceiptsUseCase      getReceipts,
        IGetReceiptByIdUseCase   getReceiptById,
        ICreateReceiptUseCase    createReceipt,
        IUpdateReceiptUseCase    updateReceipt,
        IDeleteReceiptUseCase    deleteReceipt,
        IGetCustomersUseCase     getCustomers,
        IGetEmployeesUseCase     getEmployees,
        ILogger<ReceiptViewModel> logger)
    {
        _getReceipts    = getReceipts;
        _getReceiptById = getReceiptById;
        _createReceipt  = createReceipt;
        _updateReceipt  = updateReceipt;
        _deleteReceipt  = deleteReceipt;
        _getCustomers   = getCustomers;
        _getEmployees   = getEmployees;
        _logger         = logger;

        Entries.CollectionChanged += (_, _) => RecalculateTotals();
    }

    // ── Init ──────────────────────────────────────────────────────────────

    public async Task LoadAsync(CancellationToken ct = default)
    {
        await LoadLookupsAsync(ct);
        await LoadReceiptsAsync(ct);
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
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
            _logger.LogWarning(ex, "Could not preload lookup data for ReceiptWindow");
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadAsync2(CancellationToken ct = default)
        => await LoadReceiptsAsync(ct);

    private async Task LoadReceiptsAsync(CancellationToken ct)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            var list = await _getReceipts.ExecuteAsync(ct);
            _receiptListCache = list.ToList();
            ReceiptList       = _receiptListCache;

            if (_receiptListCache.Count > 0)
            {
                _currentIndex  = 0;
                CurrentReceipt = _receiptListCache[0];
                PopulateFormFromCurrent();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load receipts");
            HasError     = true;
            ErrorMessage = $"Không thể tải danh sách phiếu thu: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void AddNew()
    {
        CurrentReceipt        = null;
        _currentIndex         = -1;
        IsEditing             = true;
        ClearForm();
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (SelectedCustomer is null)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng chọn đối tượng (khách hàng).";
            return;
        }

        IsBusy = true;
        try
        {
            if (CurrentReceipt is null)
            {
                // Create
                var request = BuildCreateRequest();
                var result  = await _createReceipt.ExecuteAsync(request, ct);
                _logger.LogInformation("Receipt created: {DocumentNumber}", result.DocumentNumber);
                await LoadReceiptsAsync(ct);
                NavigateToReceipt(result.Id);
            }
            else
            {
                // Update
                var request = BuildUpdateRequest();
                var result  = await _updateReceipt.ExecuteAsync(CurrentReceipt.Id, request, ct);
                _logger.LogInformation("Receipt updated: {Id}", result.Id);
                await LoadReceiptsAsync(ct);
                NavigateToReceipt(result.Id);
            }

            IsEditing = false;
            ReceiptSaved?.Invoke();
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
        if (CurrentReceipt is null) return;

        IsBusy = true;
        try
        {
            await _deleteReceipt.ExecuteAsync(CurrentReceipt.Id, ct);
            _logger.LogInformation("Receipt deleted: {Id}", CurrentReceipt.Id);
            await LoadReceiptsAsync(ct);
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
        CurrentReceipt = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand]
    private void NavigateNext()
    {
        if (_receiptListCache.Count == 0 || _currentIndex >= _receiptListCache.Count - 1) return;
        _currentIndex++;
        CurrentReceipt = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand]
    private void AddEntry()
    {
        var entry = new ReceiptEntryItem
        {
            DebitAccount  = "Cash111",
            CreditAccount = "Receivable131",
        };
        entry.PropertyChanged += (_, _) => RecalculateTotals();
        Entries.Add(entry);
    }

    [RelayCommand]
    private void RemoveEntry(ReceiptEntryItem entry)
    {
        Entries.Remove(entry);
        RecalculateTotals();
    }

    [RelayCommand]
    private async Task CancelAsync(CancellationToken ct = default)
    {
        IsEditing = false;
        HasError  = false;
        await LoadReceiptsAsync(ct);
    }

    // ── Partial property change hooks ─────────────────────────────────────

    partial void OnSelectedCustomerChanged(ISearchableItem? value)
    {
        if (value is not null)
            PayerName = value.Name;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void ClearForm()
    {
        SelectedCustomer          = null;
        PayerName                 = string.Empty;
        Address                   = null;
        SelectedPaymentReason     = "ThuKhac";
        SelectedCollectorEmployee = null;
        Attachment                = null;
        Reference                 = null;
        AccountingDate            = DateTime.Today;
        DocumentDate              = DateTime.Today;
        DocumentNumber            = "PT00067";
        Entries.Clear();
        RecalculateTotals();
    }

    private void PopulateFormFromCurrent()
    {
        if (CurrentReceipt is null) return;

        SelectedCustomer          = Customers.FirstOrDefault(c => c.Id == CurrentReceipt.CustomerId);
        PayerName                 = CurrentReceipt.PayerName;
        Address                   = CurrentReceipt.Address;
        SelectedPaymentReason     = CurrentReceipt.PaymentReason;
        SelectedCollectorEmployee = Employees.FirstOrDefault(e => e.Id == CurrentReceipt.CollectorEmployeeId);
        Attachment                = CurrentReceipt.Attachment;
        Reference                 = CurrentReceipt.Reference;
        AccountingDate            = CurrentReceipt.AccountingDate.ToLocalTime();
        DocumentDate              = CurrentReceipt.DocumentDate.ToLocalTime();
        DocumentNumber            = CurrentReceipt.DocumentNumber;

        Entries.Clear();
        foreach (var e in CurrentReceipt.Entries)
        {
            var item = new ReceiptEntryItem
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

    private CreateReceiptRequestDto BuildCreateRequest() => new()
    {
        CustomerId          = SelectedCustomer!.Id,
        PayerName           = PayerName.Trim(),
        Address             = string.IsNullOrWhiteSpace(Address)         ? null : Address.Trim(),
        PaymentReason       = SelectedPaymentReason,
        CollectorEmployeeId = SelectedCollectorEmployee?.Id,
        Attachment          = string.IsNullOrWhiteSpace(Attachment)      ? null : Attachment.Trim(),
        Reference           = string.IsNullOrWhiteSpace(Reference)       ? null : Reference.Trim(),
        AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date,    DateTimeKind.Unspecified),
        DocumentNumber      = DocumentNumber.Trim(),
        Entries             = Entries.Select(ToEntryDto).ToList(),
    };

    private UpdateReceiptRequestDto BuildUpdateRequest() => new()
    {
        CustomerId          = SelectedCustomer!.Id,
        PayerName           = PayerName.Trim(),
        Address             = string.IsNullOrWhiteSpace(Address)         ? null : Address.Trim(),
        PaymentReason       = SelectedPaymentReason,
        CollectorEmployeeId = SelectedCollectorEmployee?.Id,
        Attachment          = string.IsNullOrWhiteSpace(Attachment)      ? null : Attachment.Trim(),
        Reference           = string.IsNullOrWhiteSpace(Reference)       ? null : Reference.Trim(),
        AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date,    DateTimeKind.Unspecified),
        DocumentNumber      = DocumentNumber.Trim(),
        Entries             = Entries.Select(ToEntryDto).ToList(),
    };

    private static ReceiptEntryDto ToEntryDto(ReceiptEntryItem item) => new()
    {
        Description   = item.Description,
        DebitAccount  = item.DebitAccount,
        CreditAccount = item.CreditAccount,
        Amount        = item.Amount,
        SubjectCode   = item.SubjectCode,
        SubjectName   = item.SubjectName,
        BankAccount   = item.BankAccount,
    };

    private void NavigateToReceipt(int id)
    {
        var idx = _receiptListCache.FindIndex(r => r.Id == id);
        if (idx >= 0)
        {
            _currentIndex  = idx;
            CurrentReceipt = _receiptListCache[idx];
            PopulateFormFromCurrent();
        }
    }
}
