// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.SalesReturn.ViewModels;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.SalesReturn.Views;

public partial class SalesReturnWindow : Window
{
    public SalesReturnViewModel ViewModel { get; }

    private SalesReturnResponseDto? _initialReturn;

    public SalesReturnWindow(SalesReturnViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };

        // Chọn xong 1 sản phẩm trong AppSearchableComboBox (Mã hàng/Tên hàng) → CommitEdit ngay
        // cho cả dòng, không đợi user bấm ra ngoài — tránh các cột tự điền (ĐVT/Đơn giá/TK...)
        // trông như chưa cập nhật vì dòng vẫn còn ở trạng thái edit của riêng cell Mã/Tên hàng.
        // Trì hoãn qua Dispatcher (ContextIdle) — gọi CommitEdit ngay trong lúc WPF còn đang xử lý
        // click chọn item (PreviewMouseDown) dễ đụng vào state máy edit nội bộ của DataGrid. Khớp
        // đúng cách SalesOrderWindow đang làm cho cùng control này.
        LinesDataGrid.AddHandler(AppSearchableComboBox.SelectionCommittedEvent,
            new RoutedEventHandler((_, _) => Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.ContextIdle,
                new Action(() => LinesDataGrid.CommitEdit(DataGridEditingUnit.Row, true)))));
    }

    public void Initialize(SalesReturnResponseDto? salesReturn)
        => _initialReturn = salesReturn;

    // Cho phép Trước/Sau/Thêm duyệt ngay trong popup khi mở từ 1 danh sách đã tải sẵn (vd.
    // SalesReturnListViewModel.SalesReturns) — không gọi thì Trước/Sau chỉ là no-op.
    public void SetSiblingContext(IReadOnlyList<SalesReturnResponseDto> siblings, int currentIndex)
        => ViewModel.SetSiblingContext(siblings, currentIndex);

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        try
        {
            await ViewModel.InitializeAsync(_initialReturn);
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
