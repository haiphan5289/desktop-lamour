// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

public partial class AccountingViewModel : ViewModelBase
{
    private readonly INavigationService    _navigationService;
    private readonly IGetCashLedgerUseCase _getCashLedger;

    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private string  _errorMessage  = string.Empty;
    [ObservableProperty] private bool    _hasItems;
    [ObservableProperty] private decimal _openingBalance;
    [ObservableProperty] private decimal _closingBalance;
    [ObservableProperty] private DateTime _fromDate = new(2023, 11, 1);
    [ObservableProperty] private DateTime _toDate   = new(2023, 11, 30);

    public ObservableCollection<CashLedgerEntryDto> Items { get; } = new();

    public AccountingViewModel(
        INavigationService    navigationService,
        IGetCashLedgerUseCase getCashLedger)
    {
        _navigationService = navigationService;
        _getCashLedger     = getCashLedger;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

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
