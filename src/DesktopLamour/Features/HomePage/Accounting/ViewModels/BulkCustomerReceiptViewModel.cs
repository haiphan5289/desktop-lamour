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
// Số tiền từng dòng (thu 1 phần), chọn NV thu nợ, rồi Cất — mỗi khách hàng khác nhau trong danh
// sách sinh ra 1 Receipt riêng (BE: CreateBulkCustomerReceiptUseCase gom theo CustomerId).
public partial class BulkCustomerReceiptViewModel : ViewModelBase
{
    public event Action? RequestClose;

    private readonly ICreateBulkCustomerReceiptUseCase _createBulk;
    private readonly IGetEmployeesUseCase              _getEmployees;
    private readonly ILogger<BulkCustomerReceiptViewModel> _logger;

    private string  _debitAccount = "Cash111";
    private string? _bankAccount;

    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private ISearchableItem? _selectedCollectorEmployee;

    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";

    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<BulkReceiptLineItem> Lines { get; } = new();

    public BulkCustomerReceiptViewModel(
        ICreateBulkCustomerReceiptUseCase createBulk,
        IGetEmployeesUseCase              getEmployees,
        ILogger<BulkCustomerReceiptViewModel> logger)
    {
        _createBulk    = createBulk;
        _getEmployees  = getEmployees;
        _logger        = logger;
    }

    public void Initialize(
        IReadOnlyList<OutstandingSalesOrderCheckItem> selected,
        string debitAccount, string? bankAccount, int? collectorEmployeeId)
    {
        _debitAccount = debitAccount;
        _bankAccount  = bankAccount;

        Lines.Clear();
        foreach (var s in selected)
        {
            var line = new BulkReceiptLineItem(s);
            line.PropertyChanged += (_, _) => RecalculateTotal();
            Lines.Add(line);
        }
        RecalculateTotal();

        _ = LoadEmployeesAsync(collectorEmployeeId);
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
                Lines = Lines.Select(l => new BulkReceiptLineRequestDto
                {
                    SalesOrderId = l.SalesOrderId,
                    Amount       = l.Amount,
                }).ToList(),
            };

            var result  = await _createBulk.ExecuteAsync(request, ct);
            var numbers = string.Join(", ", result.Receipts.Select(r => r.DocumentNumber));
            MessageBox.Show($"Đã tạo {result.Receipts.Count} phiếu thu: {numbers}", "Thu tiền thành công",
                MessageBoxButton.OK, MessageBoxImage.Information);

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
