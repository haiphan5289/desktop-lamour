// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

// Popup 2/2 của "Phiếu Thu Hàng Loạt" — xác nhận danh sách đơn đã chọn ở popup tìm kiếm, cho sửa
// Số tiền từng dòng (thu 1 phần), chọn NV thu nợ, rồi Cất. 2026-08-26 (so ảnh mẫu MISA): tạo
// ĐÚNG 1 Receipt duy nhất cho toàn bộ danh sách (khác nhiều khách hàng vẫn chung 1 phiếu) — không
// còn group theo CustomerId ra nhiều phiếu như bản trước (xem CreateBulkCustomerReceiptUseCase BE).
public partial class BulkCustomerReceiptViewModel : ViewModelBase
{
    public event Action? RequestClose;

    private readonly ICreateBulkCustomerReceiptUseCase _createBulk;
    private readonly IGetEmployeesUseCase              _getEmployees;
    private readonly IGetNextReceiptCodeUseCase        _getNextCode;
    private readonly ILogger<BulkCustomerReceiptViewModel> _logger;

    private string  _debitAccount = "Cash111";
    private string? _bankAccount;

    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Thông tin chung — khớp ảnh mẫu MISA ─────────────────────────────────
    [ObservableProperty] private string  _payerName = string.Empty;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _attachment;
    [ObservableProperty] private ISearchableItem? _selectedCollectorEmployee;
    // "Tham chiếu" — tự nối danh sách Số chứng từ (BH...) của các đơn đã chọn, chỉ để xem, không sửa tay.
    [ObservableProperty] private string  _reference = string.Empty;

    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    // Dự đoán số chứng từ tiếp theo để hiển thị trước khi lưu — BE tự gán số thật lúc Cất (giống
    // GenerateNextDocumentNumber() ở các form khác), có thể lệch nếu có phiếu khác vừa tạo song song.
    [ObservableProperty] private string   _documentNumber = "";

    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";

    public string ReasonLabel => "Thu tiền khách hàng";

    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<BulkReceiptLineItem> Lines { get; } = new();

    public BulkCustomerReceiptViewModel(
        ICreateBulkCustomerReceiptUseCase createBulk,
        IGetEmployeesUseCase              getEmployees,
        IGetNextReceiptCodeUseCase        getNextCode,
        ILogger<BulkCustomerReceiptViewModel> logger)
    {
        _createBulk    = createBulk;
        _getEmployees  = getEmployees;
        _getNextCode   = getNextCode;
        _logger        = logger;
    }

    public void Initialize(
        IReadOnlyList<OutstandingSalesOrderCheckItem> selected,
        string debitAccount, string? bankAccount, int? collectorEmployeeId)
    {
        _debitAccount = debitAccount;
        _bankAccount  = bankAccount;

        var debitDisplay = debitAccount switch
        {
            "Bank112" => "112",
            _         => "111",
        };

        Lines.Clear();
        foreach (var s in selected)
        {
            var line = new BulkReceiptLineItem(s)
            {
                DebitAccountDisplay  = debitDisplay,
                CreditAccountDisplay = "131",
            };
            line.PropertyChanged += (_, _) => RecalculateTotal();
            Lines.Add(line);
        }
        RecalculateTotal();

        Reference = string.Join(", ", Lines.Select(l => l.DocumentNumber).Distinct());

        _ = LoadEmployeesAsync(collectorEmployeeId);
        _ = LoadNextCodeAsync();
    }

    private async Task LoadEmployeesAsync(int? preselectId)
    {
        try
        {
            var employees = await _getEmployees.ExecuteAsync();
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));

            if (preselectId.HasValue)
                SelectedCollectorEmployee = Employees.FirstOrDefault(e => e.Id == preselectId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load employees for BulkCustomerReceipt confirm popup");
        }
    }

    private async Task LoadNextCodeAsync()
    {
        try { DocumentNumber = await _getNextCode.ExecuteAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not predict next receipt code for BulkCustomerReceipt"); }
    }

    partial void OnSelectedCollectorEmployeeChanged(ISearchableItem? value)
    {
        // Khớp ảnh mẫu MISA: "Người nộp" = tên nhân viên thu (không phải tên khách hàng, vì phiếu
        // gộp nhiều khách hàng khác nhau) — chỉ auto-fill nếu user chưa tự gõ gì khác.
        if (value is not null && string.IsNullOrWhiteSpace(PayerName))
            PayerName = value.Name;
    }

    private void RecalculateTotal()
    {
        TotalAmount  = Lines.Sum(l => l.Amount);
        LineSummary  = $"Số dòng = {Lines.Count}";
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (Lines.Count == 0) return;

        var invalid = Lines.FirstOrDefault(l => l.Amount <= 0 || l.Amount > l.MaxAmount);
        if (invalid is not null)
        {
            HasError     = true;
            ErrorMessage = $"Số tiền thu của chứng từ '{invalid.DocumentNumber}' phải > 0 và không vượt quá số còn nợ ({invalid.MaxAmount:N0}).";
            return;
        }

        IsBusy = true;
        try
        {
            var request = new CreateBulkCustomerReceiptRequestDto
            {
                AccountingDate      = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
                DocumentDate        = DateTime.SpecifyKind(DocumentDate.Date, DateTimeKind.Unspecified),
                DebitAccount        = _debitAccount,
                BankAccount         = _bankAccount,
                CollectorEmployeeId = SelectedCollectorEmployee?.Id,
                PayerName           = string.IsNullOrWhiteSpace(PayerName) ? null : PayerName.Trim(),
                Address             = string.IsNullOrWhiteSpace(Address)   ? null : Address.Trim(),
                Attachment          = string.IsNullOrWhiteSpace(Attachment) ? null : Attachment.Trim(),
                Lines = Lines.Select(l => new BulkReceiptLineRequestDto
                {
                    SalesOrderId = l.SalesOrderId,
                    Amount       = l.Amount,
                }).ToList(),
            };

            var result = await _createBulk.ExecuteAsync(request, ct);
            MessageBox.Show($"Đã tạo phiếu thu {result.Receipt.DocumentNumber} ({Lines.Count} dòng, tổng {TotalAmount:N0}).",
                "Thu tiền thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create bulk customer receipt");
            HasError     = true;
            ErrorMessage = $"Không thể tạo phiếu thu: {ex.Message}";
        }
        finally { IsBusy = false; }
    }
}
