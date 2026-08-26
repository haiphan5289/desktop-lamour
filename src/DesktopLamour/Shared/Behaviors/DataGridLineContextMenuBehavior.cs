// DataGridLineContextMenuBehavior.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DesktopLamour.Shared.Behaviors;

// Right-click context menu + shortcuts cho grid dòng hàng (Sản phẩm) trên các form chứng từ
// (SalesOrderWindow/SalesReturnWindow/WarehouseReceiptFormWindow) — theo mẫu UI tham chiếu MISA.
// "Thêm dòng"/"Xóa dòng" gọi thẳng AddLineCommand/RemoveLineCommand đã có sẵn trên ViewModel qua
// reflection (tên property giống nhau ở cả 3 ViewModel) — không cần ViewModel implement interface
// chung, tránh phải sửa cả 3 ViewModel chỉ để thêm 1 marker interface.
public static class DataGridLineContextMenuBehavior
{
    public static readonly DependencyProperty EnableLineContextMenuProperty =
        DependencyProperty.RegisterAttached(
            "EnableLineContextMenu",
            typeof(bool),
            typeof(DataGridLineContextMenuBehavior),
            new PropertyMetadata(false, OnEnableLineContextMenuChanged));

    public static bool GetEnableLineContextMenu(DependencyObject obj)
        => (bool)obj.GetValue(EnableLineContextMenuProperty);

    public static void SetEnableLineContextMenu(DependencyObject obj, bool value)
        => obj.SetValue(EnableLineContextMenuProperty, value);

    // Cho phép form không phải "dòng sản phẩm" (VD Payment — dòng hạch toán, không có
    // Product/ProductId/tồn kho) tái dùng behavior này: đổi tên command Thêm/Xóa dòng và
    // tắt hẳn mục "Xem số tồn vật tư" (không có khái niệm sản phẩm để tra). Mặc định giữ
    // nguyên hành vi cũ (AddLineCommand/RemoveLineCommand, có mục Xem số tồn vật tư) cho
    // SalesOrder/SalesReturn/WarehouseReceipt.
    public static readonly DependencyProperty AddCommandNameProperty =
        DependencyProperty.RegisterAttached(
            "AddCommandName", typeof(string), typeof(DataGridLineContextMenuBehavior),
            new PropertyMetadata("AddLineCommand"));

    public static string GetAddCommandName(DependencyObject obj) => (string)obj.GetValue(AddCommandNameProperty);
    public static void SetAddCommandName(DependencyObject obj, string value) => obj.SetValue(AddCommandNameProperty, value);

    public static readonly DependencyProperty RemoveCommandNameProperty =
        DependencyProperty.RegisterAttached(
            "RemoveCommandName", typeof(string), typeof(DataGridLineContextMenuBehavior),
            new PropertyMetadata("RemoveLineCommand"));

    public static string GetRemoveCommandName(DependencyObject obj) => (string)obj.GetValue(RemoveCommandNameProperty);
    public static void SetRemoveCommandName(DependencyObject obj, string value) => obj.SetValue(RemoveCommandNameProperty, value);

    public static readonly DependencyProperty ShowProductStockMenuItemProperty =
        DependencyProperty.RegisterAttached(
            "ShowProductStockMenuItem", typeof(bool), typeof(DataGridLineContextMenuBehavior),
            new PropertyMetadata(true));

    public static bool GetShowProductStockMenuItem(DependencyObject obj) => (bool)obj.GetValue(ShowProductStockMenuItemProperty);
    public static void SetShowProductStockMenuItem(DependencyObject obj, bool value) => obj.SetValue(ShowProductStockMenuItemProperty, value);

    // Lưu vị trí tìm kiếm gần nhất mỗi grid, để Ctrl+F lặp lại thì nhảy tới kết quả tiếp theo
    // thay vì luôn quay lại kết quả đầu tiên. ConditionalWeakTable (thay vì Dictionary thường)
    // để không giữ DataGrid/Window sống mãi sau khi user đóng form — các form này mở/đóng lặp
    // đi lặp lại nhiều lần trong 1 phiên làm việc (SalesOrder/SalesReturn/WarehouseReceipt).
    private static readonly ConditionalWeakTable<DataGrid, SearchState> _lastSearch = new();

    private sealed class SearchState
    {
        public string Text { get; set; } = "";
        public int LastIndex { get; set; }
    }

    private static void OnEnableLineContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.PreviewKeyDown -= OnPreviewKeyDown;
        if (!(bool)e.NewValue) return;

        grid.PreviewKeyDown += OnPreviewKeyDown;
        grid.ContextMenu = BuildContextMenu(grid);
    }

    private static ContextMenu BuildContextMenu(DataGrid grid)
    {
        var menu = new ContextMenu();

        menu.Items.Add(MenuItem("Thêm dòng", "Ctrl+Insert", (_, _) => AddLine(grid)));
        menu.Items.Add(MenuItem("Xóa dòng", "Ctrl+Delete", (_, _) => RemoveLine(grid)));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem("Sao chép dữ liệu cho các dòng phía dưới", null, (_, _) => CopyValueDown(grid)));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem("Tìm kiếm...", "Ctrl+F", (_, _) => ShowFindDialog(grid)));

        if (GetShowProductStockMenuItem(grid))
            menu.Items.Add(MenuItem("Xem số tồn vật tư...", "Ctrl+F2", (_, _) => ShowProductStock(grid)));

        return menu;
    }

    private static MenuItem MenuItem(string header, string? gestureText, RoutedEventHandler click)
    {
        var item = new MenuItem { Header = header, InputGestureText = gestureText };
        item.Click += click;
        return item;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not DataGrid grid) return;
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;

        switch (e.Key)
        {
            case Key.Insert:
                AddLine(grid);
                e.Handled = true;
                break;
            case Key.Delete:
                RemoveLine(grid);
                e.Handled = true;
                break;
            case Key.F:
                ShowFindDialog(grid);
                e.Handled = true;
                break;
            case Key.F2 when GetShowProductStockMenuItem(grid):
                ShowProductStock(grid);
                e.Handled = true;
                break;
        }
    }

    // ── Thêm dòng / Xóa dòng — ủy quyền cho AddLineCommand/RemoveLineCommand đã có sẵn trên ViewModel ──

    private static void AddLine(DataGrid grid)
    {
        var command = GetCommand(grid.DataContext, GetAddCommandName(grid));
        if (command?.CanExecute(null) == true) command.Execute(null);
    }

    private static void RemoveLine(DataGrid grid)
    {
        var line = grid.CurrentCell.Item ?? grid.SelectedItem;
        if (line is null) return;

        var command = GetCommand(grid.DataContext, GetRemoveCommandName(grid));
        if (command?.CanExecute(line) == true) command.Execute(line);
    }

    private static ICommand? GetCommand(object? viewModel, string propertyName)
        => viewModel?.GetType().GetProperty(propertyName)?.GetValue(viewModel) as ICommand;

    // ── Sao chép dữ liệu cho các dòng phía dưới ──────────────────────────────────────────────

    private static void CopyValueDown(DataGrid grid)
    {
        grid.CommitEdit(DataGridEditingUnit.Cell, true);
        grid.CommitEdit(DataGridEditingUnit.Row, true);

        var cell = grid.CurrentCell;
        if (cell.Item is null || cell.Column is not DataGridBoundColumn boundColumn) return;
        if (boundColumn.Binding is not Binding binding || string.IsNullOrEmpty(binding.Path?.Path)) return;

        var propertyPath = binding.Path.Path;
        var sourceProperty = cell.Item.GetType().GetProperty(propertyPath);
        if (sourceProperty is null || !sourceProperty.CanWrite) return;

        var value = sourceProperty.GetValue(cell.Item);
        var startIndex = grid.Items.IndexOf(cell.Item);
        if (startIndex < 0) return;

        for (var i = startIndex + 1; i < grid.Items.Count; i++)
        {
            var targetItem = grid.Items[i];
            var targetProperty = targetItem.GetType().GetProperty(propertyPath);
            if (targetProperty is not null && targetProperty.CanWrite)
                targetProperty.SetValue(targetItem, value);
        }
    }

    // ── Tìm kiếm (Ctrl+F) — quét toàn bộ property string/number của mỗi dòng ─────────────────

    private static void ShowFindDialog(DataGrid grid)
    {
        var owner = Window.GetWindow(grid);
        var searchBox = new TextBox { Width = 220, Margin = new Thickness(0, 0, 8, 0) };
        var findButton = new Button { Content = "Tìm tiếp", Padding = new Thickness(8, 2, 8, 2), IsDefault = true };

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12) };
        panel.Children.Add(searchBox);
        panel.Children.Add(findButton);

        var dialog = new Window
        {
            Title = "Tìm kiếm",
            Content = panel,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ShowInTaskbar = false,
        };

        if (_lastSearch.TryGetValue(grid, out var last)) searchBox.Text = last.Text;

        findButton.Click += (_, _) => FindNext(grid, searchBox.Text);
        searchBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) FindNext(grid, searchBox.Text);
        };

        searchBox.Focus();
        searchBox.SelectAll();
        dialog.ShowDialog();
    }

    private static void FindNext(DataGrid grid, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return;

        var last = _lastSearch.GetOrCreateValue(grid);
        var startIndex = last.Text == searchText ? last.LastIndex + 1 : 0;

        for (var offset = 0; offset < grid.Items.Count; offset++)
        {
            var index = (startIndex + offset) % grid.Items.Count;
            var item = grid.Items[index];
            if (!RowMatches(item, searchText)) continue;

            grid.SelectedItem = item;
            grid.ScrollIntoView(item);
            last.Text = searchText;
            last.LastIndex = index;
            return;
        }

        MessageBox.Show($"Không tìm thấy \"{searchText}\".", "Tìm kiếm", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static bool RowMatches(object item, string searchText)
    {
        foreach (var property in item.GetType().GetProperties())
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0) continue;

            object? value;
            try { value = property.GetValue(item); }
            catch { continue; }

            if (value is null || value is bool) continue;
            if (value.ToString()?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) return true;
        }

        return false;
    }

    // ── Xem số tồn vật tư (Ctrl+F2) ───────────────────────────────────────────────────────────

    private static void ShowProductStock(DataGrid grid)
    {
        var line = grid.CurrentCell.Item ?? grid.SelectedItem;
        if (line is null) return;

        var productId = GetProductId(line);
        if (productId is null)
        {
            MessageBox.Show("Chưa chọn sản phẩm cho dòng này.", "Số tồn vật tư", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var product = FindProductById(grid.DataContext, productId.Value);
        if (product is null)
        {
            MessageBox.Show("Không tìm thấy thông tin tồn kho cho sản phẩm này.", "Số tồn vật tư", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = product.GetType().GetProperty("Name")?.GetValue(product) as string ?? "";
        var stock = product.GetType().GetProperty("StockQuantity")?.GetValue(product);

        MessageBox.Show(
            stock is null
                ? $"Sản phẩm \"{name}\" không có dữ liệu tồn kho."
                : $"Sản phẩm \"{name}\" — tồn kho hiện tại: {stock}",
            "Số tồn vật tư",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // Line item có thể expose "ProductId" (int) trực tiếp (SalesOrderLineItem/SalesReturnLineItem)
    // hoặc chỉ có "SelectedProduct" (ISearchableItem, WarehouseReceiptLineItem) — thử cả 2 kiểu.
    private static int? GetProductId(object line)
    {
        var type = line.GetType();

        var productIdProperty = type.GetProperty("ProductId");
        if (productIdProperty?.GetValue(line) is int productId && productId > 0)
            return productId;

        var selectedProductProperty = type.GetProperty("SelectedProduct");
        if (selectedProductProperty?.GetValue(line) is { } selectedProduct)
        {
            var idProperty = selectedProduct.GetType().GetProperty("Id");
            if (idProperty?.GetValue(selectedProduct) is int id && id > 0) return id;
        }

        return null;
    }

    // ViewModel của cả 3 form đều expose 1 collection tên "Products" chứa danh sách sản phẩm
    // (ObservableCollection<ISearchableItem> hoặc IReadOnlyList<ISearchableItem>) đã nạp sẵn từ BE.
    private static object? FindProductById(object? viewModel, int productId)
    {
        if (viewModel is null) return null;

        var productsProperty = viewModel.GetType().GetProperty("Products");
        if (productsProperty?.GetValue(viewModel) is not IEnumerable products) return null;

        foreach (var product in products)
        {
            var idProperty = product.GetType().GetProperty("Id");
            if (idProperty?.GetValue(product) is int id && id == productId) return product;
        }

        return null;
    }
}
