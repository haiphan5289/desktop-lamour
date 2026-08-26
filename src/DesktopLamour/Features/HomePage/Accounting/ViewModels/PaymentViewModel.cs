// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Data.Storage;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Views;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class PaymentViewModel : ViewModelBase
{
    public event Action? PaymentSaved;
    public event Action? RequestClose;
    private readonly IGetPaymentsUseCase          _getPayments;
    private readonly IGetPaymentByIdUseCase       _getPaymentById;
    private readonly ICreatePaymentUseCase        _createPayment;
    private readonly IUpdatePaymentUseCase        _updatePayment;
    private readonly IDeletePaymentUseCase        _deletePayment;
    private readonly IConfirmPaymentUseCase       _confirmPayment;
    private readonly IUnconfirmPaymentUseCase     _unconfirmPayment;
    private readonly ISetPaymentTreoUseCase       _setPaymentTreo;
    private readonly IGetSuppliersUseCase         _getSuppliers;
    private readonly IGetCustomersUseCase         _getCustomers;
    private readonly IGetEmployeesUseCase         _getEmployees;
    private readonly IGetExpenseCategoriesUseCase _getExpenseCategories;
    private readonly IGetAccountSettingsUseCase   _getAccountSettings;
    private readonly ILastUsedPaymentAccountsStore _lastUsedAccounts;
    private readonly Func<EmployeeFormWindow>     _employeeFormWindowFactory;
    private readonly Func<PaymentPrintWindow>     _printWindowFactory;
    private readonly ILogger<PaymentViewModel>    _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private bool    _isEditing;

    // ── Header — Thông tin chung ──────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedPartner;
    [ObservableProperty] private IReadOnlyList<ISearchableItem> _partnerItems = Array.Empty<ISearchableItem>();
    [ObservableProperty] private string  _payeeName             = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string  _selectedPaymentReason = "ChiKhac";
    [ObservableProperty] private string? _reasonDetail;
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
    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> AccountSettings { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ExpenseCategory> ExpenseCategories { get; private set; }
        = Array.Empty<ExpenseCategory>();

    public IReadOnlyList<string> PaymentReasons { get; } = new[]
    {
        "ChiKhac", "ChiMuaHang", "ChiTraNo"
    };

    // Chỉ Nháp (hoặc phiếu mới, chưa lưu) mới cho phép sửa — phiếu đã Ghi số là bất biến.
    public bool CanEdit => CurrentPayment is null || CurrentPayment.Status != "Confirmed";
    public bool CanPrint => CurrentPayment is not null;
    public bool CanUnconfirm => CurrentPayment is not null && CurrentPayment.Status == "Confirmed";

    private List<PaymentResponseDto> _receiptListCache = new();
    private int _currentIndex = -1;

    public PaymentViewModel(
        IGetPaymentsUseCase          getPayments,
        IGetPaymentByIdUseCase       getPaymentById,
        ICreatePaymentUseCase        createPayment,
        IUpdatePaymentUseCase        updatePayment,
        IDeletePaymentUseCase        deletePayment,
        IConfirmPaymentUseCase       confirmPayment,
        IUnconfirmPaymentUseCase     unconfirmPayment,
        ISetPaymentTreoUseCase       setPaymentTreo,
        IGetSuppliersUseCase         getSuppliers,
        IGetCustomersUseCase         getCustomers,
        IGetEmployeesUseCase         getEmployees,
        IGetExpenseCategoriesUseCase getExpenseCategories,
        IGetAccountSettingsUseCase   getAccountSettings,
        ILastUsedPaymentAccountsStore lastUsedAccounts,
        Func<EmployeeFormWindow>     employeeFormWindowFactory,
        Func<PaymentPrintWindow>     printWindowFactory,
        ILogger<PaymentViewModel>    logger)
    {
        _getPayments               = getPayments;
        _getPaymentById            = getPaymentById;
        _createPayment             = createPayment;
        _updatePayment             = updatePayment;
        _deletePayment             = deletePayment;
        _confirmPayment            = confirmPayment;
        _unconfirmPayment          = unconfirmPayment;
        _setPaymentTreo            = setPaymentTreo;
        _getSuppliers              = getSuppliers;
        _getCustomers              = getCustomers;
        _getEmployees              = getEmployees;
        _getExpenseCategories      = getExpenseCategories;
        _getAccountSettings        = getAccountSettings;
        _lastUsedAccounts          = lastUsedAccounts;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _printWindowFactory        = printWindowFactory;
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
            var suppliers        = await _getSuppliers.ExecuteAsync(ct);
            var customers        = await _getCustomers.ExecuteAsync(ct);
            var employees        = await _getEmployees.ExecuteAsync(ct);
            var expenseCategories = await _getExpenseCategories.ExecuteAsync(ct);
            var accountSettings  = await _getAccountSettings.ExecuteAsync(ct);

            Suppliers         = suppliers.Cast<ISearchableItem>().ToList().AsReadOnly();
            Customers         = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            Employees         = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            ExpenseCategories = expenseCategories.ToList().AsReadOnly();
            AccountSettings   = accountSettings.Cast<ISearchableItem>().ToList().AsReadOnly();

            OnPropertyChanged(nameof(Suppliers));
            OnPropertyChanged(nameof(Customers));
            OnPropertyChanged(nameof(Employees));
            OnPropertyChanged(nameof(ExpenseCategories));
            OnPropertyChanged(nameof(AccountSettings));

            // "Đối tượng" — 1 ô tìm kiếm chung cho cả 3 loại (khớp ảnh mẫu MISA: không có combo
            // "chọn loại đối tượng" riêng, gõ mã gì cũng tìm ra đúng nguồn tương ứng).
            PartnerItems = Suppliers.Concat(Customers).Concat(Employees).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload lookup data for PaymentWindow");
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────

    // "🔄 Làm mới" — refresh danh sách phiếu chi từ server (không đổi trạng thái gì).
    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct = default)
        => await LoadPaymentsAsync(ct);

    // "📌 Treo" — lưu phiếu (thay cho nút Cất đã bỏ). Đang Nháp thì lưu + chuyển sang Treo;
    // đã Treo rồi thì chỉ lưu lại thay đổi, không gọi lại endpoint /treo.
    [RelayCommand]
    private async Task TreoAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (CurrentPayment is not null && CurrentPayment.Status == "Confirmed")
        {
            HasError     = true;
            ErrorMessage = "Phiếu chi đã ghi số, không thể sửa.";
            return;
        }

        var isDraft = CurrentPayment is null || CurrentPayment.Status == "Draft";

        var savedId = await PersistAsync(ct);
        if (savedId is null) return;

        if (!isDraft) // đã Treo — vừa lưu lại thay đổi, không cần đổi trạng thái nữa
        {
            IsEditing = false;
            PaymentSaved?.Invoke();
            RequestClose?.Invoke();
            return;
        }

        IsBusy = true;
        try
        {
            var treo = await _setPaymentTreo.ExecuteAsync(savedId.Value, ct);
            _logger.LogInformation("Payment set to Treo: {Id}", treo.Id);
            await LoadPaymentsAsync(ct);
            NavigateToPayment(treo.Id);

            IsEditing = false;
            PaymentSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set payment to Treo");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

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
    private async Task ConfirmAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (CurrentPayment is not null && CurrentPayment.Status != "Treo")
        {
            HasError     = true;
            ErrorMessage = CurrentPayment.Status == "Confirmed"
                ? "Phiếu chi này đã được ghi số trước đó."
                : "Chỉ phiếu chi ở trạng thái Treo mới có thể ghi số. Vui lòng bấm Treo trước.";
            return;
        }

        if (Entries.Count == 0)
        {
            HasError     = true;
            ErrorMessage = "Phiếu chi phải có ít nhất 1 dòng hạch toán.";
            return;
        }

        var savedId = await PersistAsync(ct);
        if (savedId is null) return;

        IsBusy = true;
        try
        {
            var confirmed = await _confirmPayment.ExecuteAsync(savedId.Value, ct);
            _logger.LogInformation("Payment confirmed: {Id}", confirmed.Id);
            await LoadPaymentsAsync(ct);
            NavigateToPayment(confirmed.Id);

            IsEditing = false;
            PaymentSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm payment");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    // "↩️ Hoàn" (MISA) — hủy Ghi số, đưa phiếu về Treo (mirror UnconfirmWarehouseReceiptUseCase).
    [RelayCommand(CanExecute = nameof(CanUnconfirm))]
    private async Task UnconfirmAsync(CancellationToken ct = default)
    {
        if (CurrentPayment is null || CurrentPayment.Status != "Confirmed") return;

        HasError     = false;
        ErrorMessage = string.Empty;
        IsBusy       = true;
        try
        {
            var reverted = await _unconfirmPayment.ExecuteAsync(CurrentPayment.Id, ct);
            _logger.LogInformation("Payment unconfirmed: {Id}", reverted.Id);
            await LoadPaymentsAsync(ct);
            NavigateToPayment(reverted.Id);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unconfirm payment");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    /// Lưu (create/update), trả về Id của phiếu vừa lưu — hoặc null nếu lưu thất bại.
    private async Task<int?> PersistAsync(CancellationToken ct)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (SelectedPartner is null)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng chọn đối tượng.";
            return null;
        }

        IsBusy = true;
        try
        {
            int id;
            if (CurrentPayment is null)
            {
                var request = BuildCreateRequest();
                var result  = await _createPayment.ExecuteAsync(request, ct);
                _logger.LogInformation("Payment created: {DocumentNumber}", result.DocumentNumber);
                id = result.Id;
            }
            else
            {
                var request = BuildUpdateRequest();
                var result  = await _updatePayment.ExecuteAsync(CurrentPayment.Id, request, ct);
                _logger.LogInformation("Payment updated: {Id}", result.Id);
                id = result.Id;
            }

            await LoadPaymentsAsync(ct);
            NavigateToPayment(id);
            return id;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save payment");
            HasError     = true;
            ErrorMessage = ex.Message;
            return null;
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
    private void Edit()
    {
        if (CurrentPayment is not null && CurrentPayment.Status == "Confirmed")
        {
            MessageBox.Show(
                "Phiếu chi đã ghi số, không thể sửa.",
                "Không thể sửa",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanPrint))]
    private void Print()
    {
        if (CurrentPayment is null) return;
        var window = _printWindowFactory();
        window.Initialize(CurrentPayment);
        window.ShowDialog();
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
            SelectedDebitAccount  = AccountSettings.FirstOrDefault(a => a.Id == _lastUsedAccounts.LastDebitAccountId),
            SelectedCreditAccount = AccountSettings.FirstOrDefault(a => a.Id == _lastUsedAccounts.LastCreditAccountId),
            SubjectCode           = SelectedPartner?.Code,
            SubjectName           = SelectedPartner?.Name,
        };
        AttachEntryHandlers(entry);
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

    partial void OnSelectedPartnerChanged(ISearchableItem? value)
    {
        if (value is not null)
        {
            PayeeName = value.Name;

            // Auto-populate Address — chỉ Supplier/Customer có field Address, Employee thì không.
            Address = value switch
            {
                DesktopLamour.Features.HomePage.Suppliers.Domain.Models.Supplier supplier => supplier.Address,
                DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer customer => customer.Address,
                _ => Address,
            };

            // Khớp ảnh mẫu MISA — đổi "Đối tượng" ở header đồng bộ luôn "Đối tượng"/"Tên đối tượng"
            // xuống mọi dòng hạch toán hiện có (dòng mới thêm sau đó cũng mặc định theo AddEntry()).
            foreach (var entry in Entries)
            {
                entry.SubjectCode = value.Code;
                entry.SubjectName = value.Name;
            }
        }
    }

    // "Đối tượng" là 1 ô tìm kiếm chung (PartnerItems gộp Suppliers/Customers/Employees) — loại
    // được suy ra từ kiểu runtime của object đã chọn, không cần combo "chọn loại" riêng cho user.
    private static string ResolvePartnerType(ISearchableItem partner) => partner switch
    {
        DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer => "Customer",
        DesktopLamour.Features.HomePage.Employees.Domain.Models.Employee => "Employee",
        _ => "Supplier",
    };

    partial void OnCurrentPaymentChanged(PaymentResponseDto? value)
    {
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanPrint));
        OnPropertyChanged(nameof(CanUnconfirm));
        PrintCommand.NotifyCanExecuteChanged();
        UnconfirmCommand.NotifyCanExecuteChanged();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    // Ghi nhớ TK Nợ/TK Có vừa chọn để dòng mới lần sau mặc định theo đó. Không cần đồng bộ
    // tên hiển thị thủ công nữa — SelectedDebitAccount/SelectedCreditAccount/SelectedExpenseCategory
    // giữ nguyên object, CellTemplate đọc trực tiếp qua property path (VD: SelectedDebitAccount.DisplayText).
    private void AttachEntryHandlers(PaymentEntryItem entry)
    {
        entry.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(PaymentEntryItem.SelectedDebitAccount):
                    _lastUsedAccounts.LastDebitAccountId = entry.SelectedDebitAccount?.Id;
                    break;
                case nameof(PaymentEntryItem.SelectedCreditAccount):
                    _lastUsedAccounts.LastCreditAccountId = entry.SelectedCreditAccount?.Id;
                    break;
            }
            RecalculateTotals();
        };
    }

    private void ClearForm()
    {
        SelectedPartner           = null;
        PayeeName                 = string.Empty;
        Address                   = null;
        SelectedPaymentReason     = "ChiKhac";
        ReasonDetail              = null;
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

        SelectedPartner           = CurrentPayment.PartnerType switch
        {
            "Customer" => Customers.FirstOrDefault(c => c.Id == CurrentPayment.PartnerId),
            "Employee" => Employees.FirstOrDefault(c => c.Id == CurrentPayment.PartnerId),
            _          => Suppliers.FirstOrDefault(c => c.Id == CurrentPayment.PartnerId),
        };
        PayeeName                 = CurrentPayment.PayeeName;
        Address                   = CurrentPayment.Address;
        SelectedPaymentReason     = CurrentPayment.PaymentReason;
        ReasonDetail              = CurrentPayment.ReasonDetail;
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
                Description            = e.Description,
                SelectedDebitAccount    = AccountSettings.FirstOrDefault(a => a.Id == e.DebitAccountId),
                SelectedCreditAccount   = AccountSettings.FirstOrDefault(a => a.Id == e.CreditAccountId),
                Amount                  = e.Amount,
                SubjectCode             = e.SubjectCode,
                SubjectName             = e.SubjectName,
                BankAccount             = e.BankAccount,
                SelectedExpenseCategory = ExpenseCategories.FirstOrDefault(c => c.Id == e.ExpenseCategoryId),
            };
            AttachEntryHandlers(item);
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
        PartnerType          = ResolvePartnerType(SelectedPartner!),
        PartnerId            = SelectedPartner!.Id,
        PayeeName           = PayeeName.Trim(),
        Address             = string.IsNullOrWhiteSpace(Address)         ? null : Address.Trim(),
        PaymentReason       = SelectedPaymentReason,
        ReasonDetail        = string.IsNullOrWhiteSpace(ReasonDetail)    ? null : ReasonDetail.Trim(),
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
        PartnerType          = ResolvePartnerType(SelectedPartner!),
        PartnerId            = SelectedPartner!.Id,
        PayeeName           = PayeeName.Trim(),
        Address             = string.IsNullOrWhiteSpace(Address)         ? null : Address.Trim(),
        PaymentReason       = SelectedPaymentReason,
        ReasonDetail        = string.IsNullOrWhiteSpace(ReasonDetail)    ? null : ReasonDetail.Trim(),
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
        Description       = item.Description,
        DebitAccountId     = item.SelectedDebitAccount?.Id ?? 0,
        CreditAccountId    = item.SelectedCreditAccount?.Id ?? 0,
        Amount             = item.Amount,
        SubjectCode        = item.SubjectCode,
        SubjectName        = item.SubjectName,
        BankAccount        = item.BankAccount,
        ExpenseCategoryId  = item.SelectedExpenseCategory?.Id,
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
