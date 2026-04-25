// ProductListViewModel.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;

namespace DesktopLamour.Features.HomePage.ProductList.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public ProductListViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();
}
