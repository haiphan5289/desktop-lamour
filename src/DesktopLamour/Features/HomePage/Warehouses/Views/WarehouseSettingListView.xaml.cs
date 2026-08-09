// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Warehouses.Views;

public partial class WarehouseSettingListView : UserControl
{
    public WarehouseSettingListView(WarehouseSettingListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadWarehousesCommand.ExecuteAsync(null);
    }
}
