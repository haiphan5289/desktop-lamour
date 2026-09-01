// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;
using DesktopLamour.Shared.Models;

namespace DesktopLamour.Features.HomePage.Deposits.ViewModels;

public partial class DepositDeductionReportViewModel : ViewModelBase
{
    private readonly INavigationService               _navigationService;
    private readonly IGetDepositDeductionsUseCase      _getDeductions;
    private readonly IDeleteDepositDeductionUseCase    _deleteDeduction;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasItems;
    [ObservableProperty] private decimal  _totalDeducted;
    // Mặc định "Đầu tháng đến hiện tại" (áp dụng đồng bộ toàn app — 2026-08-31), thay cho không lọc
    // (hiện toàn bộ lịch sử) trước đây.
    [ObservableProperty] private DateTime? _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime? _toDate   = DateTime.Today;
    [ObservableProperty] private string   _filterKeyword = string.Empty;
    [ObservableProperty] private DepositDeductionResponseDto? _selectedDeduction;

    // ── Per-column filter row, embedded directly in each header (no popup) ─────
    // Text columns: plain textbox, case-insensitive Contains against the cell's displayed text.
    // Date/numeric columns: an operator combo (=, ≤, ...) + a typed value, shown side by side.
    // Composes (AND) with the existing top-toolbar keyword/date-range filter above.
    [ObservableProperty] private string _filterDocumentNumber           = string.Empty;
    [ObservableProperty] private string _filterDepositDocumentNumber    = string.Empty;
    [ObservableProperty] private string _filterSalesOrderDocumentNumber = string.Empty;
    [ObservableProperty] private string _filterCustomerName             = string.Empty;
    [ObservableProperty] private string _filterEmployeeName             = string.Empty;
    [ObservableProperty] private string _filterDescription              = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value)           => ApplyFilter();
    partial void OnFilterDepositDocumentNumberChanged(string value)    => ApplyFilter();
    partial void OnFilterSalesOrderDocumentNumberChanged(string value) => ApplyFilter();
    partial void OnFilterCustomerNameChanged(string value)             => ApplyFilter();
    partial void OnFilterEmployeeNameChanged(string value)             => ApplyFilter();
    partial void OnFilterDescriptionChanged(string value)              => ApplyFilter();

    public DateColumnFilter    AccountingDateFilter { get; } = new();
    public NumericColumnFilter AmountFilter         { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = ApplyFilter;
        AmountFilter.Changed         = ApplyFilter;
    }

    private List<DepositDeductionResponseDto> _allItems = new();

    public ObservableCollection<DepositDeductionResponseDto> Items { get; } = new();

    public DepositDeductionReportViewModel(
        INavigationService            navigationService,
        IGetDepositDeductionsUseCase  getDeductions,
        IDeleteDepositDeductionUseCase deleteDeduction)
    {
        _navigationService = navigationService;
        _getDeductions     = getDeductions;
        _deleteDeduction   = deleteDeduction;

        WireColumnFilters();
    }

    partial void OnFilterKeywordChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _getDeductions.ExecuteAsync(
                customerId: null, employeeId: null, salesOrderId: null,
                fromDate: FromDate, toDate: ToDate, ct);

            _allItems = result.OrderByDescending(x => x.AccountingDate).ToList();
            ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (SelectedDeduction is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa đơn trừ cọc '{SelectedDeduction.DocumentNumber}'? Số tiền sẽ được hoàn lại vào cọc '{SelectedDeduction.DepositDocumentNumber}'.",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteDeduction.ExecuteAsync(SelectedDeduction.Id, ct);
            await LoadAsync(ct);
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    private bool HasSelection => SelectedDeduction is not null;

    partial void OnSelectedDeductionChanged(DepositDeductionResponseDto? value)
        => DeleteCommand.NotifyCanExecuteChanged();

    private void ApplyFilter()
    {
        var filtered = _allItems.Where(MatchesAllFilters);

        if (!string.IsNullOrWhiteSpace(FilterKeyword))
            filtered = filtered.Where(x =>
                x.CustomerName.Contains(FilterKeyword, StringComparison.OrdinalIgnoreCase) ||
                x.SalesOrderDocumentNumber.Contains(FilterKeyword, StringComparison.OrdinalIgnoreCase) ||
                x.DepositDocumentNumber.Contains(FilterKeyword, StringComparison.OrdinalIgnoreCase));

        Items.Clear();
        foreach (var item in filtered) Items.Add(item);

        HasItems       = Items.Count > 0;
        TotalDeducted  = Items.Sum(x => x.Amount);
    }

    private bool MatchesAllFilters(DepositDeductionResponseDto item)
        => AccountingDateFilter.Matches(item.AccountingDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterDepositDocumentNumber, item.DepositDocumentNumber)
        && Matches(FilterSalesOrderDocumentNumber, item.SalesOrderDocumentNumber)
        && Matches(FilterCustomerName, item.CustomerName)
        && Matches(FilterEmployeeName, item.EmployeeName ?? string.Empty)
        && AmountFilter.Matches(item.Amount)
        && Matches(FilterDescription, item.Description ?? string.Empty);

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);
}
