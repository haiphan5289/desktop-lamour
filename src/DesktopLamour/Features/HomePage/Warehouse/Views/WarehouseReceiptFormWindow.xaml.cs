// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;
using DesktopLamour.Shared.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class WarehouseReceiptFormWindow : Window
{
    public WarehouseReceiptFormViewModel ViewModel { get; }

    // Cất/Bỏ ghi không còn tự đóng popup (xem WarehouseReceiptFormViewModel.SaveAsync) — nhớ đã có thay
    // đổi để trả DialogResult=true khi đóng, nếu không danh sách "Nhập, Xuất Kho" sẽ không reload
    // (chỉ reload khi ShowDialog() == true).
    private bool _hasSaved;

    public WarehouseReceiptFormWindow(WarehouseReceiptFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
        viewModel.ReceiptSaved += () => _hasSaved = true;

        // Chọn xong 1 sản phẩm trong AppSearchableComboBox (Mã hàng/Tên hàng) → CommitEdit ngay cho cả
        // dòng để các cột tự điền (Tên/Kho/TK/ĐVT/Đơn giá) cập nhật liền — cùng cách SalesOrderWindow.
        LinesDataGrid.AddHandler(AppSearchableComboBox.SelectionCommittedEvent,
            new RoutedEventHandler((_, _) => Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(() => LinesDataGrid.CommitEdit(DataGridEditingUnit.Row, true)))));
    }

    // Mở form ở chế độ Sửa 1 phiếu đã tồn tại (vd. click 1 dòng NK từ "Nhập, Xuất Kho").
    // Không gọi (hoặc truyền null) → form tạo mới như cũ.
    public void Initialize(WarehouseReceiptResponseDto? existing) => ViewModel.Initialize(existing);

    // Cho phép Trước/Sau/Thêm duyệt ngay trong popup khi mở từ "Nhập, Xuất Kho"
    // (WarehouseTransactionListViewModel) — không gọi thì Trước/Sau chỉ là no-op.
    public void SetSiblingContext(IReadOnlyList<int> receiptIds, int currentIndex)
        => ViewModel.SetSiblingContext(receiptIds, currentIndex);

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (ViewModel.IsDirty && DialogResult is null)
        {
            var r = MessageBox.Show(
                "Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) { e.Cancel = true; return; }
        }

        if (_hasSaved && DialogResult is null) DialogResult = true;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadAsync();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
