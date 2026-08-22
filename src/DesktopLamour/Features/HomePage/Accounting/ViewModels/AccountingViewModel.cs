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
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class AccountingViewModel : ViewModelBase
{
    private readonly INavigationService   _navigationService;
    private readonly IGetCashLedgerUseCase _getCashLedger;
    private readonly Func<ReceiptWindow>   _receiptWindowFactory;
    private readonly Func<PaymentWindow>   _paymentWindowFactory;
    private readonly Func<BulkCustomerReceiptSearchWindow> _bulkReceiptSearchWindowFactory;

    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage  = string.Empty;
    [ObservableProperty] private bool    _hasItems;
    [ObservableProperty] private decimal _openingBalance;
    [ObservableProperty] private decimal _closingBalance;
    [ObservableProperty] private DateTime _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime _toDate   = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

    // "Kỳ" — chọn nhanh khoảng ngày (theo mẫu MISA), chọn "Tùy chọn" thì để Từ ngày/Đến ngày tự
    // do chỉnh tay. Đổi Period sẽ ghi đè FromDate/ToDate, KHÔNG tự gọi LoadAsync — người dùng vẫn
    // bấm "Lấy dữ liệu" để áp dụng (khớp hành vi màn MISA).
    public static string[] PeriodOptions { get; } =
        { "Tùy chọn", "Hôm nay", "Hôm qua", "Tuần này", "Tháng này", "Tháng trước", "Quý này", "Năm nay" };

    [ObservableProperty] private string _selectedPeriod = "Tháng này";

    // Lọc Trạng thái/Loại áp trực tiếp lên dữ liệu đã tải (không gọi lại BE) — ItemsView là nguồn
    // DataGrid bind vào; Items vẫn là dữ liệu gốc từ LoadAsync.
    public static string[] StatusOptions { get; } = { "Tất cả", "Nháp", "Treo", "Đã ghi sổ" };
    public static string[] TypeOptions   { get; } = { "Tất cả", "Thu", "Chi" };

    [ObservableProperty] private string _filterStatus = "Tất cả";
    [ObservableProperty] private string _filterType    = "Tất cả";

    public ObservableCollection<CashLedgerEntryDto> Items { get; } = new();

    public ICollectionView ItemsView { get; }

    public AccountingViewModel(
        INavigationService   navigationService,
        IGetCashLedgerUseCase getCashLedger,
        Func<ReceiptWindow>   receiptWindowFactory,
        Func<PaymentWindow>   paymentWindowFactory,
        Func<BulkCustomerReceiptSearchWindow> bulkReceiptSearchWindowFactory)
    {
        _navigationService    = navigationService;
        _getCashLedger        = getCashLedger;
        _receiptWindowFactory = receiptWindowFactory;
        _paymentWindowFactory = paymentWindowFactory;
        _bulkReceiptSearchWindowFactory = bulkReceiptSearchWindowFactory;

        ItemsView = CollectionViewSource.GetDefaultView(Items);
        ItemsView.Filter = FilterEntry;
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
            case "Tùy chọn":
            default:
                break;
        }
    }

    private bool FilterEntry(object obj)
    {
        if (obj is not CashLedgerEntryDto entry) return false;

        if (FilterStatus != "Tất cả")
        {
            var statusLabel = entry.Status switch
            {
                "Draft"     => "Nháp",
                "Treo"      => "Treo",
                "Confirmed" => "Đã ghi sổ",
                _           => entry.Status,
            };
            if (statusLabel != FilterStatus) return false;
        }

        if (FilterType == "Thu" && string.IsNullOrEmpty(entry.ReceiptNumber)) return false;
        if (FilterType == "Chi" && string.IsNullOrEmpty(entry.PaymentNumber)) return false;

        return true;
    }

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

    [RelayCommand]
    private void OpenBulkCustomerReceipt()
    {
        var window = _bulkReceiptSearchWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.ShowDialog();
        _ = LoadAsync(CancellationToken.None);
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

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sổ quỹ tiền mặt");

            string[] headers =
            {
                "Ngày hạch toán", "Ngày chứng từ", "Số phiếu thu", "Số phiếu chi", "Trạng thái",
                "Diễn giải", "Tài khoản", "TK đối ứng", "Nợ", "Có", "Số tồn", "Người nhận/Người nộp",
            };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var e in ItemsView.Cast<CashLedgerEntryDto>())
            {
                worksheet.Cell(row, 1).Value  = e.AccountingDate;
                worksheet.Cell(row, 2).Value  = e.DocumentDate;
                worksheet.Cell(row, 3).Value  = e.ReceiptNumber;
                worksheet.Cell(row, 4).Value  = e.PaymentNumber;
                worksheet.Cell(row, 5).Value  = e.Status switch
                {
                    "Draft"     => "Nháp",
                    "Treo"      => "Treo",
                    "Confirmed" => "Đã ghi sổ",
                    _           => e.Status,
                };
                worksheet.Cell(row, 6).Value  = e.Description;
                worksheet.Cell(row, 7).Value  = e.Account;
                worksheet.Cell(row, 8).Value  = e.CounterAccount;
                worksheet.Cell(row, 9).Value  = e.DebitAmount;
                worksheet.Cell(row, 10).Value = e.CreditAmount;
                worksheet.Cell(row, 11).Value = e.Balance;
                worksheet.Cell(row, 12).Value = e.PersonName;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
