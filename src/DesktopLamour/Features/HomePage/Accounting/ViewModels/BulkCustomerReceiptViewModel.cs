// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Shared.Controls;
using DesktopLamour.Shared.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

// "Phiếu thu tiền mặt khách hàng hàng loạt" — 2026-09-26 (review kỹ ảnh mẫu MISA, xem
// Accounting/docs/phieu-thu-hang-loat.md mục "So sánh chi tiết"): viết lại thành 1 cửa sổ chứng từ
// ĐẦY ĐỦ giống ReceiptWindow/PaymentWindow (Trước/Sau/Thêm/Sửa/Xóa/Ghi sổ-Bỏ ghi/Xuất khẩu) thay vì
// popup "tạo 1 lần rồi đóng" như trước — khớp toolbar MISA (Trước·Sau·Thêm·Sửa·Sửa nhanh |
// Cất·Xóa·Hoàn·Ghi sổ | ...). "Sửa nhanh"/"Nạp"/"Tiện ích"/"Mẫu" KHÔNG có trong bản này — không
// có tính năng tương ứng ở bất kỳ đâu khác trong app. "In" có từ 2026-09-29 (ReceiptPrintWindow,
// mẫu 01-TT khớp bản in MISA).
public partial class BulkCustomerReceiptViewModel : ViewModelBase
{
    public event Action? BulkReceiptSaved;
    public event Action? RequestClose;

    private readonly IGetReceiptsUseCase               _getReceipts;
    private readonly ICreateBulkCustomerReceiptUseCase _createBulk;
    private readonly IUpdateReceiptUseCase             _updateReceipt;
    private readonly IConfirmReceiptUseCase            _confirmReceipt;
    private readonly IUnconfirmReceiptUseCase          _unconfirmReceipt;
    private readonly IDeleteReceiptUseCase             _deleteReceipt;
    private readonly IGetNextReceiptCodeUseCase        _getNextCode;
    private readonly IGetSalesOrdersByIdsUseCase       _getSalesOrdersByIds;
    private readonly IGetEmployeesUseCase              _getEmployees;
    private readonly Func<BulkCustomerReceiptSearchWindow> _searchWindowFactory;
    // Mở lại 1 hóa đơn gốc khi bấm link "Tham chiếu" (khớp ảnh mẫu MISA — Tham chiếu là link bấm
    // được). Cross-feature reference — cùng pattern InventoryDetailViewModel đã dùng (Warehouse →
    // Sales) để mở SalesOrderWindow/lấy SalesOrderResponseDto.
    private readonly IGetSalesOrderByIdUseCase _getSalesOrderById;
    private readonly Func<SalesOrderWindow>    _salesOrderWindowFactory;
    private readonly Func<ReceiptPrintWindow>  _printWindowFactory;
    private readonly ILogger<BulkCustomerReceiptViewModel> _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _isEditing;

    // ── Thông tin chung — khớp ảnh mẫu MISA ─────────────────────────────────
    [ObservableProperty] private string  _payerName = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _attachment;
    [ObservableProperty] private ISearchableItem? _selectedCollectorEmployee;
    // "Tham chiếu" — tự nối danh sách Số chứng từ (BH...) của các đơn đã chọn, chỉ để xem, không sửa
    // tay. Hiện dạng link bấm được trên UI (xem Lines binding trong XAML) — property này chỉ dùng để
    // GỬI LÊN BE (Update request), không bind trực tiếp lên UI nữa.
    [ObservableProperty] private string _reference = string.Empty;

    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "";

    [ObservableProperty] private decimal _totalAmount;
    // Tổng dưới lưới tab "2. Chứng từ" (khớp MISA): Σ Số phải thu / Σ Số chưa thu.
    [ObservableProperty] private decimal _totalGrandTotal;
    [ObservableProperty] private decimal _totalRemaining;
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";
    // 2026-09-29 (khớp MISA): "Số dòng" dưới tab "1. Hạch toán" đếm dòng ĐÃ GỘP theo khách hàng —
    // khác LineSummary (tab "2. Chứng từ", đếm từng chứng từ bán hàng).
    [ObservableProperty] private string  _groupedLineSummary = "Số dòng = 0";

    public string ReasonLabel => "Thu tiền khách hàng";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private ReceiptResponseDto? _currentReceipt;

    // "Cash111"/"Bank112" — TK Nợ áp dụng cho MỌI dòng của phiếu hiện tại. Đọc lại từ
    // entries[0].DebitAccountCode khi mở 1 phiếu đã lưu (mọi dòng luôn cùng 1 TK Nợ — chọn 1 lần ở popup
    // tìm kiếm lúc tạo). Không phải ObservableProperty vì không có control nào bind trực tiếp — chỉ
    // dùng nội bộ lúc build request lưu.
    private string  _debitAccount = "Cash111";
    private string? _bankAccount;

    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();

    // 1 dòng/1 hóa đơn — tab "2. Chứng từ" bind thẳng vào đây, "Số thu" sửa được (Mode=TwoWay).
    public ObservableCollection<BulkReceiptLineItem> Lines { get; } = new();
    // 1 dòng/1 khách hàng (gộp) — tab "1. Hạch toán" bind vào đây, chỉ để xem (khớp ảnh mẫu MISA).
    // Dựng lại TOÀN BỘ mỗi khi Lines đổi hoặc bất kỳ Line nào đổi Amount — xem RecalculateTotals.
    public ObservableCollection<BulkReceiptGroupedLine> GroupedLines { get; } = new();

    private List<ReceiptResponseDto> _bulkReceiptListCache = new();
    private int _currentIndex = -1;

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
        OnPropertyChanged(nameof(IsGridLocked));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(UnpostButtonLabel));
        EditCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        ToggleConfirmCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
    }

    public bool CanNavigatePrev => _currentIndex > 0;
    public bool CanNavigateNext => _currentIndex >= 0 && _currentIndex < _bulkReceiptListCache.Count - 1;

    // ── Trạng thái popup — giống hệt ReceiptViewModel/PaymentViewModel (khớp quy trình Chứng từ
    // bán hàng): mở phiếu có sẵn → form khóa; "Sửa" chỉ bật khi chưa ghi sổ; "Cất" = lưu (KHÔNG tự
    // ghi sổ — khác ReceiptViewModel, xem docs/phieu-thu-hang-loat.md: MISA có Cất và Ghi sổ TÁCH
    // RIÊNG cho phiếu hàng loạt); "Ghi sổ/Bỏ ghi" 1 nút toggle; "Xóa" chỉ khi chưa ghi sổ.
    public bool IsConfirmed => CurrentReceipt is not null && CurrentReceipt.Status == "Confirmed";
    public bool IsEditable  => CurrentReceipt is null || (IsEditing && !IsConfirmed);
    private bool CanEdit    => CurrentReceipt is not null && !IsEditing && !IsConfirmed;
    private bool CanToggleConfirm => CurrentReceipt is not null && !IsEditing;
    public string UnpostButtonLabel => IsConfirmed ? "Bỏ ghi" : "Ghi sổ";
    public bool CanDelete   => CurrentReceipt is not null && !IsConfirmed && !IsEditing;
    // In chỉ phiếu ĐÃ LƯU (Treo hay đã ghi sổ đều in được) và không đang sửa — in đúng dữ liệu BE
    // trả về, không in số liệu đang gõ dở chưa Cất (giống PaymentViewModel.CanPrint).
    private bool CanPrint   => CurrentReceipt is not null && !IsEditing;
    // Lưới tab "2. Chứng từ" khóa nhập liệu (IsReadOnly) thay vì IsEnabled — để link "Số chứng từ"
    // vẫn bấm được khi phiếu đang khóa (xem XAML).
    public bool IsGridLocked => !IsEditable;

    public BulkCustomerReceiptViewModel(
        IGetReceiptsUseCase               getReceipts,
        ICreateBulkCustomerReceiptUseCase createBulk,
        IUpdateReceiptUseCase             updateReceipt,
        IConfirmReceiptUseCase            confirmReceipt,
        IUnconfirmReceiptUseCase          unconfirmReceipt,
        IDeleteReceiptUseCase             deleteReceipt,
        IGetNextReceiptCodeUseCase        getNextCode,
        IGetSalesOrdersByIdsUseCase       getSalesOrdersByIds,
        IGetEmployeesUseCase              getEmployees,
        Func<BulkCustomerReceiptSearchWindow> searchWindowFactory,
        IGetSalesOrderByIdUseCase         getSalesOrderById,
        Func<SalesOrderWindow>            salesOrderWindowFactory,
        Func<ReceiptPrintWindow>          printWindowFactory,
        ILogger<BulkCustomerReceiptViewModel> logger)
    {
        _getReceipts             = getReceipts;
        _createBulk               = createBulk;
        _updateReceipt            = updateReceipt;
        _confirmReceipt           = confirmReceipt;
        _unconfirmReceipt         = unconfirmReceipt;
        _deleteReceipt            = deleteReceipt;
        _getNextCode              = getNextCode;
        _getSalesOrdersByIds      = getSalesOrdersByIds;
        _getEmployees             = getEmployees;
        _searchWindowFactory      = searchWindowFactory;
        _getSalesOrderById        = getSalesOrderById;
        _salesOrderWindowFactory  = salesOrderWindowFactory;
        _printWindowFactory       = printWindowFactory;
        _logger                   = logger;
    }

    // ── Init ──────────────────────────────────────────────────────────────

    // 2026-09-26: cửa sổ đang chứa ViewModel này (BulkCustomerReceiptWindow tự gán lúc khởi tạo) —
    // popup con (tìm chứng từ, mở hóa đơn từ link) lấy làm Owner để căn giữa ĐÚNG cửa sổ phiếu thay
    // vì cửa sổ chính (trước đây lệch/che nửa cửa sổ phiếu, sai màn hình khi kéo sang màn khác).
    // Cửa sổ chưa Show (lúc StartNewAsync mở bộ chọn lần đầu) → rơi về MainWindow.
    public Window? HostWindow { get; set; }

    private Window? PopupOwner => HostWindow is { IsVisible: true } ? HostWindow : Application.Current.MainWindow;

    public async Task LoadAsync(CancellationToken ct = default)
    {
        await LoadLookupsAsync(ct);
        await LoadBulkReceiptsAsync(ct);
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            var employees = await _getEmployees.ExecuteAsync(ct);
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload employees for BulkCustomerReceiptWindow");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct = default) => await LoadBulkReceiptsAsync(ct);

    // Chỉ phiếu thu hàng loạt (PartnerType == null) — Trước/Sau chỉ browse trong nhóm này, không lẫn
    // phiếu thu 1 khách hàng bình thường (ReceiptWindow lo phần đó).
    // showFirst = false: chỉ nạp cache cho Trước/Sau, KHÔNG đổ phiếu mới nhất lên form (dùng ở
    // StartNewAsync — form đang là phiếu mới vừa chọn, không được bị ghi đè).
    private async Task LoadBulkReceiptsAsync(CancellationToken ct, bool showFirst = true)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            var all = await _getReceipts.ExecuteAsync(ct);
            _bulkReceiptListCache = all
                .Where(r => r.PartnerType is null)
                .OrderByDescending(r => r.AccountingDate)
                .ThenByDescending(r => r.Id)
                .ToList();

            if (showFirst && _bulkReceiptListCache.Count > 0)
            {
                _currentIndex  = 0;
                CurrentReceipt = _bulkReceiptListCache[0];
                await PopulateFormFromCurrentAsync(ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load bulk customer receipts");
            HasError     = true;
            ErrorMessage = $"Không thể tải danh sách phiếu thu hàng loạt: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    // ── Commands ──────────────────────────────────────────────────────────

    // "➕ Thêm" — mở popup tìm kiếm để chọn chứng từ còn nợ, dựng phiếu MỚI (chưa lưu) từ lựa chọn.
    // Hủy popup tìm kiếm → giữ nguyên phiếu đang xem, không đổi gì (khớp hành vi "không phá dữ liệu
    // đang xem khi Thêm bị hủy" — không có confirm dialog riêng vì chưa động tới state nào cả).
    [RelayCommand]
    private async Task AddNewAsync(CancellationToken ct = default)
    {
        var picked = ShowPicker();
        if (picked is null) return;
        await BuildNewFromPickerAsync(picked, ct);
    }

    // 2026-09-26 (khớp MISA): entry point từ Quỹ "Thêm ▾ → Thu tiền khách hàng hàng loạt" — mở BỘ
    // CHỌN TRƯỚC, cửa sổ phiếu chỉ Show() khi trả về true (AccountingViewModel). Hủy bộ chọn → false,
    // không mở gì cả (trước đây cửa sổ phiếu đã mở sẵn và hiện phiếu cũ phía sau, Hủy xong vẫn kẹt
    // lại ở phiếu cũ). Danh sách phiếu cũ vẫn nạp để Trước/Sau dùng được, nhưng không đổ lên form.
    public async Task<bool> StartNewAsync(CancellationToken ct = default)
    {
        var picked = ShowPicker();
        if (picked is null) return false;

        await LoadLookupsAsync(ct);   // Employees phải có trước khi map NV thu nợ ở BuildNewFromPickerAsync
        await LoadBulkReceiptsAsync(ct, showFirst: false);
        await BuildNewFromPickerAsync(picked, ct);
        return true;
    }

    // 2026-09-26: entry point từ màn Quỹ (double-click / Sửa trên 1 dòng phiếu thu hàng loạt) — mở
    // thẳng vào phiếu đó, không qua bộ chọn. Trả false nếu phiếu không còn (vd. vừa bị xóa ở nơi khác).
    public async Task<bool> OpenExistingAsync(int receiptId, CancellationToken ct = default)
    {
        await LoadLookupsAsync(ct);   // Employees phải có trước khi PopulateForm map NV thu nợ
        await LoadBulkReceiptsAsync(ct, showFirst: false);
        if (!_bulkReceiptListCache.Any(r => r.Id == receiptId)) return false;
        await NavigateToBulkReceiptAsync(receiptId, ct);
        return true;
    }

    private BulkCustomerReceiptSearchViewModel? ShowPicker()
    {
        var searchWindow = _searchWindowFactory();
        searchWindow.Owner = PopupOwner;
        return searchWindow.ShowDialog() == true ? searchWindow.ViewModel : null;
    }

    private async Task BuildNewFromPickerAsync(BulkCustomerReceiptSearchViewModel picked, CancellationToken ct)
    {
        CurrentReceipt = null;
        _currentIndex  = -1;
        IsEditing      = true;
        ClearForm();

        _debitAccount = picked.PaymentMethod;
        _bankAccount  = picked.PaymentMethod == "Bank112" ? picked.BankAccount : null;
        SelectedCollectorEmployee = picked.SelectedEmployee is { } emp
            ? Employees.FirstOrDefault(e => e.Id == emp.Id)
            : null;
        AccountingDate = picked.CollectionDate;
        DocumentDate   = picked.CollectionDate;

        var debitDisplay = _debitAccount == "Bank112" ? "112" : "111";
        Lines.Clear();
        foreach (var s in picked.SelectedItems)
        {
            var line = new BulkReceiptLineItem(s)
            {
                DebitAccountDisplay  = debitDisplay,
                CreditAccountDisplay = "131",
            };
            AttachLineHandlers(line);
            Lines.Add(line);
        }
        RecalculateTotals();
        Reference = string.Join(", ", Lines.Select(l => l.DocumentNumber).Distinct());

        // Số chứng từ tự sinh dạng PT{5 số} — số hiện chỉ là dự đoán, số thật gán khi Cất.
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
    private void Edit() => IsEditing = true;

    // "💾 Cất" — lưu (Create nếu mới / Update nếu đang sửa phiếu có sẵn) rồi Ghi sổ ngay, giống
    // ReceiptViewModel.SaveAsync / SalesOrderViewModel.
    [RelayCommand(CanExecute = nameof(IsEditable))]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (Lines.Count == 0)
        {
            HasError     = true;
            ErrorMessage = "Phiếu thu hàng loạt phải có ít nhất 1 chứng từ.";
            return;
        }

        var invalid = Lines.FirstOrDefault(l => l.Amount <= 0 || l.Amount > l.MaxAmount);
        if (invalid is not null)
        {
            HasError     = true;
            ErrorMessage = $"Số tiền thu của chứng từ '{invalid.DocumentNumber}' phải > 0 và không vượt quá số còn nợ ({MoneyFormat.Format(invalid.MaxAmount)}).";
            return;
        }

        IsBusy = true;
        try
        {
            ReceiptResponseDto result;
            if (CurrentReceipt is null)
            {
                var request = new CreateBulkCustomerReceiptRequestDto
                {
                    AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                    DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date, DateTimeKind.Unspecified),
                    DebitAccount        = _debitAccount,
                    BankAccount         = _bankAccount,
                    CollectorEmployeeId = SelectedCollectorEmployee?.Id,
                    PayerName           = string.IsNullOrWhiteSpace(PayerName)  ? null : PayerName.Trim(),
                    Address             = string.IsNullOrWhiteSpace(Address)    ? null : Address.Trim(),
                    Attachment          = string.IsNullOrWhiteSpace(Attachment) ? null : Attachment.Trim(),
                    Lines = Lines.Select(l => new BulkReceiptLineRequestDto
                    {
                        SalesOrderId = l.SalesOrderId,
                        Amount       = l.Amount,
                    }).ToList(),
                };
                var response = await _createBulk.ExecuteAsync(request, ct);
                result = response.Receipt;
                _logger.LogInformation("Bulk customer receipt created: {DocumentNumber}", result.DocumentNumber);
            }
            else
            {
                var request = new UpdateReceiptRequestDto
                {
                    PayerName           = PayerName.Trim(),
                    Address             = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                    PaymentReason       = "ThuKhachHangHangLoat",
                    CollectorEmployeeId = SelectedCollectorEmployee?.Id,
                    Attachment          = string.IsNullOrWhiteSpace(Attachment) ? null : Attachment.Trim(),
                    Reference           = string.IsNullOrWhiteSpace(Reference) ? null : Reference.Trim(),
                    AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                    DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date, DateTimeKind.Unspecified),
                    DocumentNumber      = DocumentNumber.Trim(),
                    Entries             = Lines.Select(ToEntryDto).ToList(),
                };
                result = await _updateReceipt.ExecuteAsync(CurrentReceipt.Id, request, ct);
                _logger.LogInformation("Bulk customer receipt updated: {Id}", result.Id);
            }

            // 2026-09-28: Cất = Lưu + Ghi sổ ngay (khớp Chứng từ bán hàng / Phiếu chi) — đảo lại quyết
            // định 2026-09-26 "Cất = chỉ lưu". Ghi sổ lỗi thì phiếu vẫn đã lưu ở Treo, báo lỗi để bấm
            // "Ghi sổ" lại sau.
            string? confirmError = null;
            try
            {
                result = await _confirmReceipt.ExecuteAsync(result.Id, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Bulk customer receipt {Id} saved but confirm failed", result.Id);
                confirmError = $"Đã lưu phiếu (Treo) nhưng ghi sổ thất bại: {ex.Message}";
            }

            await LoadBulkReceiptsAsync(ct);
            await NavigateToBulkReceiptAsync(result.Id, ct);

            // Cất xong khóa form lại, GIỮ cửa sổ mở (giống ReceiptWindow).
            IsEditing = false;
            // Load*Async reset HasError — gán lỗi Ghi sổ SAU khi nạp lại để banner không bị xóa.
            if (confirmError is not null)
            {
                HasError     = true;
                ErrorMessage = confirmError;
            }
            BulkReceiptSaved?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save bulk customer receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentReceipt is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{CurrentReceipt.DocumentNumber}'?",
            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _deleteReceipt.ExecuteAsync(CurrentReceipt.Id, ct);
            _logger.LogInformation("Bulk customer receipt deleted: {Id}", CurrentReceipt.Id);
            BulkReceiptSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete bulk customer receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

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
            _logger.LogInformation("Bulk customer receipt {Id} toggled confirm — new status {Status}", result.Id, result.Status);
            await LoadBulkReceiptsAsync(ct);
            await NavigateToBulkReceiptAsync(result.Id, ct);
            BulkReceiptSaved?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle confirm for bulk customer receipt");
            MessageBox.Show(ex.Message, "Thao tác thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrev))]
    private async Task NavigatePrevAsync(CancellationToken ct = default)
    {
        if (_bulkReceiptListCache.Count == 0 || _currentIndex <= 0) return;
        _currentIndex--;
        IsEditing      = false;
        CurrentReceipt = _bulkReceiptListCache[_currentIndex];
        await PopulateFormFromCurrentAsync(ct);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private async Task NavigateNextAsync(CancellationToken ct = default)
    {
        if (_bulkReceiptListCache.Count == 0 || _currentIndex >= _bulkReceiptListCache.Count - 1) return;
        _currentIndex++;
        IsEditing      = false;
        CurrentReceipt = _bulkReceiptListCache[_currentIndex];
        await PopulateFormFromCurrentAsync(ct);
    }

    [RelayCommand]
    private async Task CancelAsync(CancellationToken ct = default)
    {
        IsEditing = false;
        HasError  = false;
        await LoadBulkReceiptsAsync(ct);
    }

    // "Tham chiếu" — bấm 1 số chứng từ (BH#####) mở lại đúng hóa đơn gốc đó, chỉ xem (khớp ảnh mẫu
    // MISA: Tham chiếu hiện dạng link). Cross-feature — xem doc comment field _getSalesOrderById.
    [RelayCommand]
    private async Task OpenSalesOrderAsync(int salesOrderId, CancellationToken ct = default)
    {
        try
        {
            var order = await _getSalesOrderById.ExecuteAsync(salesOrderId, ct);
            if (order is null)
            {
                MessageBox.Show("Không tìm thấy chứng từ bán hàng này (có thể đã bị xóa).", "Không tìm thấy",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = _salesOrderWindowFactory();
            window.Owner = PopupOwner;
            window.Initialize(order, isReadOnly: true);
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not open sales order {Id} from Tham chiếu link", salesOrderId);
            MessageBox.Show(ex.Message, "Không thể mở chứng từ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // "🖨 In" — Phiếu thu mẫu 01-TT (khớp bản in MISA), 1 tổng Số tiền cho cả phiếu.
    [RelayCommand(CanExecute = nameof(CanPrint))]
    private void Print()
    {
        if (CurrentReceipt is null) return;
        var window = _printWindowFactory();
        window.Owner = PopupOwner;
        window.Initialize(CurrentReceipt, ReasonLabel);
        window.ShowDialog();
    }

    // "📤 Xuất khẩu" — xuất tab "2. Chứng từ" (đủ chi tiết từng hóa đơn) ra Excel.
    [RelayCommand]
    private void ExportExcel()
    {
        if (Lines.Count == 0) return;
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"PhieuThuHangLoat_{DocumentNumber}_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Phiếu thu hàng loạt");

            string[] headers = { "Ngày chứng từ", "Số chứng từ", "Mã khách hàng", "Tên khách hàng", "Số phải thu", "Số chưa thu", "Số thu" };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var l in Lines)
            {
                worksheet.Cell(row, 1).Value = l.DocumentDate;
                worksheet.Cell(row, 2).Value = l.DocumentNumber;
                worksheet.Cell(row, 3).Value = l.CustomerCode;
                worksheet.Cell(row, 4).Value = l.CustomerName;
                worksheet.Cell(row, 5).Value = l.GrandTotal;
                worksheet.Cell(row, 6).Value = l.MaxAmount;
                worksheet.Cell(row, 7).Value = l.Amount;
                row++;
            }
            worksheet.Cell(row, 4).Value           = "Tổng cộng";
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 7).Value            = TotalAmount;
            worksheet.Cell(row, 7).Style.Font.Bold  = true;

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void AttachLineHandlers(BulkReceiptLineItem line) =>
        line.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BulkReceiptLineItem.Amount))
                RecalculateTotals();
        };

    private void RecalculateTotals()
    {
        TotalAmount     = Lines.Sum(l => l.Amount);
        TotalGrandTotal = Lines.Sum(l => l.GrandTotal);
        TotalRemaining  = Lines.Sum(l => l.MaxAmount);
        LineSummary = $"Số dòng = {Lines.Count}";

        GroupedLines.Clear();
        foreach (var g in BulkReceiptGroupedLine.FromLines(Lines)) GroupedLines.Add(g);
        GroupedLineSummary = $"Số dòng = {GroupedLines.Count}";
    }

    private void ClearForm()
    {
        PayerName                 = string.Empty;
        Address                   = null;
        Attachment                = null;
        SelectedCollectorEmployee = null;
        Reference                 = string.Empty;
        AccountingDate            = DateTime.Today;
        DocumentDate              = DateTime.Today;
        DocumentNumber            = "";
        Lines.Clear();
        RecalculateTotals();
    }

    // Dựng lại form từ 1 phiếu ĐÃ LƯU (CurrentReceipt) — phải gọi BE lấy lại thông tin SalesOrder
    // cho từng dòng (Chứng từ bán hàng gốc không nằm trong ReceiptEntryDto — chỉ có SalesOrderId).
    private async Task PopulateFormFromCurrentAsync(CancellationToken ct)
    {
        if (CurrentReceipt is null) return;

        PayerName                 = CurrentReceipt.PayerName;
        Address                   = CurrentReceipt.Address;
        SelectedCollectorEmployee = Employees.FirstOrDefault(e => e.Id == CurrentReceipt.CollectorEmployeeId);
        Attachment                = CurrentReceipt.Attachment;
        Reference                 = CurrentReceipt.Reference ?? "";
        AccountingDate            = CurrentReceipt.AccountingDate.ToLocalTime();
        DocumentDate              = CurrentReceipt.DocumentDate.ToLocalTime();
        DocumentNumber            = CurrentReceipt.DocumentNumber;

        Lines.Clear();
        var entries = CurrentReceipt.Entries.Where(e => e.SalesOrderId.HasValue).ToList();
        if (entries.Count == 0)
        {
            RecalculateTotals();
            return;
        }

        // BE trả mã TK trong danh mục (1111 / 1121) — đổi về lựa chọn "Tiền mặt / Tiền gửi" của màn này.
        _debitAccount = entries[0].DebitAccountCode?.StartsWith("112") == true ? "Bank112" : "Cash111";
        _bankAccount  = entries[0].BankAccount;
        var debitDisplay = _debitAccount == "Bank112" ? "112" : "111";

        try
        {
            var orders = await _getSalesOrdersByIds.ExecuteAsync(
                entries.Select(e => e.SalesOrderId!.Value).Distinct(), ct);
            var ordersById = orders.ToDictionary(o => o.SalesOrderId);

            foreach (var e in entries)
            {
                if (!ordersById.TryGetValue(e.SalesOrderId!.Value, out var order))
                {
                    _logger.LogWarning("Sales order {Id} referenced by bulk receipt {ReceiptId} not found — skipping line",
                        e.SalesOrderId, CurrentReceipt.Id);
                    continue;
                }

                var line = new BulkReceiptLineItem(order, e.Amount)
                {
                    DebitAccountDisplay  = debitDisplay,
                    CreditAccountDisplay = "131",
                };
                AttachLineHandlers(line);
                Lines.Add(line);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload sales order details for bulk receipt {Id}", CurrentReceipt.Id);
            HasError     = true;
            ErrorMessage = $"Không thể tải lại chi tiết chứng từ: {ex.Message}";
        }

        RecalculateTotals();
    }

    // Instance method (không static) — cần đọc _debitAccount/_bankAccount của phiếu hiện tại (áp
    // dụng chung cho mọi dòng, không lưu riêng trên từng BulkReceiptLineItem).
    private ReceiptEntryDto ToEntryDto(BulkReceiptLineItem line) => new()
    {
        Description   = $"Thu tiền khách hàng - {line.DocumentNumber}",
        DebitAccount  = _debitAccount,
        CreditAccount = "Receivable131",
        Amount        = line.Amount,
        SubjectCode   = line.CustomerCode,
        SubjectName   = line.CustomerName,
        BankAccount   = _bankAccount,
        SalesOrderId  = line.SalesOrderId,
    };

    private async Task NavigateToBulkReceiptAsync(int id, CancellationToken ct)
    {
        var idx = _bulkReceiptListCache.FindIndex(r => r.Id == id);
        if (idx < 0) return;
        _currentIndex  = idx;
        CurrentReceipt = _bulkReceiptListCache[idx];
        await PopulateFormFromCurrentAsync(ct);
    }
}
