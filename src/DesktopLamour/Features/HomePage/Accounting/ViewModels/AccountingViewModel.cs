// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Views;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class AccountingViewModel : ViewModelBase
{
    private readonly INavigationService   _navigationService;
    private readonly IGetCashLedgerUseCase _getCashLedger;
    private readonly Func<ReceiptWindow>   _receiptWindowFactory;

    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage  = string.Empty;
    [ObservableProperty] private bool    _hasItems;
    [ObservableProperty] private decimal _openingBalance;
    [ObservableProperty] private decimal _closingBalance;
    [ObservableProperty] private DateTime _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime _toDate   = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

    public ObservableCollection<CashLedgerEntryDto> Items { get; } = new();

    public AccountingViewModel(
        INavigationService   navigationService,
        IGetCashLedgerUseCase getCashLedger,
        Func<ReceiptWindow>   receiptWindowFactory)
    {
        _navigationService    = navigationService;
        _getCashLedger        = getCashLedger;
        _receiptWindowFactory = receiptWindowFactory;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void OpenReceipt()
    {
        var window = _receiptWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.ViewModel.ReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
        window.Show();
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
}
