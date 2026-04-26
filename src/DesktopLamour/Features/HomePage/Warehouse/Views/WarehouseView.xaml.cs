// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class WarehouseView : System.Windows.Controls.UserControl
{
    public WarehouseView(WarehouseViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
