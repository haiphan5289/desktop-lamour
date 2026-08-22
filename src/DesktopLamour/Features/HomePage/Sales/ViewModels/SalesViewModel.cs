// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Deposits.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private const string AllOption = "Tất cả";

    private readonly INavigationService _navigationService;
    private readonly Func<DepositWindow> _depositWindowFactory;
    private readonly IGetSalesOrderSummaryReportUseCase _getSummaryReport;
    private readonly IGetProductsUseCase  _getProducts;
    private readonly IGetEmployeesUseCase _getEmployees;
    private readonly IGetCustomersUseCase _getCustomers;

    private List<Product> _allProducts = new();

    // ── Panel "Báo cáo theo Mặt hàng" nhúng ngay trên màn hình hub "Bán hàng" ───
    // 2026-08-22: ban đầu cố định "đầu tháng đến hôm nay", không filter gì khác. Theo yêu cầu tiếp
    // theo, chuyển nguyên bộ field lọc từ popup SalesOrderReportFilterWindow (đã xoá, không còn
    // dùng) lên thẳng panel này — tile "📊 Báo cáo" + trang SalesOrderReportView không còn là lối
    // vào báo cáo nào cả, panel trên màn hub là cách DUY NHẤT xem báo cáo bán hàng.
    [ObservableProperty] private bool   _isReportPanelExpanded = true;
    [ObservableProperty] private bool   _isReportLoading;
    [ObservableProperty] private bool   _hasReportError;
    [ObservableProperty] private string _reportErrorMessage = string.Empty;
    [ObservableProperty] private bool   _hasReportRows;

    [ObservableProperty] private ISearchableItem? _selectedEmployee;
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private string?          _selectedUnit;
    [ObservableProperty] private string?          _selectedCategory;
    [ObservableProperty] private string           _selectedPeriod = SalesOrderReportPeriods.MonthToDate;
    [ObservableProperty] private bool             _areAllProductsSelected;
    [ObservableProperty] private DateTime?        _fromDate;
    [ObservableProperty] private DateTime?        _toDate;

    public ObservableCollection<ISearchableItem>  Employees    { get; } = new();
    public ObservableCollection<ISearchableItem>  Customers    { get; } = new();
    public ObservableCollection<string>           Units        { get; } = new();
    public ObservableCollection<string>           Categories   { get; } = new();
    public ObservableCollection<ProductCheckItem> ProductItems { get; } = new();
    public IReadOnlyList<string>                  Periods      { get; } = SalesOrderReportPeriods.All;

    public ObservableCollection<ProductSalesSummaryRow> ReportRows { get; } = new();

    public SalesViewModel(
        INavigationService navigationService,
        Func<DepositWindow> depositWindowFactory,
        IGetSalesOrderSummaryReportUseCase getSummaryReport,
        IGetProductsUseCase  getProducts,
        IGetEmployeesUseCase getEmployees,
        IGetCustomersUseCase getCustomers)
    {
        _navigationService    = navigationService;
        _depositWindowFactory = depositWindowFactory;
        _getSummaryReport     = getSummaryReport;
        _getProducts          = getProducts;
        _getEmployees         = getEmployees;
        _getCustomers         = getCustomers;

        ApplyPeriodPreset(SelectedPeriod);
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private void NavigateToSalesOrders()
        => _navigationService.NavigateTo(NavigationRoutes.SalesOrders.List);

    [RelayCommand]
    private void NavigateToSalesReturns()
        => _navigationService.NavigateTo(NavigationRoutes.SalesReturns.List);

    [RelayCommand]
    private void OpenDeposit()
    {
        var window = _depositWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.ShowDialog();
    }

    [RelayCommand]
    private void NavigateToDepositDeductionReport()
        => _navigationService.NavigateTo(NavigationRoutes.Deposits.DeductionReport);

    [RelayCommand]
    private void ToggleReportPanel() => IsReportPanelExpanded = !IsReportPanelExpanded;

    // Gọi 1 lần khi màn hình mở (xem SalesView.xaml.cs Loaded) — load lookup cho 5 ô filter rồi
    // load luôn báo cáo với filter mặc định (Kỳ báo cáo = Đầu tháng đến hiện tại, không lọc gì khác).
    [RelayCommand]
    private async Task InitializeReportPanelAsync(CancellationToken ct = default)
    {
        await LoadLookupsAsync(ct);
        await LoadReportPanelAsync(ct);
    }

    private async Task LoadLookupsAsync(CancellationToken ct = default)
    {
        try
        {
            var productTask  = _getProducts.ExecuteAsync(ct);
            var employeeTask = _getEmployees.ExecuteAsync(ct);
            var customerTask = _getCustomers.ExecuteAsync(ct);

            await Task.WhenAll(productTask, employeeTask, customerTask);

            _allProducts = productTask.Result.ToList();

            Employees.Clear();
            foreach (var e in employeeTask.Result) Employees.Add(e);

            Customers.Clear();
            foreach (var c in customerTask.Result) Customers.Add(c);

            Units.Clear();
            Units.Add(AllOption);
            foreach (var u in _allProducts.Select(p => p.Unit)
                         .Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().OrderBy(u => u))
                Units.Add(u);
            SelectedUnit ??= AllOption;

            Categories.Clear();
            Categories.Add(AllOption);
            foreach (var c in _allProducts.Select(p => p.CategoryName)
                         .Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c))
                Categories.Add(c!);
            SelectedCategory ??= AllOption;

            ApplyProductFilter();
        }
        catch (OperationCanceledException) { }
    }

    partial void OnSelectedUnitChanged(string? value) => ApplyProductFilter();
    partial void OnSelectedCategoryChanged(string? value) => ApplyProductFilter();
    partial void OnSelectedPeriodChanged(string value) => ApplyPeriodPreset(value);

    partial void OnAreAllProductsSelectedChanged(bool value)
    {
        foreach (var item in ProductItems) item.IsSelected = value;
    }

    private static bool IsAnyOption(string? value) => string.IsNullOrEmpty(value) || value == AllOption;

    private void ApplyProductFilter()
    {
        var filtered = _allProducts.Where(p =>
            (IsAnyOption(SelectedUnit) || p.Unit == SelectedUnit) &&
            (IsAnyOption(SelectedCategory) || p.CategoryName == SelectedCategory));

        ProductItems.Clear();
        foreach (var p in filtered.OrderBy(p => p.Code))
            ProductItems.Add(new ProductCheckItem(p));

        AreAllProductsSelected = false;
    }

    private void ApplyPeriodPreset(string period)
    {
        var today = DateTime.Today;
        switch (period)
        {
            case SalesOrderReportPeriods.Today:
                FromDate = today; ToDate = today;
                break;
            case SalesOrderReportPeriods.Yesterday:
                FromDate = today.AddDays(-1); ToDate = today.AddDays(-1);
                break;
            case SalesOrderReportPeriods.ThisWeek:
                FromDate = StartOfWeek(today); ToDate = today;
                break;
            case SalesOrderReportPeriods.LastWeek:
                var lastWeekStart = StartOfWeek(today).AddDays(-7);
                FromDate = lastWeekStart; ToDate = lastWeekStart.AddDays(6);
                break;
            case SalesOrderReportPeriods.ThisMonth:
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate   = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                break;
            case SalesOrderReportPeriods.LastMonth:
                var lastMonth = today.AddMonths(-1);
                FromDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                ToDate   = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                break;
            case SalesOrderReportPeriods.MonthToDate:
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate   = today;
                break;
            case SalesOrderReportPeriods.ThisQuarter:
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                FromDate = new DateTime(today.Year, quarterStartMonth, 1);
                ToDate   = today;
                break;
            case SalesOrderReportPeriods.ThisYear:
                FromDate = new DateTime(today.Year, 1, 1);
                ToDate   = today;
                break;
            case SalesOrderReportPeriods.Custom:
                break;
        }
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    [RelayCommand]
    private void ClearReportFilters()
    {
        SelectedEmployee = null;
        SelectedCustomer = null;
        SelectedUnit     = AllOption;
        SelectedCategory = AllOption;
        SelectedPeriod   = SalesOrderReportPeriods.Custom;
        FromDate = null;
        ToDate   = null;
        foreach (var item in ProductItems) item.IsSelected = false;

        _ = LoadReportPanelAsync();
    }

    // "🔍 Lọc" — cộng dồn theo ProductId (giống ReportType "Mặt hàng" cũ của SalesOrderReportViewModel,
    // vẫn gộp mọi khách hàng/nhân viên của cùng 1 sản phẩm thành 1 dòng) nhưng giờ nhận đủ bộ filter
    // Mặt hàng/Nhân viên/Khách hàng/ĐVT/Nhóm VTHH/khoảng ngày thay vì cố định đầu-tháng-không-lọc-gì.
    [RelayCommand]
    private async Task LoadReportPanelAsync(CancellationToken ct = default)
    {
        IsReportLoading    = true;
        HasReportError     = false;
        ReportErrorMessage = string.Empty;
        try
        {
            var selectedProductIds = ProductItems.Where(p => p.IsSelected).Select(p => p.Id).ToList();
            var productIds = selectedProductIds.Count > 0 ? selectedProductIds : null;

            var lines = await _getSummaryReport.ExecuteAsync(
                productIds,
                SelectedEmployee?.Id,
                SelectedCustomer?.Id,
                IsAnyOption(SelectedUnit) ? null : SelectedUnit,
                IsAnyOption(SelectedCategory) ? null : SelectedCategory,
                FromDate, ToDate, ct);

            var rows = lines
                .GroupBy(l => l.ProductId)
                .Select(g => new ProductSalesSummaryRow
                {
                    ProductId      = g.Key,
                    ProductCode    = g.First().ProductCode,
                    ProductName    = g.First().ProductName,
                    Unit           = g.First().Unit,
                    QuantitySold   = g.Sum(l => l.QuantitySold),
                    SalesAmount    = g.Sum(l => l.SalesAmount),
                    DiscountAmount = g.Sum(l => l.DiscountAmount),
                    ReturnQuantity = g.Sum(l => l.ReturnQuantity),
                    ReturnValue    = g.Sum(l => l.ReturnValue),
                    NetRevenue     = g.Sum(l => l.NetRevenue),
                })
                .OrderByDescending(r => r.NetRevenue);

            ReportRows.Clear();
            foreach (var row in rows) ReportRows.Add(row);
            HasReportRows = ReportRows.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasReportError     = true;
            ReportErrorMessage = $"Không thể tải báo cáo: {ex.Message}";
        }
        finally { IsReportLoading = false; }
    }

    // Double-click 1 dòng trong bảng kết quả → mở "Sổ chi tiết bán hàng" (SalesOrderReportDetailView)
    // lọc đúng theo Mặt hàng của dòng đó — kế thừa nguyên Nhân viên/Khách hàng/ĐVT/Nhóm VTHH/khoảng
    // ngày đang lọc trên panel này (2026-08-22, tái dùng hạ tầng SalesOrderDetailFilter/ReportDetail
    // route đã có sẵn từ trang báo cáo cũ SalesOrderReportView — trang đó không còn ai gọi tới, chỉ
    // còn màn Sổ chi tiết là điểm đến, không phải điểm xuất phát).
    [RelayCommand]
    private void DrillDownReportRow(ProductSalesSummaryRow? row)
    {
        if (row is null) return;

        var filter = new SalesOrderDetailFilter
        {
            Title      = $"Mặt hàng: {row.ProductCode} — {row.ProductName}",
            ProductId  = row.ProductId,
            EmployeeId = SelectedEmployee?.Id,
            CustomerId = SelectedCustomer?.Id,
            Unit       = IsAnyOption(SelectedUnit) ? null : SelectedUnit,
            Category   = IsAnyOption(SelectedCategory) ? null : SelectedCategory,
            FromDate   = FromDate,
            ToDate     = ToDate,
        };
        _navigationService.NavigateTo(NavigationRoutes.SalesOrders.ReportDetail, filter);
    }
}
