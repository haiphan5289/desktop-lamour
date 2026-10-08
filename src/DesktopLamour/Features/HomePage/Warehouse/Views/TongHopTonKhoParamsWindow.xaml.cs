// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

// Hộp "Chọn tham số" của Tổng hợp tồn kho — DataContext do TongHopTonKhoViewModel gán lúc mở.
public partial class TongHopTonKhoParamsWindow : Window
{
    public TongHopTonKhoParamsWindow() => InitializeComponent();

    private void OkButton_Click(object sender, RoutedEventArgs e)     => DialogResult = true;
    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
