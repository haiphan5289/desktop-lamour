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
    // 2026-09-26: hiện sẵn N dòng trống để gõ ngay, đỡ phải bấm "+ Thêm dòng" nhiều lần (mirror
    // SalesOrderViewModel/SalesReturnViewModel InitialEmptyLineCount, giá trị nhỏ hơn vì phiếu chi
    // hiếm khi có >10 dòng hạch toán thật). QUAN TRỌNG: dòng trống (Amount = 0, chưa chọn TK) phải bị
    // lọc khỏi request lúc lưu (xem BuildCreateRequest/BuildUpdateRequest) — nếu gửi thẳng lên BE,
    // CreatePaymentUseCase sẽ ném "Tài khoản Nợ không tồn tại." vì SelectedDebitAccount null → Id=0.
    private const int InitialEmptyLineCount = 50;
    public event Action? PaymentSaved;
    public event Action? RequestClose;
    private readonly IGetPaymentsUseCase          _getPayments;
    private readonly IGetPaymentByIdUseCase       _getPaymentById;
    private readonly ICreatePaymentUseCase        _createPayment;
    private readonly IUpdatePaymentUseCase        _updatePayment;
    private readonly IDeletePaymentUseCase        _deletePayment;
    private readonly IConfirmPaymentUseCase       _confirmPayment;
    private readonly IUnconfirmPaymentUseCase     _unconfirmPayment;
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

    // 2026-09-29 (khớp MISA): 4 lý do chi. Phiếu cũ có lý do đã bỏ (ChiMuaHang/ChiTraNo/ChiLuong) thì
    // thêm đúng giá trị đó vào danh sách khi mở phiếu để ô vẫn hiện được (xem RefreshPaymentReasons).
    private static readonly string[] SelectablePaymentReasons =
    {
        "TamUngNhanVien", "GuiTienNganHang", "ChiKhac", "ThueTNDNTamTinh",
    };

    public IReadOnlyList<string> PaymentReasons { get; private set; } = SelectablePaymentReasons;

    private void RefreshPaymentReasons(string? currentReason)
    {
        PaymentReasons = string.IsNullOrEmpty(currentReason) || SelectablePaymentReasons.Contains(currentReason)
            ? SelectablePaymentReasons
            : SelectablePaymentReasons.Append(currentReason).ToArray();
        OnPropertyChanged(nameof(PaymentReasons));
    }

    // 2026-09-29 (khớp MISA bước 3-4): dòng hạch toán tự điền "Diễn giải" theo nội dung chi tiết ở ô
    // bên cạnh Lý do chi, và đổi theo khi gõ tiếp — CHỈ với dòng còn trống hoặc đang mang đúng nội dung
    // cũ (chưa bị người dùng tự sửa). Dòng đã có diễn giải riêng thì giữ nguyên.
    partial void OnReasonDetailChanged(string? oldValue, string? newValue)
    {
        var oldText = oldValue?.Trim() ?? "";
        var newText = newValue?.Trim() ?? "";
        foreach (var entry in Entries.Where(IsActiveEntry))
        {
            if (string.IsNullOrWhiteSpace(entry.Description) || entry.Description.Trim() == oldText)
                entry.Description = newText;
        }
    }

    // 2026-10-01 (giống ReceiptViewModel): form mở sẵn nhiều dòng trống, lưới trống hẳn như các popup chứng
    // từ khác. Chỉ tự điền (Diễn giải, TK Nợ/Có, Đối tượng) cho dòng ĐẦU sau khi đã chọn Đối tượng và cho
    // dòng đã có số tiền.
    private bool IsActiveEntry(PaymentEntryItem entry) =>
        entry.Amount != 0
        || (SelectedPartner is not null && Entries.Count > 0 && ReferenceEquals(Entries[0], entry));

    // Điền các ô còn trống của 1 dòng theo header; không ghi đè ô người dùng đã nhập. Mặc định TK Nợ 6418 /
    // TK Có 1111 (theo kế toán); danh mục chưa có mã đó thì rơi về TK dùng gần nhất.
    private void ApplyEntryDefaults(PaymentEntryItem entry)
    {
        entry.SelectedDebitAccount  ??= FindAccountByCode(DefaultDebitAccountCode)
                                        ?? AccountSettings.FirstOrDefault(a => a.Id == _lastUsedAccounts.LastDebitAccountId);
        entry.SelectedCreditAccount ??= FindAccountByCode(DefaultCreditAccountCode)
                                        ?? AccountSettings.FirstOrDefault(a => a.Id == _lastUsedAccounts.LastCreditAccountId);
        if (string.IsNullOrWhiteSpace(entry.Description)) entry.Description = ReasonDetail?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(entry.SubjectCode)) entry.SubjectCode = SelectedPartner?.Code;
        if (string.IsNullOrWhiteSpace(entry.SubjectName)) entry.SubjectName = SelectedPartner?.Name;
    }

    // ── Trạng thái popup — 2026-09-26: khớp ĐÚNG quy trình Chứng từ bán hàng
    // (Sales/docs/ChungTuTraHangBan-Review.html, SalesOrderViewModel):
    //   • Mở phiếu có sẵn → form KHÓA; bấm "Sửa" (chỉ khi chưa ghi sổ) mới nhập được.
    //   • "Cất" = Ghi sổ ngay, xong form tự khóa lại, popup vẫn mở (để In).
    //   • Nút "Ghi sổ / Bỏ ghi" dùng chung 1 nút toggle, chỉ bấm được khi form đang khóa, không hỏi lại.
    //   • "Xóa" chỉ khi chưa ghi sổ + form đang khóa, có hỏi Yes/No, xóa xong đóng popup.
    //   • Không còn nút "Treo" (trạng thái chưa ghi sổ chỉ còn là kết quả của "Bỏ ghi").
    public bool IsConfirmed => CurrentPayment is not null && CurrentPayment.Status == "Confirmed";
    public bool IsEditable  => CurrentPayment is null || (IsEditing && !IsConfirmed);
    private bool CanEdit    => CurrentPayment is not null && !IsEditing && !IsConfirmed;
    public bool CanPrint    => CurrentPayment is not null;
    private bool CanToggleConfirm => CurrentPayment is not null && !IsEditing;
    public string UnpostButtonLabel => IsConfirmed ? "Bỏ ghi" : "Ghi sổ";
    public bool CanDelete   => CurrentPayment is not null && !IsConfirmed && !IsEditing;

    // CanExecute cho NavigatePrev/NavigateNextCommand — WPF tự disable (mờ) nút khi đang ở
    // đầu/cuối danh sách, không phải ẩn hẳn (nút vẫn nằm đúng chỗ trong toolbar).
    public bool CanNavigatePrev => _currentIndex > 0;
    public bool CanNavigateNext => _currentIndex >= 0 && _currentIndex < _receiptListCache.Count - 1;

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
        for (var i = 0; i < InitialEmptyLineCount; i++) AddEntry();
    }

    // "💾 Cất" = lưu + Ghi sổ ngay (khớp Chứng từ bán hàng).
    [RelayCommand(CanExecute = nameof(IsEditable))]
    private async Task ConfirmAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        // 2026-09-26: bỏ bước bắt buộc "bấm Treo trước" — khớp Chứng từ bán hàng, "Cất" = Ghi sổ ngay
        // cho mọi phiếu chưa ghi sổ (BE ConfirmPaymentUseCase giờ nhận cả Draft lẫn Treo). Trước đây
        // phiếu chi MỚI bấm Cất luôn bị BE từ chối vì phiếu vừa tạo ở Draft.
        if (CurrentPayment is not null && CurrentPayment.Status == "Confirmed")
        {
            HasError     = true;
            ErrorMessage = "Phiếu chi này đã được ghi sổ trước đó.";
            return;
        }

        // Kiểm tra "ít nhất 1 dòng hạch toán" đã chuyển vào PersistAsync (dùng chung với Sửa/Cất
        // khác) — Entries.Count == 0 không còn đúng nữa vì Entries luôn có sẵn InitialEmptyLineCount
        // dòng trống ngay từ khi mở form.
        var savedId = await PersistAsync(ct);
        if (savedId is null) return;

        IsBusy = true;
        try
        {
            var confirmed = await _confirmPayment.ExecuteAsync(savedId.Value, ct);
            _logger.LogInformation("Payment confirmed: {Id}", confirmed.Id);
            await LoadPaymentsAsync(ct);
            NavigateToPayment(confirmed.Id);

            // Cất xong khóa form lại, GIỮ popup mở (khớp Chứng từ bán hàng — để bấm In ngay).
            IsEditing = false;
            PaymentSaved?.Invoke();
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

    // "↩️ Bỏ ghi / 📗 Ghi sổ" — 1 nút toggle (khớp Chứng từ bán hàng), mỗi chiều 1 lần bấm, không hỏi
    // xác nhận. Bỏ ghi đưa phiếu về Treo, form vẫn khóa — phải bấm "Sửa" mới nhập được.
    [RelayCommand(CanExecute = nameof(CanToggleConfirm))]
    private async Task ToggleConfirmAsync(CancellationToken ct = default)
    {
        if (CurrentPayment is null) return;

        HasError     = false;
        ErrorMessage = string.Empty;
        IsBusy       = true;
        try
        {
            var result = IsConfirmed
                ? await _unconfirmPayment.ExecuteAsync(CurrentPayment.Id, ct)
                : await _confirmPayment.ExecuteAsync(CurrentPayment.Id, ct);
            _logger.LogInformation("Payment {Id} toggled confirm — new status {Status}", result.Id, result.Status);
            await LoadPaymentsAsync(ct);
            NavigateToPayment(result.Id);
            PaymentSaved?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle confirm for payment");
            MessageBox.Show(ex.Message, "Thao tác thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        // Lọc dòng trống (Amount = 0 — chưa gõ tới, còn nguyên từ InitialEmptyLineCount) trước khi
        // validate/gửi BE, khớp SalesOrderViewModel (Lines.Where(l => l.ProductId > 0)).
        if (!Entries.Any(e => e.Amount != 0))
        {
            HasError     = true;
            ErrorMessage = "Vui lòng nhập ít nhất một dòng hạch toán.";
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

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentPayment is null) return;

        // Khớp Chứng từ bán hàng: Xóa có hỏi Yes/No, xóa xong đóng popup.
        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{CurrentPayment.DocumentNumber}'?",
            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _deletePayment.ExecuteAsync(CurrentPayment.Id, ct);
            _logger.LogInformation("Payment deleted: {Id}", CurrentPayment.Id);
            PaymentSaved?.Invoke();
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

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        IsEditing = true;
        // PopulateFormFromCurrent (đã chạy trước đó) chỉ nạp đúng số dòng THẬT — thêm dòng trống để
        // gõ thêm ngay, không phải tự bấm "+ Thêm dòng" nhiều lần (mirror SalesOrderViewModel.Edit).
        for (var i = 0; i < InitialEmptyLineCount; i++) AddEntry();
    }

    [RelayCommand(CanExecute = nameof(CanPrint))]
    private void Print()
    {
        if (CurrentPayment is null) return;
        var window = _printWindowFactory();
        window.Initialize(CurrentPayment);
        window.ShowDialog();
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrev))]
    private void NavigatePrev()
    {
        if (_receiptListCache.Count == 0 || _currentIndex <= 0) return;
        _currentIndex--;
        IsEditing      = false;
        CurrentPayment = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private void NavigateNext()
    {
        if (_receiptListCache.Count == 0 || _currentIndex >= _receiptListCache.Count - 1) return;
        _currentIndex++;
        IsEditing      = false;
        CurrentPayment = _receiptListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    private const string DefaultDebitAccountCode  = "6418";
    private const string DefaultCreditAccountCode = "1111";

    private ISearchableItem? FindAccountByCode(string code) =>
        AccountSettings.FirstOrDefault(a => string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase));

    [RelayCommand]
    private void AddEntry()
    {
        // Chỉ dòng đầu được điền sẵn (sau khi chọn Đối tượng); dòng trống phía sau tự điền khi gõ số tiền
        // (xem AttachEntryHandlers / ApplyEntryDefaults).
        var entry = new PaymentEntryItem();
        AttachEntryHandlers(entry);
        Entries.Add(entry);
        if (IsActiveEntry(entry)) ApplyEntryDefaults(entry);
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
            foreach (var entry in Entries.Where(IsActiveEntry))
            {
                entry.SubjectCode = value.Code;
                entry.SubjectName = value.Name;
                ApplyEntryDefaults(entry);
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
        NotifyEditStateChanged();
        OnPropertyChanged(nameof(CanPrint));
        PrintCommand.NotifyCanExecuteChanged();
        NavigatePrevCommand.NotifyCanExecuteChanged();
        NavigateNextCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditingChanged(bool value) => NotifyEditStateChanged();

    private void NotifyEditStateChanged()
    {
        OnPropertyChanged(nameof(IsConfirmed));
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(UnpostButtonLabel));
        EditCommand.NotifyCanExecuteChanged();
        ConfirmCommand.NotifyCanExecuteChanged();
        ToggleConfirmCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
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
                // Gõ số tiền vào 1 dòng trống → lúc đó mới tự điền dòng ấy.
                case nameof(PaymentEntryItem.Amount) when entry.Amount != 0 && IsEditable:
                    ApplyEntryDefaults(entry);
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
        RefreshPaymentReasons("ChiKhac");
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
        // Phải nạp danh sách TRƯỚC khi gán SelectedItem, không thì lý do cũ (ChiMuaHang...) bị ComboBox
        // ghi đè về null vì không có trong ItemsSource.
        RefreshPaymentReasons(CurrentPayment.PaymentReason);
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
        // Đếm dòng THẬT (đã nhập Amount) — không tính dòng trống hiện sẵn để gõ, khớp
        // SalesOrderViewModel.LineSummary (Lines.Count(l => l.ProductId > 0)).
        EntrySummary  = $"Số dòng = {Entries.Count(e => e.Amount != 0)}";
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
        // Bỏ dòng trống (Amount = 0) — xem ghi chú ở InitialEmptyLineCount/PersistAsync.
        Entries             = Entries.Where(e => e.Amount != 0).Select(ToEntryDto).ToList(),
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
        // Bỏ dòng trống (Amount = 0) — xem ghi chú ở InitialEmptyLineCount/PersistAsync.
        Entries             = Entries.Where(e => e.Amount != 0).Select(ToEntryDto).ToList(),
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

    // Mở màn "Xem" 1 phiếu chi cụ thể từ nơi khác (VD: double-click 1 dòng trên "Sổ Kế Toán Chi
    // Tiết Quỹ Tiền Mặt") — chỉ có DocumentNumber (CashLedgerEntryDto không mang PaymentId), nên
    // tìm theo DocumentNumber thay vì Id trong _receiptListCache đã LoadAsync sẵn. Gọi SAU khi
    // PaymentWindow.OnContentRendered đã LoadAsync — xem PaymentWindow.InitialDocumentNumber.
    public void NavigateToPaymentByDocumentNumber(string documentNumber)
    {
        var idx = _receiptListCache.FindIndex(r => r.DocumentNumber == documentNumber);
        if (idx >= 0)
        {
            _currentIndex  = idx;
            CurrentPayment = _receiptListCache[idx];
            PopulateFormFromCurrent();
        }
    }
}
