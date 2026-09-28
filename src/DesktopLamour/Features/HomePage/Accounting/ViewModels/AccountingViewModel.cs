// Copyright © 2026 DesktopLamour. All rights reserved.
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Views;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class AccountingViewModel : ViewModelBase
{
    private readonly INavigationService   _navigationService;
    private readonly IGetCashLedgerUseCase _getCashLedger;
    private readonly Func<ReceiptWindow>   _receiptWindowFactory;
    private readonly Func<PaymentWindow>   _paymentWindowFactory;
    private readonly Func<BulkCustomerReceiptWindow> _bulkReceiptWindowFactory;
    private readonly IConfirmReceiptUseCase   _confirmReceipt;
    private readonly IUnconfirmReceiptUseCase _unconfirmReceipt;
    private readonly IDeleteReceiptUseCase    _deleteReceipt;
    private readonly IConfirmPaymentUseCase   _confirmPayment;
    private readonly IUnconfirmPaymentUseCase _unconfirmPayment;
    private readonly IDeletePaymentUseCase    _deletePayment;

    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage  = string.Empty;
    [ObservableProperty] private bool    _hasItems;
    [ObservableProperty] private decimal _openingBalance;
    [ObservableProperty] private decimal _closingBalance;
    // 2026-09-28 (khớp box "Tồn quỹ đến hiện tại" ảnh mẫu MISA): tồn quỹ tính tới hôm nay, độc lập
    // bộ lọc Từ ngày/Đến ngày đang chọn — BE trả sẵn trong CashLedgerResponseDto.current_balance.
    [ObservableProperty] private decimal _currentBalance;
    // 2026-09-28 (khớp dòng tổng cuối lưới ảnh mẫu MISA "Số dòng = N · Tổng thu: X · Tổng chi: X"):
    // tính trên các dòng ĐANG HIỂN THỊ sau lọc (ItemsView), không phải toàn bộ Items — giống cách
    // "Xuất khẩu" chỉ xuất dòng đang hiển thị.
    [ObservableProperty] private int     _visibleLineCount;
    [ObservableProperty] private decimal _totalThu;
    [ObservableProperty] private decimal _totalChi;
    // 2026-09-16: đổi lại mặc định "Hôm nay" (đảo ngược quyết định 2026-08-31 "Đầu tháng đến hiện
    // tại") — khớp SelectedPeriod bên dưới.
    [ObservableProperty] private DateTime _fromDate = DateTime.Today;
    [ObservableProperty] private DateTime _toDate   = DateTime.Today;

    // "Kỳ" — chọn nhanh khoảng ngày (theo mẫu MISA), chọn "Tùy chọn" thì để Từ ngày/Đến ngày tự
    // do chỉnh tay. Đổi Period sẽ ghi đè FromDate/ToDate, KHÔNG tự gọi LoadAsync — người dùng vẫn
    // bấm "Lấy dữ liệu" để áp dụng (khớp hành vi màn MISA).
    public static string[] PeriodOptions { get; } =
        { "Tùy chọn", "Hôm nay", "Hôm qua", "Tuần này", "Tháng này", "Tháng trước", "Quý này", "Năm nay", "Đầu tháng đến hiện tại" };

    [ObservableProperty] private string _selectedPeriod = "Hôm nay";

    // Lọc Trạng thái/Loại áp trực tiếp lên dữ liệu đã tải (không gọi lại BE) — ItemsView là nguồn
    // DataGrid bind vào; Items vẫn là dữ liệu gốc từ LoadAsync.
    // 2026-09-26: bỏ "Nháp" — khớp quy trình Chứng từ bán hàng (ChungTuTraHangBan-Review.html): chỉ còn
    // Treo (chưa ghi sổ) và Đã ghi sổ. BE trả "Treo" cho mọi dòng chưa ghi sổ.
    public static string[] StatusOptions { get; } = { "Tất cả", "Treo", "Đã ghi sổ" };
    public static string[] TypeOptions   { get; } = { "Tất cả", "Thu", "Chi" };

    [ObservableProperty] private string _filterStatus = "Tất cả";
    [ObservableProperty] private string _filterType    = "Tất cả";

    // ── Per-column filter row, embedded directly in each DataGrid column header (no popup) ────
    // Same visual pattern as SalesOrderReportDetailView: text columns get a plain Contains
    // textbox, date/numeric columns get an operator combo (=, ≤, ...) + typed value. These apply
    // on top of the existing FilterStatus/FilterType toolbar filters via the same ItemsView
    // (CollectionView) filter predicate — FilterEntry — so ItemsView.Refresh() plays the role
    // ApplyFilters() plays on the Sales screen (Items itself is not re-populated here).
    [ObservableProperty] private string _filterReceiptNumber  = string.Empty;
    [ObservableProperty] private string _filterPaymentNumber  = string.Empty;
    [ObservableProperty] private string _filterDescription    = string.Empty;
    [ObservableProperty] private string _filterPersonName     = string.Empty;
    [ObservableProperty] private string _filterPaymentReason  = string.Empty;
    [ObservableProperty] private string _filterDocumentType   = string.Empty;

    partial void OnFilterReceiptNumberChanged(string value)  => ItemsView.Refresh();
    partial void OnFilterPaymentNumberChanged(string value)  => ItemsView.Refresh();
    partial void OnFilterDescriptionChanged(string value)    => ItemsView.Refresh();
    partial void OnFilterPersonNameChanged(string value)     => ItemsView.Refresh();
    partial void OnFilterPaymentReasonChanged(string value)  => ItemsView.Refresh();
    partial void OnFilterDocumentTypeChanged(string value)   => ItemsView.Refresh();

    public DateColumnFilter AccountingDateFilter { get; } = new();
    public DateColumnFilter DocumentDateFilter   { get; } = new();

    public NumericColumnFilter AmountFilter { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = ItemsView.Refresh;
        DocumentDateFilter.Changed   = ItemsView.Refresh;
        AmountFilter.Changed         = ItemsView.Refresh;
    }

    public ObservableCollection<CashLedgerEntryDto> Items { get; } = new();

    public ICollectionView ItemsView { get; }

    [ObservableProperty] private CashLedgerEntryDto? _selectedEntry;

    private bool HasSelectedEntry => SelectedEntry is not null;

    // 2026-09-26: thanh công cụ giống Chứng từ bán hàng — quy tắc bật/tắt nút theo TRẠNG THÁI phiếu
    // (khớp quy trình ChungTuTraHangBan-Review.html + quy tắc phiếu thu/chi hiện có):
    //   Ghi sổ  — phiếu chưa ghi sổ (Treo)
    //   Bỏ ghi  — phiếu đã ghi sổ
    //   Sửa/Xóa — chỉ khi chưa ghi sổ (đã ghi sổ phải Bỏ ghi trước)
    // Dòng không tìm được phiếu gốc (không có receipt_id/payment_id) thì không thao tác được.
    private bool SelectedHasSource  => SelectedEntry is { ReceiptId: not null } or { PaymentId: not null };
    private bool SelectedIsPosted   => SelectedEntry?.Status == "Confirmed";
    private bool CanConfirmSelected   => SelectedHasSource && !SelectedIsPosted;
    private bool CanUnconfirmSelected => SelectedHasSource && SelectedIsPosted;
    private bool CanModifySelected    => SelectedHasSource && !SelectedIsPosted;

    public AccountingViewModel(
        INavigationService   navigationService,
        IGetCashLedgerUseCase getCashLedger,
        Func<ReceiptWindow>   receiptWindowFactory,
        Func<PaymentWindow>   paymentWindowFactory,
        Func<BulkCustomerReceiptWindow> bulkReceiptWindowFactory,
        IConfirmReceiptUseCase   confirmReceipt,
        IUnconfirmReceiptUseCase unconfirmReceipt,
        IDeleteReceiptUseCase    deleteReceipt,
        IConfirmPaymentUseCase   confirmPayment,
        IUnconfirmPaymentUseCase unconfirmPayment,
        IDeletePaymentUseCase    deletePayment)
    {
        _navigationService    = navigationService;
        _getCashLedger        = getCashLedger;
        _receiptWindowFactory = receiptWindowFactory;
        _paymentWindowFactory = paymentWindowFactory;
        _bulkReceiptWindowFactory = bulkReceiptWindowFactory;
        _confirmReceipt   = confirmReceipt;
        _unconfirmReceipt = unconfirmReceipt;
        _deleteReceipt    = deleteReceipt;
        _confirmPayment   = confirmPayment;
        _unconfirmPayment = unconfirmPayment;
        _deletePayment    = deletePayment;

        ItemsView = CollectionViewSource.GetDefaultView(Items);
        ItemsView.Filter = FilterEntry;
        // Refresh() (đổi bộ lọc) và Items.Clear()/Add() (LoadAsync) đều bắn CollectionChanged trên
        // ItemsView — 1 chỗ duy nhất tính lại dòng tổng cuối lưới, không cần gọi tay ở nhiều nơi.
        ItemsView.CollectionChanged += (_, _) => RecalculateFooterTotals();

        WireColumnFilters();
    }

    private void RecalculateFooterTotals()
    {
        var visible = ItemsView.Cast<CashLedgerEntryDto>().ToList();
        VisibleLineCount = visible.Count;
        TotalThu         = visible.Sum(e => e.DebitAmount);
        TotalChi         = visible.Sum(e => e.CreditAmount);
    }

    partial void OnFilterStatusChanged(string value) => ItemsView.Refresh();
    partial void OnFilterTypeChanged(string value)   => ItemsView.Refresh();

    partial void OnSelectedPeriodChanged(string value)
    {
        var today = DateTime.Today;
        switch (value)
        {
            case "Hôm nay":
                FromDate = today;
                ToDate   = today;
                break;
            case "Hôm qua":
                FromDate = today.AddDays(-1);
                ToDate   = today.AddDays(-1);
                break;
            case "Tuần này":
                FromDate = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                ToDate   = today;
                break;
            case "Tháng này":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate   = FromDate.AddMonths(1).AddDays(-1);
                break;
            case "Tháng trước":
                FromDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                ToDate   = FromDate.AddMonths(1).AddDays(-1);
                break;
            case "Quý này":
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                FromDate = new DateTime(today.Year, quarterStartMonth, 1);
                ToDate   = FromDate.AddMonths(3).AddDays(-1);
                break;
            case "Năm nay":
                FromDate = new DateTime(today.Year, 1, 1);
                ToDate   = new DateTime(today.Year, 12, 31);
                break;
            case "Đầu tháng đến hiện tại":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate   = today;
                break;
            case "Tùy chọn":
            default:
                break;
        }
    }

    private bool FilterEntry(object obj)
    {
        if (obj is not CashLedgerEntryDto entry) return false;

        var statusLabel = entry.Status switch
        {
            "Draft"     => "Treo", // dữ liệu cũ — coi như Treo, không còn trạng thái "Nháp"
            "Treo"      => "Treo",
            "Confirmed" => "Đã ghi sổ",
            _           => entry.Status,
        };

        if (FilterStatus != "Tất cả" && statusLabel != FilterStatus) return false;

        if (FilterType == "Thu" && string.IsNullOrEmpty(entry.ReceiptNumber)) return false;
        if (FilterType == "Chi" && string.IsNullOrEmpty(entry.PaymentNumber)) return false;

        var paymentReasonLabel = entry.PaymentReason switch
        {
            "ThuKhac"     => "Thu khác",
            "ThuTienHang" => "Thu tiền hàng",
            "ThuCongNo"   => "Thu công nợ",
            "ThuKhachHangHangLoat" => "Phiếu thu tiền mặt khách hàng hàng loạt",
            "ChiKhac"     => "Chi khác",
            "ChiMuaHang"  => "Chi mua hàng",
            "ChiTraNo"    => "Chi trả nợ",
            "ChiLuong"    => "Chi lương",
            _             => entry.PaymentReason ?? "",
        };

        return AccountingDateFilter.Matches(entry.AccountingDate)
            && DocumentDateFilter.Matches(entry.DocumentDate)
            && Matches(FilterReceiptNumber, entry.ReceiptNumber ?? "")
            && Matches(FilterPaymentNumber, entry.PaymentNumber ?? "")
            && Matches(FilterDescription, entry.Description)
            && AmountFilter.Matches(entry.Amount)
            && Matches(FilterPersonName, entry.PersonName ?? "")
            && Matches(FilterPaymentReason, paymentReasonLabel)
            && Matches(FilterDocumentType, entry.DocumentType);
    }

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void OpenReceipt()
    {
        var window = _receiptWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.ViewModel.ReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
        window.Show();
    }

    [RelayCommand]
    private void OpenPayment()
    {
        var window = _paymentWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.ViewModel.PaymentSaved += () => _ = LoadAsync(CancellationToken.None);
        window.Show();
    }

    // 2026-09-26: BulkCustomerReceiptWindow là cửa sổ chứng từ đầy đủ (Trước/Sau/Sửa/Xóa/Ghi sổ),
    // mở KHÔNG modal (Show) — mirror OpenReceipt/OpenPayment. Khớp MISA: bộ chọn chứng từ hiện
    // TRƯỚC (StartNewAsync), cửa sổ phiếu chỉ Show() khi đã bấm "✔ Thu tiền"; Hủy → không mở gì.
    [RelayCommand]
    private async Task OpenBulkCustomerReceiptAsync(CancellationToken ct = default)
    {
        var window = _bulkReceiptWindowFactory();
        window.Owner = Application.Current.MainWindow;
        if (!await window.ViewModel.StartNewAsync(ct))
        {
            // Phải Close() dù chưa Show — ShutdownMode mặc định OnLastWindowClose: cửa sổ đã tạo mà
            // không đóng sẽ nằm mãi trong Application.Windows, tắt màn chính xong app vẫn chạy ngầm.
            window.Close();
            return;
        }

        window.ViewModel.BulkReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
        window.Show();
    }

    // "Xem" 1 dòng đã chọn (double-click hoặc nút toolbar) — mở đúng ReceiptWindow/PaymentWindow
    // (tuỳ ReceiptNumber hay PaymentNumber có giá trị) THẲNG vào bản ghi đó thay vì form Thêm mới
    // trống, tái dùng toàn bộ Sửa/Lưu/Xóa/Ghi số/Hoàn đã có sẵn trong 2 window này — không cần
    // xây UI Sửa/Xóa riêng trên màn Sổ Kế Toán. Rule "phiếu đã Ghi số bất biến" đã tự áp dụng bên
    // trong PaymentViewModel (CanEdit/thông báo "đã ghi số, không thể sửa" khi bấm Sửa/Xóa).
    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private async Task ViewEntryAsync(CancellationToken ct = default)
    {
        if (SelectedEntry is null) return;

        // 2026-09-26: phiếu thu HÀNG LOẠT phải mở đúng BulkCustomerReceiptWindow — trước đây rơi vào
        // nhánh ReceiptWindow bên dưới (chỉ phân biệt thu/chi), Cất ở đó báo "Vui lòng chọn đối tượng".
        if (SelectedEntry is { IsBulkReceipt: true, ReceiptId: int bulkId })
        {
            var bulkWindow = _bulkReceiptWindowFactory();
            bulkWindow.Owner = Application.Current.MainWindow;
            if (!await bulkWindow.ViewModel.OpenExistingAsync(bulkId, ct))
            {
                bulkWindow.Close(); // chưa Show nhưng vẫn phải Close — xem OpenBulkCustomerReceiptAsync
                MessageBox.Show("Không tìm thấy phiếu thu hàng loạt này (có thể đã bị xóa).", "Không tìm thấy",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadAsync(ct);
                return;
            }
            bulkWindow.ViewModel.BulkReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
            bulkWindow.Show();
            return;
        }

        if (!string.IsNullOrEmpty(SelectedEntry.ReceiptNumber))
        {
            var window = _receiptWindowFactory();
            window.InitialDocumentNumber = SelectedEntry.ReceiptNumber;
            window.Owner = Application.Current.MainWindow;
            window.ViewModel.ReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
            window.Show();
        }
        else if (!string.IsNullOrEmpty(SelectedEntry.PaymentNumber))
        {
            var window = _paymentWindowFactory();
            window.InitialDocumentNumber = SelectedEntry.PaymentNumber;
            window.Owner = Application.Current.MainWindow;
            window.ViewModel.PaymentSaved += () => _ = LoadAsync(CancellationToken.None);
            window.Show();
        }
    }

    partial void OnSelectedEntryChanged(CashLedgerEntryDto? value)
    {
        ViewEntryCommand.NotifyCanExecuteChanged();
        EditEntryCommand.NotifyCanExecuteChanged();
        ConfirmEntryCommand.NotifyCanExecuteChanged();
        UnconfirmEntryCommand.NotifyCanExecuteChanged();
        DeleteEntryCommand.NotifyCanExecuteChanged();
        SendEmailCommand.NotifyCanExecuteChanged();
        SendZaloCommand.NotifyCanExecuteChanged();
    }

    // "✏️ Sửa" — mở đúng phiếu gốc (giống double-click/Xem) nhưng chỉ bấm được khi phiếu chưa ghi sổ.
    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private Task EditEntryAsync(CancellationToken ct = default) => ViewEntryAsync(ct);

    // "📗 Ghi sổ" thẳng từ danh sách, không hỏi xác nhận (khớp Chứng từ bán hàng).
    [RelayCommand(CanExecute = nameof(CanConfirmSelected))]
    private async Task ConfirmEntryAsync(CancellationToken ct = default)
    {
        if (SelectedEntry is not { } entry) return;
        try
        {
            if (entry.ReceiptId is int receiptId)
                await _confirmReceipt.ExecuteAsync(receiptId, ct);
            else if (entry.PaymentId is int paymentId)
                await _confirmPayment.ExecuteAsync(paymentId, ct);
            await LoadAsync(ct);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ghi sổ thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // "↩️ Bỏ ghi" — không hỏi xác nhận (khớp Chứng từ bán hàng). Phiếu về Treo (chưa ghi sổ).
    [RelayCommand(CanExecute = nameof(CanUnconfirmSelected))]
    private async Task UnconfirmEntryAsync(CancellationToken ct = default)
    {
        if (SelectedEntry is not { } entry) return;
        try
        {
            if (entry.ReceiptId is int receiptId)
                await _unconfirmReceipt.ExecuteAsync(receiptId, ct);
            else if (entry.PaymentId is int paymentId)
                await _unconfirmPayment.ExecuteAsync(paymentId, ct);
            await LoadAsync(ct);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Bỏ ghi thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // "🗑️ Xóa" — chỉ khi chưa ghi sổ, có hỏi Yes/No (khớp Chứng từ bán hàng).
    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteEntryAsync(CancellationToken ct = default)
    {
        if (SelectedEntry is not { } entry) return;
        var number = entry.ReceiptNumber ?? entry.PaymentNumber;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{number}'?",
            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            if (entry.ReceiptId is int receiptId)
                await _deleteReceipt.ExecuteAsync(receiptId, ct);
            else if (entry.PaymentId is int paymentId)
                await _deletePayment.ExecuteAsync(paymentId, ct);
            SelectedEntry = null;
            await LoadAsync(ct);
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
    }

    // "Gửi email / Gửi Zalo" phiếu đang chọn — giống Chứng từ bán hàng: app chưa tích hợp SMTP/Zalo OA,
    // nên xuất file Excel của phiếu rồi mở email/Zalo của máy để người dùng tự đính kèm.
    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private void SendEmail()
    {
        if (SelectedEntry is not { } entry) return;
        try
        {
            var number = entry.ReceiptNumber ?? entry.PaymentNumber ?? "";
            using var workbook = BuildWorkbook(new[] { entry });
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, $"ChungTu_{number}");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenMailClient(
                $"{entry.DocumentType} - {number}",
                $"File chứng từ đã được lưu tại:\n{path}\n\nVui lòng đính kèm file này vào email trước khi gửi.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Email thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelectedEntry))]
    private void SendZalo()
    {
        if (SelectedEntry is not { } entry) return;
        try
        {
            var number = entry.ReceiptNumber ?? entry.PaymentNumber ?? "";
            using var workbook = BuildWorkbook(new[] { entry });
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, $"ChungTu_{number}");
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenZaloApp();

            MessageBox.Show("Đã mở Zalo và thư mục chứa file chứng từ. Vui lòng kéo-thả file để đính kèm.",
                "Gửi Zalo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Zalo thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var from   = DateOnly.FromDateTime(FromDate);
            var to     = DateOnly.FromDateTime(ToDate);
            var result = await _getCashLedger.ExecuteAsync(from, to, ct);

            Items.Clear();
            foreach (var entry in result.Entries) Items.Add(entry);
            OpeningBalance = result.OpeningBalance;
            ClosingBalance = result.ClosingBalance;
            CurrentBalance = result.CurrentBalance;
            HasItems       = Items.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // Xuất đúng những dòng đang hiển thị trên lưới (đã áp Trạng thái/Loại), không phải toàn bộ Items.
    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"SoQuyTienMat_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = BuildWorkbook(ItemsView.Cast<CashLedgerEntryDto>());
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Dùng chung cho Xuất khẩu (mọi dòng đang hiển thị) và Gửi email/Zalo (1 phiếu đang chọn).
    private static XLWorkbook BuildWorkbook(IEnumerable<CashLedgerEntryDto> entries)
    {
        var workbook  = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sổ quỹ tiền mặt");

        string[] headers =
        {
            "Ngày hạch toán", "Ngày chứng từ", "Số phiếu thu", "Số phiếu chi",
            "Diễn giải", "Số tiền", "Người nhận/Người nộp", "Lý do thu/chi", "Loại chứng từ",
        };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value           = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var e in entries)
        {
            worksheet.Cell(row, 1).Value = e.AccountingDate;
            worksheet.Cell(row, 2).Value = e.DocumentDate;
            worksheet.Cell(row, 3).Value = e.ReceiptNumber;
            worksheet.Cell(row, 4).Value = e.PaymentNumber;
            worksheet.Cell(row, 5).Value = e.Description;
            worksheet.Cell(row, 6).Value = e.Amount;
            worksheet.Cell(row, 7).Value = e.PersonName;
            worksheet.Cell(row, 8).Value = e.PaymentReason switch
            {
                "ThuKhac"     => "Thu khác",
                "ThuTienHang" => "Thu tiền hàng",
                "ThuCongNo"   => "Thu công nợ",
                "ThuKhachHangHangLoat" => "Phiếu thu tiền mặt khách hàng hàng loạt",
                "ChiKhac"     => "Chi khác",
                "ChiMuaHang"  => "Chi mua hàng",
                "ChiTraNo"    => "Chi trả nợ",
                "ChiLuong"    => "Chi lương",
                _             => e.PaymentReason,
            };
            worksheet.Cell(row, 9).Value = e.DocumentType;
            row++;
        }
        worksheet.Columns().AdjustToContents();
        return workbook;
    }
}
