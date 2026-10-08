// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Input;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class TongHopTonKhoView : System.Windows.Controls.UserControl
{
    private readonly TongHopTonKhoViewModel _viewModel;

    public TongHopTonKhoView(TongHopTonKhoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = viewModel;
        Loaded += async (_, _) =>
        {
            // Hộp "Chọn tham số" hiện ngay khi vào màn; Đồng ý mới nạp báo cáo (xem StartAsync).
            await viewModel.StartCommand.ExecuteAsync(null);
        };
    }

    private void SummaryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SummaryDataGrid.SelectedItem is InventorySummaryItem item && _viewModel.DrillDownCommand.CanExecute(item))
            _viewModel.DrillDownCommand.Execute(item);
    }
}
