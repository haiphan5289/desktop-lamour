// DataGridRowNavigationBehavior.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLamour.Shared.Behaviors;

// WPF DataGrid blocks arrow-key navigation while a cell is in edit mode — attach this to
// reimplement Excel-style movement: ↓/↑ commits and jumps to the same column on the next/
// previous row; ←/→ commits and jumps to the previous/next column once the caret is already
// at that edge of the text (so the caret can still move freely within a cell's text).
public static class DataGridRowNavigationBehavior
{
    public static readonly DependencyProperty EnableArrowRowNavigationProperty =
        DependencyProperty.RegisterAttached(
            "EnableArrowRowNavigation",
            typeof(bool),
            typeof(DataGridRowNavigationBehavior),
            new PropertyMetadata(false, OnEnableArrowRowNavigationChanged));

    public static bool GetEnableArrowRowNavigation(DependencyObject obj)
        => (bool)obj.GetValue(EnableArrowRowNavigationProperty);

    public static void SetEnableArrowRowNavigation(DependencyObject obj, bool value)
        => obj.SetValue(EnableArrowRowNavigationProperty, value);

    private static void OnEnableArrowRowNavigationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.PreviewKeyDown -= OnPreviewKeyDown;
        if ((bool)e.NewValue) grid.PreviewKeyDown += OnPreviewKeyDown;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid grid) return;
        if (e.Key is not (Key.Down or Key.Up or Key.Left or Key.Right)) return;

        var currentColumn = grid.CurrentCell.Column;
        if (currentColumn is null) return;

        // Let combo-box columns (product/account pickers) keep arrow keys for their own dropdown/text navigation.
        if (Keyboard.FocusedElement is DependencyObject focused && FindParent<ComboBox>(focused) is not null)
            return;

        var isHorizontal = e.Key is Key.Left or Key.Right;
        if (isHorizontal && Keyboard.FocusedElement is TextBox textBox)
        {
            // Caret not yet at the edge — let it move within the cell's text instead of jumping columns.
            var atLeftEdge  = textBox.SelectionLength == 0 && textBox.CaretIndex == 0;
            var atRightEdge = textBox.SelectionLength == 0 && textBox.CaretIndex == textBox.Text.Length;
            if (e.Key == Key.Left && !atLeftEdge) return;
            if (e.Key == Key.Right && !atRightEdge) return;
        }

        var rowIndex = grid.Items.IndexOf(grid.CurrentCell.Item);
        if (rowIndex < 0) return;

        var targetRowIndex = rowIndex;
        DataGridColumn targetColumn;

        if (isHorizontal)
        {
            var orderedColumns = grid.Columns.OrderBy(c => c.DisplayIndex).ToList();
            var currentIndex = orderedColumns.IndexOf(currentColumn);
            var targetIndex = currentIndex + (e.Key == Key.Right ? 1 : -1);
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= orderedColumns.Count) return;
            targetColumn = orderedColumns[targetIndex];
        }
        else
        {
            targetRowIndex = rowIndex + (e.Key == Key.Down ? 1 : -1);
            if (targetRowIndex < 0 || targetRowIndex >= grid.Items.Count) return;
            targetColumn = currentColumn;
        }

        grid.CommitEdit(DataGridEditingUnit.Cell, true);
        grid.CommitEdit(DataGridEditingUnit.Row, true);

        var targetItem = grid.Items[targetRowIndex];
        grid.CurrentCell = new DataGridCellInfo(targetItem, targetColumn);
        grid.SelectedItem = targetItem;
        grid.ScrollIntoView(targetItem, targetColumn);
        grid.BeginEdit();

        e.Handled = true;
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent is null) return null;
        return parent is T p ? p : FindParent<T>(parent);
    }
}
