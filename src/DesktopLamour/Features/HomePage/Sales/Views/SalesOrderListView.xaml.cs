// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderListView : UserControl
{
    private SalesOrderListViewModel ViewModel => (SalesOrderListViewModel)DataContext;

    public SalesOrderListView(SalesOrderListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadSalesOrdersCommand.ExecuteAsync(null);
    }

    private async void OrdersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.EditSalesOrderCommand.CanExecute(null))
            await ViewModel.EditSalesOrderCommand.ExecuteAsync(null);
    }

    // WPF DataGrid không tự chọn dòng khi chuột phải (chỉ SelectedItem theo lần click TRÁI gần nhất) —
    // không select trước thì ContextMenu (Thêm/Nhân bản/Xem/Xóa/Gửi email,Zalo) sẽ thao tác nhầm lên
    // dòng đang chọn cũ thay vì dòng vừa bấm chuột phải. Dò DataGridRow chứa điểm bấm rồi tự select.
    private void OrdersGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source) return;

        var row = FindAncestor<DataGridRow>(source);
        if (row?.Item is not null)
            OrdersGrid.SelectedItem = row.Item;
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
