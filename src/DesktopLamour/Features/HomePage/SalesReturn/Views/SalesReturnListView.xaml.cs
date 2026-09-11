// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

namespace DesktopLamour.Features.HomePage.SalesReturn.Views;

public partial class SalesReturnListView : UserControl
{
    private SalesReturnListViewModel ViewModel => (SalesReturnListViewModel)DataContext;

    public SalesReturnListView(SalesReturnListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    // Double-click mở Sửa; fallback qua Xem nếu không có dòng nào được chọn.
    private async void ReturnsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.EditSalesReturnCommand.CanExecute(null))
            await ViewModel.EditSalesReturnCommand.ExecuteAsync(null);
        else if (ViewModel.ViewSalesReturnCommand.CanExecute(null))
            await ViewModel.ViewSalesReturnCommand.ExecuteAsync(null);
    }

    // 2026-09-11: mirror SalesOrderListView.OrdersGrid_PreviewMouseRightButtonDown — WPF DataGrid
    // không tự chọn dòng khi chuột phải, chỉ ContextMenu (Thêm/Xem/Sửa/Xóa/Bỏ ghi) mới cần dòng
    // đúng dưới con trỏ thay vì dòng đang chọn cũ. Dò DataGridRow chứa điểm bấm rồi tự select.
    private void ReturnsGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source) return;

        var row = FindAncestor<DataGridRow>(source);
        if (row?.Item is not null)
            ReturnsGrid.SelectedItem = row.Item;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
