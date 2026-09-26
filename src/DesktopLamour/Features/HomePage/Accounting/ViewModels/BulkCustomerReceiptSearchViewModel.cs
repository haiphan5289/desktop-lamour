// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

// Popup 1/2 của "Phiếu Thu Hàng Loạt" — tìm chứng từ bán hàng còn nợ khớp filter, tick chọn nhiều
// dòng (có thể nhiều khách hàng khác nhau). 2026-09-26 (so ảnh mẫu MISA, review lại toàn bộ luồng):
// popup này giờ CHỈ LÀ BỘ CHỌN — bấm "✔ Thu tiền" không tự tạo/mở phiếu nữa, chỉ đóng lại và trả
// phần đã chọn (Items đã tick + PaymentMethod/BankAccount/SelectedEmployee/CollectionDate) cho nơi
// gọi (BulkCustomerReceiptViewModel.AddNewAsync) tự dựng dòng và lưu — mirror cách các popup
// "chọn rồi trả kết quả" khác trong app không tự sinh side-effect ngay trong popup con.
public partial class BulkCustomerReceiptSearchViewModel : ViewModelBase
{
    // true = đóng do bấm "✔ Thu tiền" hợp lệ (code-behind set DialogResult=true); false = "Hủy bỏ".
    public event Action<bool>? RequestClose;

    private readonly IGetOutstandingSalesOrdersUseCase _getOutstanding;
    private readonly IGetEmployeesUseCase               _getEmployees;
    // "Số chứng từ" trên lưới hiện dạng link bấm được (khớp ảnh mẫu MISA) — mở lại đúng hóa đơn gốc,
    // chỉ xem. Cùng cách BulkCustomerReceiptViewModel.OpenSalesOrderAsync đã làm cho "Tham chiếu".
    private readonly IGetSalesOrderByIdUseCase _getSalesOrderById;
    private readonly Func<SalesOrderWindow>    _salesOrderWindowFactory;
    private readonly ILogger<BulkCustomerReceiptSearchViewModel> _logger;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;
    [ObservableProperty] private string _lineSummary = "Số dòng = 0";

    // "Cash111" (Tiền mặt) hoặc "Bank112" (Tiền gửi) — khớp thẳng giá trị AccountCode enum, áp dụng
    // cho toàn bộ phiếu sẽ tạo ở popup xác nhận. Radio button bind qua StringEqualityConverter.
    [ObservableProperty] private string  _paymentMethod = "Cash111";
    [ObservableProperty] private string? _bankAccount;

    // 2026-09-16: đổi lại mặc định "Hôm nay" (đảo ngược quyết định 2026-08-31 "Đầu tháng đến hiện
    // tại" — vốn dĩ chính là mặc định gốc trước đó). Nhãn "Đầu tháng đến hiện tại" vẫn còn trong
    // PeriodOptions để chọn tay khi cần.
    public static string[] PeriodOptions { get; } = { "Hôm nay", "Hôm qua", "Tuần này", "Đầu tháng đến hiện tại", "Tùy chọn" };
    [ObservableProperty] private string   _selectedPeriod = "Hôm nay";
    [ObservableProperty] private DateTime _fromDate = DateTime.Today;
    [ObservableProperty] private DateTime _toDate   = DateTime.Today;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    // 2026-09-26 (khớp ảnh mẫu MISA "Ngày thu tiền") — chọn 1 lần ở đây, đổ thẳng vào Ngày hạch toán
    // + Ngày chứng từ của phiếu tạo ra ở BulkCustomerReceiptViewModel.AddNewAsync, không cần gõ lại.
    [ObservableProperty] private DateTime _collectionDate = DateTime.Today;

    [ObservableProperty] private bool _areAllSelected;

    // 2026-09-26 (khớp ảnh mẫu MISA "Số tiền") — tổng số tiền các dòng ĐANG TICK, cập nhật sống mỗi
    // khi tick/bỏ tick, KHÔNG phải tổng toàn bộ Items như LineSummary/"Số dòng".
    [ObservableProperty] private decimal _selectedTotal;

    // 2026-09-26: chính popup này (BulkCustomerReceiptSearchWindow tự gán) — hóa đơn mở từ link Số
    // chứng từ căn giữa popup thay vì cửa sổ chính.
    public Window? HostWindow { get; set; }

    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<OutstandingSalesOrderCheckItem> Items { get; } = new();

    public BulkCustomerReceiptSearchViewModel(
        IGetOutstandingSalesOrdersUseCase getOutstanding,
        IGetEmployeesUseCase              getEmployees,
        IGetSalesOrderByIdUseCase         getSalesOrderById,
        Func<SalesOrderWindow>            salesOrderWindowFactory,
        ILogger<BulkCustomerReceiptSearchViewModel> logger)
    {
        _getOutstanding          = getOutstanding;
        _getEmployees            = getEmployees;
        _getSalesOrderById       = getSalesOrderById;
        _salesOrderWindowFactory = salesOrderWindowFactory;
        _logger                  = logger;
    }

    // "Số chứng từ" trên lưới bấm được — mở lại đúng hóa đơn gốc (chỉ xem), không đụng lựa chọn
    // (IsSelected) đang tick — giống hệt BulkCustomerReceiptViewModel.OpenSalesOrderAsync.
    [RelayCommand]
    private async Task OpenSalesOrderAsync(int salesOrderId, CancellationToken ct = default)
    {
        try
        {
            var order = await _getSalesOrderById.ExecuteAsync(salesOrderId, ct);
            if (order is null)
            {
                MessageBox.Show("Không tìm thấy chứng từ bán hàng này (có thể đã bị xóa).", "Không tìm thấy",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = _salesOrderWindowFactory();
            window.Owner = HostWindow ?? Application.Current.MainWindow;
            window.Initialize(order, isReadOnly: true);
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not open sales order {Id} from outstanding-orders grid", salesOrderId);
            MessageBox.Show(ex.Message, "Không thể mở chứng từ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
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
            case "Đầu tháng đến hiện tại":
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
        // Mỗi item.IsSelected = value ở trên đã tự bắn RecalculateSelectedTotal qua PropertyChanged
        // handler gắn trong LoadAsync, nhưng gọi lại 1 lần cho chắc (vd. Items rỗng thì vòng for
        // không chạy lần nào, SelectedTotal có thể còn stale từ trước).
        RecalculateSelectedTotal();
    }

    private void RecalculateSelectedTotal() =>
        SelectedTotal = Items.Where(i => i.IsSelected).Sum(i => i.RemainingAmount);

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
            foreach (var order in data)
            {
                var item = new OutstandingSalesOrderCheckItem(order);
                item.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(OutstandingSalesOrderCheckItem.IsSelected))
                        RecalculateSelectedTotal();
                };
                Items.Add(item);
            }
            HasItems    = Items.Count > 0;
            LineSummary = $"Số dòng = {Items.Count}";
            RecalculateSelectedTotal();
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

    // Trả về đúng danh sách đang tick — dùng ngay khi ĐANG mở (trước khi đóng), nên không cần lưu
    // snapshot riêng: BulkCustomerReceiptViewModel đọc property này SAU KHI ShowDialog() trả về true.
    public IReadOnlyList<OutstandingSalesOrderCheckItem> SelectedItems { get; private set; } = Array.Empty<OutstandingSalesOrderCheckItem>();

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

        SelectedItems = selected;
        RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(false);
}
