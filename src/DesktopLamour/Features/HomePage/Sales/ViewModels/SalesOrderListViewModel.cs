// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderListViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetSalesOrdersUseCase      _getOrders;
    private readonly IDeleteSalesOrderUseCase    _deleteOrder;
    private readonly IHoldSalesOrderUseCase      _holdOrder;
    private readonly Func<SalesOrderWindow>      _formWindowFactory;

    [ObservableProperty] private bool                _isLoading;
    [ObservableProperty] private bool                _hasError;
    [ObservableProperty] private string              _errorMessage   = string.Empty;
    [ObservableProperty] private bool                _hasSalesOrders;
    [ObservableProperty] private SalesOrderListItem? _selectedOrder;
    [ObservableProperty] private DateTime?           _filterFromDate;
    [ObservableProperty] private DateTime?           _filterToDate;

    // Tổng dòng footer — cộng dồn trên danh sách ĐÃ LỌC (SalesOrders), không phải _allItems, để khớp
    // với những gì người dùng đang thấy trên lưới.
    [ObservableProperty] private int                 _totalCount;
    [ObservableProperty] private decimal             _totalGrossSum;
    [ObservableProperty] private decimal             _totalDiscountSum;
    [ObservableProperty] private decimal             _totalPaymentSum;

    // 1 ô tìm kiếm chung (AND với FilterFromDate/FilterToDate ở trên) — khớp OR trên các trường
    // text chính, không phân biệt hoa/thường.
    [ObservableProperty] private string _searchText = string.Empty;

    private readonly List<SalesOrderListItem> _allItems = new();

    public ObservableCollection<SalesOrderListItem> SalesOrders { get; } = new();

    private bool HasSelection => SelectedOrder is not null;

    public SalesOrderListViewModel(
        INavigationService         navigationService,
        IGetSalesOrdersUseCase     getOrders,
        IDeleteSalesOrderUseCase   deleteOrder,
        IHoldSalesOrderUseCase     holdOrder,
        Func<SalesOrderWindow>     formWindowFactory)
    {
        _navigationService = navigationService;
        _getOrders         = getOrders;
        _deleteOrder       = deleteOrder;
        _holdOrder         = holdOrder;
        _formWindowFactory = formWindowFactory;

        // Mặc định mở màn hình chỉ hiện chứng từ của HÔM NAY (không dồn hết lịch sử lại) —
        // giống MISA. Người dùng vẫn có thể đổi Từ ngày/Đến ngày để xem ngày khác.
        _filterFromDate = DateTime.Today;
        _filterToDate   = DateTime.Today;
    }

    partial void OnSelectedOrderChanged(SalesOrderListItem? value)
    {
        EditSalesOrderCommand.NotifyCanExecuteChanged();
        DeleteSalesOrderCommand.NotifyCanExecuteChanged();
        HoldSalesOrderCommand.NotifyCanExecuteChanged();
    }

    partial void OnFilterFromDateChanged(DateTime? value) => ApplyFilter();
    partial void OnFilterToDateChanged(DateTime? value)   => ApplyFilter();
    partial void OnSearchTextChanged(string value)        => ApplyFilter();

    // Lọc đã tự áp dụng ngay khi đổi ngày/tìm kiếm (live filter) — nút "Lọc" chỉ để người dùng có
    // affordance rõ ràng để bấm, giống hàng lọc màn Quỹ.
    [RelayCommand]
    private void Filter() => ApplyFilter();

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadSalesOrdersAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getOrders.ExecuteAsync(ct);
            _allItems.Clear();
            foreach (var dto in items)
                _allItems.Add(SalesOrderListItem.FromDto(dto));
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

    [RelayCommand]
    private async Task AddSalesOrderAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn chỉnh sửa chứng từ '{SelectedOrder.DocumentNumber}'?",
            "Xác nhận chỉnh sửa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        var window = _formWindowFactory();
        window.Initialize(SelectedOrder.Original);
        if (window.ShowDialog() == true)
            await LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task HoldSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        IsLoading = true;
        try
        {
            var updated = await _holdOrder.ExecuteAsync(SelectedOrder.Id, ct);
            var index = _allItems.IndexOf(SelectedOrder);
            if (index >= 0) _allItems[index] = SalesOrderListItem.FromDto(updated);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Treo đơn thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSalesOrderAsync(CancellationToken ct = default)
    {
        if (SelectedOrder is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{SelectedOrder.DocumentNumber}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteOrder.ExecuteAsync(SelectedOrder.Id, ct);
            _allItems.Remove(SelectedOrder);
            ApplyFilter();
            SelectedOrder = null;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    private void ApplyFilter()
    {
        var filtered = _allItems.AsEnumerable();

        if (FilterFromDate.HasValue)
            filtered = filtered.Where(o => o.DocumentDate.Date >= FilterFromDate.Value.Date);

        if (FilterToDate.HasValue)
            filtered = filtered.Where(o => o.DocumentDate.Date <= FilterToDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(o =>
                Matches(o.StatusLabel, SearchText) ||
                Matches(o.DocumentNumber, SearchText) ||
                Matches(o.CustomerName, SearchText) ||
                Matches(o.EmployeeName, SearchText) ||
                Matches(o.Notes, SearchText));

        SalesOrders.Clear();
        foreach (var item in filtered.OrderByDescending(o => o.DocumentDate))
            SalesOrders.Add(item);

        HasSalesOrders   = SalesOrders.Count > 0;
        TotalCount       = SalesOrders.Count;
        TotalGrossSum    = SalesOrders.Sum(o => o.TotalGross);
        TotalDiscountSum = SalesOrders.Sum(o => o.TotalDiscount);
        TotalPaymentSum  = SalesOrders.Sum(o => o.TotalPayment);
    }

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));
}
