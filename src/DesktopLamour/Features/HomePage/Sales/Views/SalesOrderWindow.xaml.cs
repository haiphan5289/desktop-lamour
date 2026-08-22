// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Sales.ViewModels;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderWindow : Window
{
    public SalesOrderViewModel ViewModel { get; }

    private SalesOrderResponseDto? _initialOrder;

    public SalesOrderWindow(SalesOrderViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };

        // Chọn xong 1 sản phẩm trong AppSearchableComboBox (Mã hàng/Tên hàng) → CommitEdit ngay
        // cho cả dòng, không đợi user bấm ra ngoài — tránh các cột tự điền (ĐVT/Đơn giá/TK...)
        // trông như chưa cập nhật vì dòng vẫn còn ở trạng thái edit của riêng cell Mã/Tên hàng.
        // Trì hoãn qua Dispatcher (ContextIdle) — gọi CommitEdit ngay trong lúc WPF còn đang xử lý
        // click chọn item (PreviewMouseDown) dễ đụng vào state máy edit nội bộ của DataGrid.
        LinesDataGrid.AddHandler(AppSearchableComboBox.SelectionCommittedEvent,
            new RoutedEventHandler((_, _) => Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(() => LinesDataGrid.CommitEdit(DataGridEditingUnit.Row, true)))));
    }

    public void Initialize(SalesOrderResponseDto? order, bool isFromWarehouseExport = false, bool isReadOnly = false)
    {
        _initialOrder = order;
        ViewModel.IsFromWarehouseExport = isFromWarehouseExport;
        ViewModel.IsReadOnly            = isReadOnly;
        // Chỉ đổi gợi ý hiển thị (placeholder) khi ô Số chứng từ còn trống — không đụng số chứng
        // từ thật, vốn luôn sinh dạng "XK{5 digits}" bất kể mở từ đâu (xem GetNextSalesOrderCodeUseCase).
        ViewModel.DocumentNumberPlaceholder = isFromWarehouseExport ? "XK00001" : "BH00001";
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        try
        {
            await ViewModel.InitializeAsync(_initialOrder);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (ViewModel.IsDirty && DialogResult is null)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đóng? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận đóng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) e.Cancel = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
