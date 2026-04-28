// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public WarehouseViewModel(INavigationService navigationService)
        => _navigationService = navigationService;

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void NavigateToHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private void NavigateToTongHopTonKho()
        => _navigationService.NavigateTo(NavigationRoutes.Warehouse.TongHopTonKho);

    [RelayCommand]
    private void NavigateToPhieuNhapKho()
        => _navigationService.NavigateTo(NavigationRoutes.Warehouse.PhieuNhapKho);
}
