// DataGridRowDoubleClickBehavior.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopLamour.Shared.Helpers;

namespace DesktopLamour.Shared.Behaviors;

// Nhấp đúp 1 dòng dữ liệu của DataGrid → chạy Command (vd. EditXxxCommand, mở form Sửa như MISA).
// Chỉ kích hoạt khi nhấp trúng DataGridRow — nhấp đúp vào tiêu đề cột / ô lọc nhúng trong header /
// vùng trống dưới lưới không mở gì. Dòng đã được chọn từ lần nhấp đầu của cú nhấp đúp nên
// CanExecute dựa vào SelectedItem (HasSelection) vẫn đúng.
public static class DataGridRowDoubleClickBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(DataGridRowDoubleClickBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject obj)
        => (ICommand?)obj.GetValue(CommandProperty);

    public static void SetCommand(DependencyObject obj, ICommand? value)
        => obj.SetValue(CommandProperty, value);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.MouseDoubleClick -= OnMouseDoubleClick;
        if (e.NewValue is not null) grid.MouseDoubleClick += OnMouseDoubleClick;
    }

    private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid) return;
        if (e.ChangedButton != MouseButton.Left) return;

        var row = TreeWalk.FindAncestorOrSelf<DataGridRow>(e.OriginalSource as DependencyObject);
        if (row is null) return;

        var command = GetCommand(grid);
        if (command is null || !command.CanExecute(null)) return;

        command.Execute(null);
        e.Handled = true;
    }
}
