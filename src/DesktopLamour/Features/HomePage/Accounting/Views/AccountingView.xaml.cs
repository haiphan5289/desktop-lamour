// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using System.Windows;
using System.Windows.Input;
using DesktopLamour.Shared.Helpers;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class AccountingView : System.Windows.Controls.UserControl
{
    public AccountingView(AccountingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AccountingViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    // "➕ Thêm ▾" — mở menu chọn loại phiếu ngay dưới nút. Gán PlacementTarget/DataContext thủ công vì
    // ContextMenu mở bằng code (IsOpen) không tự nhận DataContext như khi mở bằng chuột phải.
    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement button || button.ContextMenu is not { } menu) return;
        menu.PlacementTarget = button;
        menu.Placement       = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        menu.DataContext     = button.DataContext;
        menu.IsOpen          = true;
    }

    // Chuột phải chọn luôn dòng đang trỏ trước khi menu mở — khớp SalesOrderListView.
    private void LedgerGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var row = TreeWalk.FindAncestorOrSelf<System.Windows.Controls.DataGridRow>(e.OriginalSource as DependencyObject);
        if (row?.Item is { } item)
            LedgerGrid.SelectedItem = item;
    }

    private void LedgerGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is AccountingViewModel vm && vm.ViewEntryCommand.CanExecute(null))
            vm.ViewEntryCommand.Execute(null);
    }
}
