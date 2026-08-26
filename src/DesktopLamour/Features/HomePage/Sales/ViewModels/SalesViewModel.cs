// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Deposits.Views;
using DesktopLamour.Features.HomePage.Sales.Views;

namespace DesktopLamour.Features.HomePage.Sales.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly Func<DepositWindow> _depositWindowFactory;
    private readonly Func<SalesOrderReportFilterWindow> _reportFilterWindowFactory;

    public SalesViewModel(
        INavigationService navigationService,
        Func<DepositWindow> depositWindowFactory,
        Func<SalesOrderReportFilterWindow> reportFilterWindowFactory)
    {
        _navigationService         = navigationService;
        _depositWindowFactory      = depositWindowFactory;
        _reportFilterWindowFactory = reportFilterWindowFactory;
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

    // Tile "📊 Báo cáo" — mở trang báo cáo tổng hợp riêng (SalesOrderReportView, hỗ trợ 7 kiểu
    // "Thống kê theo" + group/subtotal + In/Xuất Excel/Email/Zalo), giống "Phiếu cọc" mở DepositWindow.
    // 2026-08-26: đây lại là cách DUY NHẤT xem báo cáo bán hàng — panel "Báo cáo theo Mặt hàng" nhúng
    // thẳng trên màn này (2026-08-22–2026-08-26) đã bị gỡ bỏ hoàn toàn theo yêu cầu, vì trùng lặp
    // chức năng với tile này sau khi tile được khôi phục lại.
    [RelayCommand]
    private void OpenReport()
    {
        var window = _reportFilterWindowFactory();
        if (window.ShowDialog() == true)
        {
            var filter = window.BuildFilter();
            _navigationService.NavigateTo(NavigationRoutes.SalesOrders.Report, filter);
        }
    }
}
