// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Controls;
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
        // Tiêu đề bảng (HeaderGrid) vẽ riêng phía trên DataGrid — kéo thanh cuộn ngang của lưới thì dịch
        // tiêu đề theo để cột và tiêu đề luôn thẳng hàng.
        SummaryDataGrid.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnGridScrollChanged));
        Loaded += async (_, _) =>
        {
            // Hộp "Chọn tham số" hiện ngay khi vào màn; Đồng ý mới nạp báo cáo (xem StartAsync).
            await viewModel.StartCommand.ExecuteAsync(null);
        };
    }

    private void OnGridScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange != 0)
            HeaderScroll.X = -e.HorizontalOffset;
    }

    private void SummaryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SummaryDataGrid.SelectedItem is InventorySummaryItem item && _viewModel.DrillDownCommand.CanExecute(item))
            _viewModel.DrillDownCommand.Execute(item);
    }
}
