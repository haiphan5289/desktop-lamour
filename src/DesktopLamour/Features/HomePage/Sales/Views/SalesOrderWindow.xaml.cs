// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Sales.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderWindow : Window
{
    public SalesOrderViewModel ViewModel { get; }

    private SalesOrderResponseDto? _initialOrder;

    public SalesOrderWindow(SalesOrderViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };
    }

    public void Initialize(SalesOrderResponseDto? order)
        => _initialOrder = order;

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.InitializeAsync(_initialOrder);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
