// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class ReceiptViewModel : ViewModelBase
{
    // 2026-09-26: hiện sẵn N dòng trống để gõ ngay — xem ghi chú đầy đủ ở
    // PaymentViewModel.InitialEmptyLineCount (cùng lý do, cùng giá trị).
    private const int InitialEmptyLineCount = 50;
    public event Action? ReceiptSaved;
    public event Action? RequestClose;
    private readonly IGetReceiptsUseCase      _getReceipts;
    private readonly IGetReceiptByIdUseCase   _getReceiptById;
    private readonly ICreateReceiptUseCase    _createReceipt;
    private readonly IUpdateReceiptUseCase    _updateReceipt;
    private readonly IDeleteReceiptUseCase    _deleteReceipt;
    private readonly IConfirmReceiptUseCase   _confirmReceipt;
    private readonly IUnconfirmReceiptUseCase _unconfirmReceipt;
    private readonly IGetNextReceiptCodeUseCase _getNextCode;
    private readonly IGetCustomersUseCase     _getCustomers;
    private readonly IGetEmployeesUseCase     _getEmployees;
    private readonly Func<EmployeeFormWindow> _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow> _customerFormWindowFactory;
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

    partial void OnCurrentReceiptChanged(ReceiptResponseDto? value)
    {
        NavigatePrevCommand.NotifyCanExecuteChanged();
        NavigateNextCommand.NotifyCanExecuteChanged();
        NotifyEditStateChanged();
    }

    partial void OnIsEditingChanged(bool value) => NotifyEditStateChanged();

    private void NotifyEditStateChanged()
    {
        OnPropertyChanged(nameof(IsConfirmed));
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(UnpostButtonLabel));
        EditCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        ToggleConfirmCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    // CanExecute cho NavigatePrev/NavigateNextCommand — WPF tự disable (mờ) nút khi đang ở
    // đầu/cuối danh sách, không phải ẩn hẳn (nút vẫn nằm đúng chỗ trong toolbar).
    public bool CanNavigatePrev => _currentIndex > 0;
    public bool CanNavigateNext => _currentIndex >= 0 && _currentIndex < _receiptListCache.Count - 1;

    // ── Trạng thái popup — 2026-09-26: khớp ĐÚNG quy trình Chứng từ bán hàng
    // (Sales/docs/ChungTuTraHangBan-Review.html, SalesOrderViewModel) — giống hệt PaymentViewModel:
    //   • Mở phiếu có sẵn → form KHÓA; bấm "Sửa" (chỉ khi chưa ghi sổ = Treo) mới nhập được.
    //   • "Cất" = CHỈ LƯU (phiếu ở Treo), xong form tự khóa lại, popup vẫn mở. 2026-09-26: kế toán
    //     chốt phiếu thu thường làm GIỐNG phiếu thu hàng loạt (khớp MISA) — trước đây Cất = Ghi sổ ngay.
    //     Ghi sổ là bước riêng qua nút toggle bên dưới (hoặc 📗 Ghi sổ trên màn Quỹ).
    //   • Nút "Ghi sổ / Bỏ ghi" dùng chung 1 nút toggle, chỉ bấm được khi form đang khóa, không hỏi lại.
    //   • "Xóa" chỉ khi chưa ghi sổ + form đang khóa, có hỏi Yes/No, xóa xong đóng popup.
    // "Draft" của phiếu thu = Treo (không còn khái niệm "Nháp").
    public bool IsConfirmed => CurrentReceipt is not null && CurrentReceipt.Status == "Confirmed";
    public bool IsEditable  => CurrentReceipt is null || (IsEditing && !IsConfirmed);
    private bool CanEdit    => CurrentReceipt is not null && !IsEditing && !IsConfirmed;
    private bool CanToggleConfirm => CurrentReceipt is not null && !IsEditing;
    public string UnpostButtonLabel => IsConfirmed ? "Bỏ ghi" : "Ghi sổ";
    public bool CanDelete   => CurrentReceipt is not null && !IsConfirmed && !IsEditing;

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
        IConfirmReceiptUseCase   confirmReceipt,
        IUnconfirmReceiptUseCase unconfirmReceipt,
        IGetNextReceiptCodeUseCase getNextCode,
        IGetCustomersUseCase     getCustomers,
        IGetEmployeesUseCase     getEmployees,
        Func<EmployeeFormWindow> employeeFormWindowFactory,
        Func<CustomerFormWindow> customerFormWindowFactory,
        ILogger<ReceiptViewModel> logger)
    {
        _getReceipts               = getReceipts;
        _getReceiptById            = getReceiptById;
        _createReceipt             = createReceipt;
        _updateReceipt             = updateReceipt;
        _deleteReceipt             = deleteReceipt;
        _confirmReceipt            = confirmReceipt;
        _unconfirmReceipt          = unconfirmReceipt;
        _getNextCode               = getNextCode;
        _getCustomers              = getCustomers;
        _getEmployees              = getEmployees;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _customerFormWindowFactory = customerFormWindowFactory;
        _logger                    = logger;

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
    private async Task AddNewAsync(CancellationToken ct = default)
    {
        CurrentReceipt        = null;
        _currentIndex         = -1;
        IsEditing             = true;
        ClearForm();
        for (var i = 0; i < InitialEmptyLineCount; i++) AddEntry();

        // Số chứng từ tự sinh dạng PT{5 số} — khớp GetNextSalesOrderCodeUseCase; ClearForm() đã set
        // placeholder tĩnh, ở đây gọi BE lấy số thật ngay khi mở form Thêm mới. Giữ nguyên placeholder
        // nếu gọi lỗi (offline...) thay vì chặn user nhập tay.
        try
        {
            DocumentNumber = await _getNextCode.ExecuteAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch next receipt code, keeping placeholder");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        IsEditing = true;
        // PopulateFormFromCurrent (đã chạy trước đó) chỉ nạp đúng số dòng THẬT — thêm dòng trống để
        // gõ thêm ngay, không phải tự bấm "+ Thêm dòng" nhiều lần (mirror SalesOrderViewModel.Edit).
        for (var i = 0; i < InitialEmptyLineCount; i++) AddEntry();
    }

    // "💾 Cất" = chỉ lưu, phiếu ở Treo — KHÔNG tự Ghi sổ (xem comment trạng thái popup ở trên).
    [RelayCommand(CanExecute = nameof(IsEditable))]
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

        // Lọc dòng trống (Amount = 0 — chưa gõ tới, còn nguyên từ InitialEmptyLineCount) trước khi
        // validate/gửi BE, khớp SalesOrderViewModel (Lines.Where(l => l.ProductId > 0)).
        if (!Entries.Any(e => e.Amount != 0))
        {
            HasError     = true;
            ErrorMessage = "Vui lòng nhập ít nhất một dòng hạch toán.";
            return;
        }

        IsBusy = true;
        try
        {
            ReceiptResponseDto result;
            if (CurrentReceipt is null)
            {
                // Create
                var request = BuildCreateRequest();
                result = await _createReceipt.ExecuteAsync(request, ct);
                _logger.LogInformation("Receipt created: {DocumentNumber}", result.DocumentNumber);
            }
            else
            {
                // Update
                var request = BuildUpdateRequest();
                result = await _updateReceipt.ExecuteAsync(CurrentReceipt.Id, request, ct);
                _logger.LogInformation("Receipt updated: {Id}", result.Id);
            }

            // 2026-09-26: bỏ bước tự Confirm sau khi lưu — Create/Update của BE để phiếu ở Draft
            // (= Treo), phiếu chỉ lên sổ quỹ khi kế toán bấm "Ghi sổ" riêng (ToggleConfirmAsync /
            // màn Quỹ). Phiếu Treo vẫn hiện trên sổ quỹ (không tính vào số tồn), nên không bị "kẹt".

            await LoadReceiptsAsync(ct);
            NavigateToReceipt(result.Id);

            // Cất xong khóa form lại, GIỮ popup mở (khớp Chứng từ bán hàng).
            IsEditing = false;
            ReceiptSaved?.Invoke();
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

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentReceipt is null) return;

        // Khớp Chứng từ bán hàng: Xóa có hỏi Yes/No, xóa xong đóng popup.
        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{CurrentReceipt.DocumentNumber}'?",
            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _deleteReceipt.ExecuteAsync(CurrentReceipt.Id, ct);
            _logger.LogInformation("Receipt deleted: {Id}", CurrentReceipt.Id);
            ReceiptSaved?.Invoke();
            RequestClose?.Invoke();
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

    // "↩️ Bỏ ghi / 📗 Ghi sổ" — 1 nút toggle, mỗi chiều 1 lần bấm, KHÔNG hỏi xác nhận (khớp Chứng từ
    // bán hàng — trước đây Bỏ ghi phiếu thu có hỏi Yes/No). Bỏ ghi: BE UnconfirmReceiptUseCase tự xóa
    // bút toán quỹ đã post, phiếu về Treo, form vẫn khóa — phải bấm "Sửa" mới nhập được.
    [RelayCommand(CanExecute = nameof(CanToggleConfirm))]
    private async Task ToggleConfirmAsync(CancellationToken ct = default)
    {
        if (CurrentReceipt is null) return;

        IsBusy = true;
        try
        {
            var result = IsConfirmed
                ? await _unconfirmReceipt.ExecuteAsync(CurrentReceipt.Id, ct)
                : await _confirmReceipt.ExecuteAsync(CurrentReceipt.Id, ct);
            _logger.LogInformation("Receipt {Id} toggled confirm — new status {Status}", result.Id, result.Status);
            await LoadReceiptsAsync(ct);
            NavigateToReceipt(result.Id);
            ReceiptSaved?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle confirm for receipt");
            MessageBox.Show(ex.Message, "Thao tác thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrev))]
    private void NavigatePrev()
    {
        if (_receiptListCache.Count == 0 || _currentIndex <= 0) return;
        _currentIndex--;
        IsEditing      = false;
        CurrentReceipt = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void NavigateNext()
    {
        if (_receiptListCache.Count == 0 || _currentIndex >= _receiptListCache.Count - 1) return;
        _currentIndex++;
        IsEditing      = false;
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
    private async Task AddCollectorEmployeeAsync(CancellationToken ct = default)
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
            if (newItem is not null) SelectedCollectorEmployee = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload employees after add"); }
    }

    // ── Partial property change hooks ─────────────────────────────────────

    partial void OnSelectedCustomerChanged(ISearchableItem? value)
    {
        if (value is not null)
        {
            PayerName = value.Name;
            
            // Auto-populate Address from Customer
            if (value is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer customer)
            {
                Address = customer.Address;
            }
        }
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
                SalesOrderId  = e.SalesOrderId,
            };
            item.PropertyChanged += (_, _) => RecalculateTotals();
            Entries.Add(item);
        }

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalAmount   = Entries.Sum(e => e.Amount);
        // Đếm dòng THẬT (đã nhập Amount) — không tính dòng trống hiện sẵn để gõ.
        EntrySummary  = $"Số dòng = {Entries.Count(e => e.Amount != 0)}";
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
        // Bỏ dòng trống (Amount = 0) — xem ghi chú ở InitialEmptyLineCount/SaveAsync.
        Entries             = Entries.Where(e => e.Amount != 0).Select(ToEntryDto).ToList(),
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
        // Bỏ dòng trống (Amount = 0) — xem ghi chú ở InitialEmptyLineCount/SaveAsync.
        Entries             = Entries.Where(e => e.Amount != 0).Select(ToEntryDto).ToList(),
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
        SalesOrderId  = item.SalesOrderId,
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

    // Mở màn "Xem" 1 phiếu thu cụ thể từ nơi khác (VD: double-click 1 dòng trên "Sổ Kế Toán Chi
    // Tiết Quỹ Tiền Mặt") — chỉ có DocumentNumber (CashLedgerEntryDto không mang ReceiptId), nên
    // tìm theo DocumentNumber thay vì Id trong _receiptListCache đã LoadAsync sẵn. Gọi SAU khi
    // ReceiptWindow.OnContentRendered đã LoadAsync — xem ReceiptWindow.InitialDocumentNumber.
    public void NavigateToReceiptByDocumentNumber(string documentNumber)
    {
        var idx = _receiptListCache.FindIndex(r => r.DocumentNumber == documentNumber);
        if (idx >= 0)
        {
            _currentIndex  = idx;
            CurrentReceipt = _receiptListCache[idx];
            PopulateFormFromCurrent();
        }
    }
}
