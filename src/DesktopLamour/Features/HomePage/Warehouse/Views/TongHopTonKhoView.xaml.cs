// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class TongHopTonKhoView : System.Windows.Controls.UserControl
{
    public TongHopTonKhoView(TongHopTonKhoViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) =>
        {
            await viewModel.InitializeFiltersCommand.ExecuteAsync(null);
            await viewModel.LoadCommand.ExecuteAsync(null);
        };
    }
}
