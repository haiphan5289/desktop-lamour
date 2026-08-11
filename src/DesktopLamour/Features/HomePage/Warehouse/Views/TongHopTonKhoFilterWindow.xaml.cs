// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class TongHopTonKhoFilterWindow : Window
{
    public TongHopTonKhoFilterViewModel ViewModel { get; }

    public TongHopTonKhoFilterWindow(TongHopTonKhoFilterViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => DialogResult = result;
    }

    public async Task InitializeAsync(InventoryFilter current, CancellationToken ct = default)
        => await ViewModel.InitializeAsync(current, ct);

    public InventoryFilter Result => ViewModel.Result;
}
