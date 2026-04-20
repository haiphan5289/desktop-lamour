// SupplierListViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.ViewModels;

public partial class SupplierListViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public SupplierListViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();
}
