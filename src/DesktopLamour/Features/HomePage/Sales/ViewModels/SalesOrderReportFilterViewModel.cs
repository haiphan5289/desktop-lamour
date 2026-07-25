// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesOrderReportFilterViewModel : ViewModelBase
{
    private const string AllOption = "Tất cả";

    private readonly IGetProductsUseCase  _getProducts;
    private readonly IGetEmployeesUseCase _getEmployees;
    private readonly IGetCustomersUseCase _getCustomers;

    private List<Product> _allProducts = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _dialogResult;

    [ObservableProperty] private ISearchableItem? _selectedEmployee;
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private string?          _selectedUnit;
    [ObservableProperty] private string?          _selectedCategory;
    [ObservableProperty] private string           _selectedPeriod = SalesOrderReportPeriods.MonthToDate;
    [ObservableProperty] private string           _selectedReportType = SalesOrderReportTypes.ByProduct;
    [ObservableProperty] private bool             _areAllProductsSelected;

    [ObservableProperty] private DateTime? _fromDate;
    [ObservableProperty] private DateTime? _toDate;

    public ObservableCollection<ISearchableItem>  Employees    { get; } = new();
    public ObservableCollection<ISearchableItem>  Customers    { get; } = new();
    public ObservableCollection<string>           Units        { get; } = new();
    public ObservableCollection<string>           Categories   { get; } = new();
    public ObservableCollection<ProductCheckItem> ProductItems { get; } = new();
    public IReadOnlyList<string>                  Periods      { get; } = SalesOrderReportPeriods.All;
    public IReadOnlyList<string>                  ReportTypes  { get; } = SalesOrderReportTypes.All;

    public SalesOrderReportFilterViewModel(
        IGetProductsUseCase  getProducts,
        IGetEmployeesUseCase getEmployees,
        IGetCustomersUseCase getCustomers)
    {
        _getProducts  = getProducts;
        _getEmployees = getEmployees;
        _getCustomers = getCustomers;

        ApplyPeriodPreset(SelectedPeriod);
    }

    [RelayCommand]
    private async Task LoadLookupsAsync(CancellationToken ct = default)
    {
        IsLoading = true;
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
                Categories.Add(c);
            SelectedCategory ??= AllOption;

            ApplyProductFilter();
        }
        catch (OperationCanceledException) { }
        finally { IsLoading = false; }
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
    private void Submit() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedEmployee    = null;
        SelectedCustomer    = null;
        SelectedUnit        = AllOption;
        SelectedCategory    = AllOption;
        SelectedReportType  = SalesOrderReportTypes.ByProduct;
        SelectedPeriod      = SalesOrderReportPeriods.Custom;
        FromDate = null;
        ToDate   = null;
        foreach (var item in ProductItems) item.IsSelected = false;
    }

    public SalesOrderReportFilter BuildFilter()
    {
        var selected = ProductItems.Where(p => p.IsSelected).ToList();
        return new SalesOrderReportFilter
        {
            ReportType    = SelectedReportType,
            ProductIds    = selected.Select(p => p.Id).ToList(),
            ProductLabels = selected.Select(p => $"{p.Code} — {p.Name}").ToList(),
            EmployeeId    = SelectedEmployee?.Id,
            EmployeeLabel = SelectedEmployee?.DisplayText,
            CustomerId    = SelectedCustomer?.Id,
            CustomerLabel = SelectedCustomer?.DisplayText,
            Unit          = IsAnyOption(SelectedUnit) ? null : SelectedUnit,
            Category      = IsAnyOption(SelectedCategory) ? null : SelectedCategory,
            FromDate      = FromDate,
            ToDate        = ToDate,
        };
    }
}
