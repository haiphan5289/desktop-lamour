// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Shared.Models;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseTransactionListViewModel : ViewModelBase
{
    private readonly IGetWarehouseTransactionsUseCase _getUseCase;
    private readonly IGetSalesOrderByIdUseCase        _getSalesOrderById;
    private readonly IGetWarehouseReceiptByIdUseCase  _getWarehouseReceiptById;
    private readonly IGetCustomersUseCase             _getCustomers;
    private readonly INavigationService               _navigationService;
    private readonly Func<WarehouseReceiptFormWindow>  _formWindowFactory;
    private readonly Func<SalesOrderWindow>            _salesOrderWindowFactory;
    private readonly Func<SalesOrderPrintWindow>       _printWindowFactory;
    private readonly ILogger<WarehouseTransactionListViewModel> _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage = string.Empty;
    [ObservableProperty] private bool     _hasItems;
    // Mặc định "Đầu tháng đến hiện tại" (áp dụng đồng bộ toàn app — 2026-08-31), thay cho lùi 1
    // tháng (rolling 30 ngày) trước đây.
    [ObservableProperty] private DateTime? _fromDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime? _toDate   = DateTime.Today;
    [ObservableProperty] private WarehouseTransactionResponseDto? _selectedItem;

    // 0 = Tất cả, 1 = Nhập kho, 2 = Xuất kho
    [ObservableProperty] private int _selectedTypeIndex;

    // ── Per-column filter row, embedded directly in each header (no popup) ─────
    // Text columns: plain textbox, case-insensitive Contains against the cell's displayed text.
    // Date/numeric columns: an operator combo (=, ≤, ...) + a typed value, shown side by side.
    // Same pattern as SalesOrderReportDetailViewModel — see docs on ColumnFilterModels.
    [ObservableProperty] private string _filterDocumentNumber      = string.Empty;
    [ObservableProperty] private string _filterDescription         = string.Empty;
    [ObservableProperty] private string _filterDeliveryOrReceiver  = string.Empty;
    [ObservableProperty] private string _filterObjectName          = string.Empty;
    [ObservableProperty] private string _filterHasSalesOrder       = string.Empty;
    [ObservableProperty] private string _filterDocumentTypeLabel   = string.Empty;

    partial void OnFilterDocumentNumberChanged(string value)     => ApplyFilters();
    partial void OnFilterDescriptionChanged(string value)        => ApplyFilters();
    partial void OnFilterDeliveryOrReceiverChanged(string value) => ApplyFilters();
    partial void OnFilterObjectNameChanged(string value)         => ApplyFilters();
    partial void OnFilterHasSalesOrderChanged(string value)      => ApplyFilters();
    partial void OnFilterDocumentTypeLabelChanged(string value)  => ApplyFilters();

    public DateColumnFilter    AccountingDateFilter { get; } = new();
    public DateColumnFilter    DocumentDateFilter   { get; } = new();
    public DateColumnFilter    LedgerDateFilter     { get; } = new();
    public NumericColumnFilter TotalAmountFilter    { get; } = new();

    private void WireColumnFilters()
    {
        AccountingDateFilter.Changed = ApplyFilters;
        DocumentDateFilter.Changed   = ApplyFilters;
        LedgerDateFilter.Changed     = ApplyFilters;
        TotalAmountFilter.Changed    = ApplyFilters;
    }

    public ObservableCollection<WarehouseTransactionResponseDto> Items { get; } = new();

    // Full unfiltered dataset from the last LoadAsync — Items is derived from this via ApplyFilters.
    private List<WarehouseTransactionResponseDto> _allItems = new();

    public WarehouseTransactionListViewModel(
        IGetWarehouseTransactionsUseCase getUseCase,
        IGetSalesOrderByIdUseCase        getSalesOrderById,
        IGetWarehouseReceiptByIdUseCase  getWarehouseReceiptById,
        IGetCustomersUseCase             getCustomers,
        INavigationService               navigationService,
        Func<WarehouseReceiptFormWindow>  formWindowFactory,
        Func<SalesOrderWindow>            salesOrderWindowFactory,
        Func<SalesOrderPrintWindow>       printWindowFactory,
        ILogger<WarehouseTransactionListViewModel> logger)
    {
        _getUseCase              = getUseCase;
        _getSalesOrderById       = getSalesOrderById;
        _getWarehouseReceiptById = getWarehouseReceiptById;
        _getCustomers            = getCustomers;
        _navigationService       = navigationService;
        _formWindowFactory       = formWindowFactory;
        _salesOrderWindowFactory = salesOrderWindowFactory;
        _printWindowFactory      = printWindowFactory;
        _logger                  = logger;

        WireColumnFilters();
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
        window.Initialize(null, isFromWarehouseExport: true);
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
            _allItems = transactions.OrderByDescending(t => t.DocumentDate).ToList();
            ApplyFilters();
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

    // Re-derives Items from _allItems using the active column filters.
    private void ApplyFilters()
    {
        Items.Clear();
        foreach (var item in _allItems.Where(MatchesAllFilters))
            Items.Add(item);

        HasItems = Items.Count > 0;
    }

    private bool MatchesAllFilters(WarehouseTransactionResponseDto item)
        => AccountingDateFilter.Matches(item.AccountingDate)
        && DocumentDateFilter.Matches(item.DocumentDate)
        && Matches(FilterDocumentNumber, item.DocumentNumber)
        && Matches(FilterDescription, item.Description ?? string.Empty)
        && TotalAmountFilter.Matches(item.TotalAmount)
        && Matches(FilterDeliveryOrReceiver, item.DeliveryOrReceiver ?? string.Empty)
        && Matches(FilterObjectName, item.ObjectName ?? string.Empty)
        && Matches(FilterHasSalesOrder, item.HasSalesOrder ? "✓ Đã lập" : string.Empty)
        && LedgerDateFilter.Matches(item.LedgerDate)
        && Matches(FilterDocumentTypeLabel, item.DocumentTypeLabel);

    private static bool Matches(string filter, string cellText)
        => string.IsNullOrWhiteSpace(filter)
        || cellText.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    partial void OnSelectedTypeIndexChanged(int value) => LoadCommand.Execute(null);

    // Double-click 1 dòng chứng từ:
    // - Dòng Xuất kho (TransactionType="Export") LUÔN phát sinh từ 1 Sales Order đã ghi sổ (số
    //   chứng từ mang prefix XK — xem GetWarehouseTransactionsUseCase.MapSalesOrder phía BE) →
    //   mở thẳng popup "In Hóa Đơn" (SalesOrderPrintWindow), y hệt nút 🖨 bên trong popup "Thêm
    //   chứng từ bán hàng" — từ màn Kho user cần xem/in lại hóa đơn nhanh, không cần mở form sửa.
    //   (Đã thử mở SalesOrderWindow (form sửa) trước đó theo yêu cầu — user phản hồi muốn thẳng
    //   màn in hóa đơn thay vì form sửa, nên đổi lại theo hướng này.)
    // - Dòng Nhập kho (TransactionType="Import") → mở thẳng phiếu nhập kho thật
    //   (WarehouseReceiptFormWindow, Id trùng với WarehouseReceipt.Id) thay vì popup chỉ-xem cũ —
    //   cho phép "Bỏ ghi" rồi sửa rồi "Ghi sổ" lại ngay từ đây.
    [RelayCommand]
    private async Task ShowDetailAsync(WarehouseTransactionResponseDto? item, CancellationToken ct = default)
    {
        if (item is null) return;

        if (item.TransactionType.Equals("Export", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var order = await _getSalesOrderById.ExecuteAsync(item.Id, ct);
                if (order is null)
                {
                    MessageBox.Show($"Không tìm thấy chứng từ bán hàng '{item.DocumentNumber}'.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // SalesOrderResponseDto không mang Phone/Address khách hàng (chỉ CustomerId/Name)
                // — tra thêm qua danh sách khách hàng để hóa đơn in đủ thông tin, giống cách
                // SalesOrderViewModel.ShowPrintPreview lấy Phone/Address từ SelectedCustomer.
                var customers = await _getCustomers.ExecuteAsync(ct);
                var customer  = customers.FirstOrDefault(c => c.Id == order.CustomerId);

                var printWindow = _printWindowFactory();
                // Trừ cọc không được lưu lại trên SalesOrder đã ghi sổ (chỉ gửi riêng qua
                // DepositDeduction — xem sales.md), nên không tái tạo được số trừ cọc gốc ở đây;
                // hóa đơn in từ màn Kho tạm thời không hiện dòng trừ cọc (depositDeductionAmount=0).
                printWindow.Initialize(order, customer?.Phone, customer?.Address);
                printWindow.Owner = Application.Current.MainWindow;
                printWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load sales order {DocumentNumber} for print from Kho screen", item.DocumentNumber);
                MessageBox.Show($"Không thể tải hóa đơn: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return;
        }

        try
        {
            var receipt = await _getWarehouseReceiptById.ExecuteAsync(item.Id, ct);
            if (receipt is null)
            {
                MessageBox.Show($"Không tìm thấy phiếu nhập kho '{item.DocumentNumber}'.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = _formWindowFactory();
            window.Initialize(receipt);
            // Cho phép Trước/Sau/Thêm duyệt ngay trong popup — chỉ tính các dòng Nhập kho
            // ("Import") trong danh sách đang xem, bỏ qua dòng Xuất kho (mở popup khác hẳn).
            var siblingIds = Items
                .Where(i => i.TransactionType.Equals("Import", StringComparison.OrdinalIgnoreCase))
                .Select(i => i.Id)
                .ToList();
            window.SetSiblingContext(siblingIds, siblingIds.IndexOf(item.Id));
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true)
                LoadCommand.Execute(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load warehouse receipt {DocumentNumber} for edit from Kho screen", item.DocumentNumber);
            MessageBox.Show($"Không thể tải phiếu nhập kho: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
