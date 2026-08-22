// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

// Popup 1/2 của "Phiếu Thu Hàng Loạt" — tìm chứng từ bán hàng còn nợ khớp filter, tick chọn nhiều
// dòng (có thể nhiều khách hàng khác nhau), bấm "Thu tiền" mở popup xác nhận (BulkCustomerReceiptWindow)
// để sửa số tiền từng dòng rồi Cất.
public partial class BulkCustomerReceiptSearchViewModel : ViewModelBase
{
    public event Action? RequestClose;

    private readonly IGetOutstandingSalesOrdersUseCase _getOutstanding;
    private readonly IGetEmployeesUseCase               _getEmployees;
    private readonly Func<BulkCustomerReceiptWindow>    _confirmWindowFactory;
    private readonly ILogger<BulkCustomerReceiptSearchViewModel> _logger;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;

    // "Cash111" (Tiền mặt) hoặc "Bank112" (Tiền gửi) — khớp thẳng giá trị AccountCode enum, áp dụng
    // cho toàn bộ phiếu sẽ tạo ở popup xác nhận. Radio button bind qua StringEqualityConverter.
    [ObservableProperty] private string  _paymentMethod = "Cash111";
    [ObservableProperty] private string? _bankAccount;

    public static string[] PeriodOptions { get; } = { "Hôm nay", "Hôm qua", "Tuần này", "Tháng này", "Tùy chọn" };
    [ObservableProperty] private string   _selectedPeriod = "Hôm nay";
    [ObservableProperty] private DateTime _fromDate = DateTime.Today;
    [ObservableProperty] private DateTime _toDate   = DateTime.Today;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    [ObservableProperty] private bool _areAllSelected;

    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<OutstandingSalesOrderCheckItem> Items { get; } = new();

    public BulkCustomerReceiptSearchViewModel(
        IGetOutstandingSalesOrdersUseCase getOutstanding,
        IGetEmployeesUseCase              getEmployees,
        Func<BulkCustomerReceiptWindow>   confirmWindowFactory,
        ILogger<BulkCustomerReceiptSearchViewModel> logger)
    {
        _getOutstanding        = getOutstanding;
        _getEmployees          = getEmployees;
        _confirmWindowFactory  = confirmWindowFactory;
        _logger                = logger;
    }

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            var employees = await _getEmployees.ExecuteAsync(ct);
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload employees for BulkCustomerReceiptSearch");
        }

        await LoadAsync(ct);
    }

    // Preset chỉ tính lại From/To client-side, không tự gọi BE — người dùng vẫn bấm "Lấy dữ liệu".
    partial void OnSelectedPeriodChanged(string value)
    {
        var today = DateTime.Today;
        switch (value)
        {
            case "Hôm nay":
                FromDate = today; ToDate = today;
                break;
            case "Hôm qua":
                FromDate = today.AddDays(-1); ToDate = today.AddDays(-1);
                break;
            case "Tuần này":
                FromDate = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                ToDate   = today;
                break;
            case "Tháng này":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate   = today;
                break;
            case "Tùy chọn":
            default:
                break;
        }
    }

    partial void OnAreAllSelectedChanged(bool value)
    {
        foreach (var item in Items) item.IsSelected = value;
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var from = DateOnly.FromDateTime(FromDate);
            var to   = DateOnly.FromDateTime(ToDate);
            var data = await _getOutstanding.ExecuteAsync(from, to, SelectedEmployee?.Id, ct);

            AreAllSelected = false;
            Items.Clear();
            foreach (var order in data) Items.Add(new OutstandingSalesOrderCheckItem(order));
            HasItems = Items.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load outstanding sales orders");
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Collect()
    {
        var selected = Items.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn ít nhất 1 chứng từ để thu tiền.", "Chưa chọn chứng từ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var window = _confirmWindowFactory();
        window.Owner = Application.Current.MainWindow;
        window.Initialize(
            selected,
            debitAccount: PaymentMethod,
            bankAccount: PaymentMethod == "Bank112" ? BankAccount : null,
            collectorEmployeeId: SelectedEmployee?.Id);

        if (window.ShowDialog() == true)
            _ = LoadAsync(CancellationToken.None);
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke();
}
