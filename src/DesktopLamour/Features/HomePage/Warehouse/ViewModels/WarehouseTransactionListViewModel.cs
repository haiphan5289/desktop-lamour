// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseTransactionListViewModel : ViewModelBase
{
    private readonly IGetWarehouseTransactionsUseCase _getUseCase;
    private readonly INavigationService               _navigationService;
    private readonly Func<WarehouseReceiptFormWindow>  _formWindowFactory;
    private readonly Func<SalesOrderWindow>            _salesOrderWindowFactory;
    private readonly ILogger<WarehouseTransactionListViewModel> _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasItems;
    [ObservableProperty] private DateTime? _fromDate = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime? _toDate   = DateTime.Today;
    [ObservableProperty] private WarehouseTransactionResponseDto? _selectedItem;

    // 0 = Tất cả, 1 = Nhập kho, 2 = Xuất kho
    [ObservableProperty] private int _selectedTypeIndex;

    public ObservableCollection<WarehouseTransactionResponseDto> Items { get; } = new();

    public WarehouseTransactionListViewModel(
        IGetWarehouseTransactionsUseCase getUseCase,
        INavigationService               navigationService,
        Func<WarehouseReceiptFormWindow>  formWindowFactory,
        Func<SalesOrderWindow>            salesOrderWindowFactory,
        ILogger<WarehouseTransactionListViewModel> logger)
    {
        _getUseCase              = getUseCase;
        _navigationService       = navigationService;
        _formWindowFactory       = formWindowFactory;
        _salesOrderWindowFactory = salesOrderWindowFactory;
        _logger                  = logger;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void NavigateToTongHopTonKho()
        => _navigationService.NavigateTo(NavigationRoutes.Warehouse.TongHopTonKho);

    // "Phiếu Nhập" — mở thẳng form tạo phiếu nhập kho mới.
    [RelayCommand]
    private void OpenForm()
    {
        var window = _formWindowFactory();
        window.Owner = Application.Current.MainWindow;
        var result = window.ShowDialog();
        if (result == true)
            LoadCommand.Execute(null);
    }

    // "Phiếu Xuất" — hệ thống không có luồng tạo "phiếu xuất kho" riêng, xuất kho chỉ sinh ra
    // từ 1 Chứng từ bán hàng đã ghi sổ, nên mở thẳng form tạo Sales Order mới.
    [RelayCommand]
    private void OpenSalesOrder()
    {
        var window = _salesOrderWindowFactory();
        window.Initialize(null);
        window.Owner = Application.Current.MainWindow;
        var result = window.ShowDialog();
        if (result == true)
            LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var type = SelectedTypeIndex switch
            {
                1 => "import",
                2 => "export",
                _ => null,
            };

            var transactions = await _getUseCase.ExecuteAsync(FromDate, ToDate, type, ct);
            Items.Clear();
            foreach (var t in transactions.OrderByDescending(t => t.DocumentDate))
                Items.Add(t);
            HasItems = Items.Count > 0;
            SelectedItem = Items.FirstOrDefault();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load warehouse transactions");
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    partial void OnSelectedTypeIndexChanged(int value) => LoadCommand.Execute(null);
}
