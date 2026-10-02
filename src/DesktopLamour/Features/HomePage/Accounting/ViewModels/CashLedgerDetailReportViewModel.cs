// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Views;
using DesktopLamour.Shared.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

// Trang báo cáo "Sổ kế toán chi tiết quỹ tiền mặt" (khớp mẫu MISA): dòng Số tồn đầu kỳ + mỗi dòng hạch
// toán 1 dòng, Số tồn cộng dồn; bấm số chứng từ mở phiếu gốc. Mở từ màn Quỹ → Báo cáo (nhận
// CashLedgerReportFilter qua OnNavigatedTo), cùng khung với SalesOrderReportViewModel.
public partial class CashLedgerDetailReportViewModel : ViewModelBase, INavigationParameterAware
{
    private const string FilePrefix = "SoQuyTienMat";

    private readonly IGetCashLedgerDetailReportUseCase  _getReport;
    private readonly INavigationService                 _navigationService;
    private readonly Func<CashLedgerReportFilterWindow> _filterWindowFactory;
    private readonly Func<ReceiptWindow>                _receiptWindowFactory;
    private readonly Func<PaymentWindow>                _paymentWindowFactory;
    private readonly Func<BulkCustomerReceiptWindow>    _bulkReceiptWindowFactory;
    private readonly ILogger<CashLedgerDetailReportViewModel> _logger;

    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private CashLedgerReportFilter? _currentFilter;
    [ObservableProperty] private int     _rowCount;
    [ObservableProperty] private decimal _totalDebit;
    [ObservableProperty] private decimal _totalCredit;
    [ObservableProperty] private decimal _closingBalance;

    public string ReportTitle => "SỔ KẾ TOÁN CHI TIẾT QUỸ TIỀN MẶT";
    public string Subtitle    => CurrentFilter?.Subtitle ?? string.Empty;

    public ObservableCollection<CashLedgerReportRow> Rows { get; } = new();

    public CashLedgerDetailReportViewModel(
        IGetCashLedgerDetailReportUseCase  getReport,
        INavigationService                 navigationService,
        Func<CashLedgerReportFilterWindow> filterWindowFactory,
        Func<ReceiptWindow>                receiptWindowFactory,
        Func<PaymentWindow>                paymentWindowFactory,
        Func<BulkCustomerReceiptWindow>    bulkReceiptWindowFactory,
        ILogger<CashLedgerDetailReportViewModel> logger)
    {
        _getReport                = getReport;
        _navigationService        = navigationService;
        _filterWindowFactory      = filterWindowFactory;
        _receiptWindowFactory     = receiptWindowFactory;
        _paymentWindowFactory     = paymentWindowFactory;
        _bulkReceiptWindowFactory = bulkReceiptWindowFactory;
        _logger                   = logger;
    }

    partial void OnCurrentFilterChanged(CashLedgerReportFilter? value) => OnPropertyChanged(nameof(Subtitle));

    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is not CashLedgerReportFilter filter) return;
        CurrentFilter = filter;
        _ = LoadAsync();
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        if (CurrentFilter is not { } filter) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var report = await _getReport.ExecuteAsync(filter, ct);

            Rows.Clear();
            Rows.Add(CashLedgerReportRow.Opening(report.OpeningBalance));
            foreach (var row in report.Rows) Rows.Add(CashLedgerReportRow.From(row));

            RowCount       = report.Rows.Count;
            TotalDebit     = report.TotalDebit;
            TotalCredit    = report.TotalCredit;
            ClosingBalance = report.ClosingBalance;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cash ledger detail report");
            HasError     = true;
            ErrorMessage = $"Không thể tải báo cáo: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // "🔄 Nạp" — tải lại với tham số đang xem.
    [RelayCommand]
    private Task ReloadAsync(CancellationToken ct = default) => LoadAsync(ct);

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void ChooseParameters()
    {
        var window = _filterWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.Initialize(CurrentFilter);
        if (window.ShowDialog() != true) return;

        CurrentFilter = window.BuildFilter();
        _ = LoadAsync();
    }

    // Bấm số phiếu thu → mở phiếu gốc (cùng cách màn Quỹ mở khi double-click): phiếu hàng loạt mở
    // BulkCustomerReceiptWindow, còn lại ReceiptWindow. Sửa/Ghi sổ/Bỏ ghi trong phiếu xong thì nạp lại báo cáo.
    [RelayCommand]
    private async Task OpenReceiptAsync(CashLedgerReportRow? row, CancellationToken ct = default)
    {
        if (row?.ReceiptId is not int receiptId) return;

        if (row.IsBulkReceipt)
        {
            var bulkWindow = _bulkReceiptWindowFactory();
            bulkWindow.Owner = Application.Current.MainWindow;
            if (!await bulkWindow.ViewModel.OpenExistingAsync(receiptId, ct))
            {
                bulkWindow.Close(); // chưa Show nhưng vẫn phải Close — xem AccountingViewModel.OpenBulkCustomerReceiptAsync
                MessageBox.Show("Không tìm thấy phiếu thu hàng loạt này (có thể đã bị xóa).", "Không tìm thấy",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadAsync(ct);
                return;
            }
            bulkWindow.ViewModel.BulkReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
            bulkWindow.Show();
            return;
        }

        var window = _receiptWindowFactory();
        window.InitialDocumentNumber = row.ReceiptNumber;
        window.Owner = Application.Current.MainWindow;
        window.ViewModel.ReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
        window.Show();
    }

    [RelayCommand]
    private void OpenPayment(CashLedgerReportRow? row)
    {
        if (row?.PaymentId is null) return;

        var window = _paymentWindowFactory();
        window.InitialDocumentNumber = row.PaymentNumber;
        window.Owner = Application.Current.MainWindow;
        window.ViewModel.PaymentSaved += () => _ = LoadAsync(CancellationToken.None);
        window.Show();
    }

    // ── Xuất khẩu / In / Gửi ──────────────────────────────────────────────

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"{FilePrefix}_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = BuildWorkbook();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void SendEmail()
    {
        try
        {
            using var workbook = BuildWorkbook();
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, FilePrefix);
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenMailClient(
                ReportTitle,
                $"File báo cáo đã được lưu tại:\n{path}\n\nVui lòng đính kèm file này vào email trước khi gửi.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Email thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void SendZalo()
    {
        try
        {
            using var workbook = BuildWorkbook();
            var path = ReportSharingHelper.SaveWorkbookToTempFile(workbook, FilePrefix);
            ReportSharingHelper.RevealInExplorer(path);
            ReportSharingHelper.OpenZaloApp();

            MessageBox.Show("Đã mở Zalo và thư mục chứa file báo cáo. Vui lòng kéo-thả file để đính kèm.",
                "Gửi Zalo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Gửi Zalo thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void Print()
    {
        var document = BuildReportDocument();

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = printDialog.PrintableAreaHeight;
        document.PageWidth   = printDialog.PrintableAreaWidth;
        document.PagePadding = new Thickness(30);
        document.ColumnWidth = printDialog.PrintableAreaWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, "Sổ kế toán chi tiết quỹ tiền mặt");
    }

    private static readonly string[] ColumnHeaders =
    {
        "Ngày hạch toán", "Ngày chứng từ", "Số phiếu thu", "Số phiếu chi", "Diễn giải", "Tài khoản",
        "TK đối ứng", "Phát sinh Nợ", "Phát sinh Có", "Số tồn", "Người nhận/Người nộp",
        "Mã mục thu/chi", "Tên mục thu/chi",
    };

    private ClosedXML.Excel.XLWorkbook BuildWorkbook()
    {
        var workbook  = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sổ quỹ");

        worksheet.Cell(1, 1).Value = ReportTitle;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(2, 1).Value = Subtitle;

        const int headerRow = 4;
        for (var i = 0; i < ColumnHeaders.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value           = ColumnHeaders[i];
            cell.Style.Font.Bold = true;
        }

        var r = headerRow + 1;
        foreach (var row in Rows)
        {
            if (row.AccountingDate is { } accountingDate) worksheet.Cell(r, 1).Value = accountingDate;
            if (row.DocumentDate   is { } documentDate)   worksheet.Cell(r, 2).Value = documentDate;
            worksheet.Cell(r, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            worksheet.Cell(r, 2).Style.DateFormat.Format = "dd/MM/yyyy";
            worksheet.Cell(r, 3).Value  = row.ReceiptNumber ?? "";
            worksheet.Cell(r, 4).Value  = row.PaymentNumber ?? "";
            worksheet.Cell(r, 5).Value  = row.Description;
            // Mã TK là chữ (vd "1111") — ép kiểu text để Excel không đổi thành số.
            worksheet.Cell(r, 6).SetValue(row.Account);
            worksheet.Cell(r, 7).SetValue(row.CounterAccount);
            worksheet.Cell(r, 8).Value  = row.DebitAmount;
            worksheet.Cell(r, 9).Value  = row.CreditAmount;
            worksheet.Cell(r, 10).Value = row.Balance;
            worksheet.Cell(r, 11).Value = row.PersonName ?? "";
            worksheet.Cell(r, 12).SetValue(row.CategoryCode ?? "");
            worksheet.Cell(r, 13).Value = row.CategoryName ?? "";
            if (row.IsOpening) worksheet.Range(r, 1, r, ColumnHeaders.Length).Style.Font.Bold = true;
            r++;
        }

        worksheet.Cell(r, 5).Value  = $"Số dòng = {RowCount}";
        worksheet.Cell(r, 8).Value  = TotalDebit;
        worksheet.Cell(r, 9).Value  = TotalCredit;
        worksheet.Cell(r, 10).Value = ClosingBalance;
        worksheet.Range(r, 1, r, ColumnHeaders.Length).Style.Font.Bold = true;

        worksheet.Range(headerRow + 1, 8, r, 10).Style.NumberFormat.Format = "#,##0";
        worksheet.Columns().AdjustToContents();
        return workbook;
    }

    private FlowDocument BuildReportDocument()
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 9,
            PagePadding = new Thickness(20),
        };

        doc.Blocks.Add(new Paragraph(new Bold(new Run(ReportTitle)) { FontSize = 16 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 4),
        });
        doc.Blocks.Add(new Paragraph(new Italic(new Run(Subtitle)))
        {
            TextAlignment = TextAlignment.Center,
            FontSize      = 11,
            Margin        = new Thickness(0, 0, 0, 12),
        });

        // Bản in bỏ 2 cột mục thu/chi cho vừa khổ giấy (vẫn có trong lưới và file Excel).
        var table = new Table { CellSpacing = 0 };
        foreach (var width in new[] { 62, 62, 58, 58, 150, 42, 46, 78, 78, 84, 110 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var group = new TableRowGroup();
        group.Rows.Add(PrintRow(true, TextAlignment.Center, ColumnHeaders.Take(11).ToArray()));
        foreach (var row in Rows)
        {
            group.Rows.Add(PrintRow(row.IsOpening, null,
                row.AccountingDate?.ToString("dd/MM/yyyy") ?? "",
                row.DocumentDate?.ToString("dd/MM/yyyy") ?? "",
                row.ReceiptNumber ?? "",
                row.PaymentNumber ?? "",
                row.Description,
                row.Account,
                row.CounterAccount,
                MoneyFormat.Format(row.DebitAmount),
                MoneyFormat.Format(row.CreditAmount),
                MoneyFormat.Format(row.Balance),
                row.PersonName ?? ""));
        }
        group.Rows.Add(PrintRow(true, null,
            "", "", "", "", $"Số dòng = {RowCount}", "", "",
            MoneyFormat.Format(TotalDebit), MoneyFormat.Format(TotalCredit), MoneyFormat.Format(ClosingBalance), ""));

        table.RowGroups.Add(group);
        doc.Blocks.Add(table);
        return doc;
    }

    // Cột 8–10 (Nợ / Có / Số tồn) canh phải; còn lại canh trái, trừ khi ép alignment (dòng tiêu đề).
    private static TableRow PrintRow(bool bold, TextAlignment? alignment, params string[] values)
    {
        var row = new TableRow { Background = bold ? Brushes.WhiteSmoke : Brushes.Transparent };
        for (var i = 0; i < values.Length; i++)
        {
            Inline content = bold ? new Bold(new Run(values[i])) : new Run(values[i]);
            row.Cells.Add(new TableCell(new Paragraph(content)
            {
                TextAlignment = alignment ?? (i is >= 7 and <= 9 ? TextAlignment.Right : TextAlignment.Left),
            })
            {
                Padding         = new Thickness(3),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }
}
