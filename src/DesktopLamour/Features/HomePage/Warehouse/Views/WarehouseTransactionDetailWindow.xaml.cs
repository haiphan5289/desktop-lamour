// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

// Popup mở khi double-click 1 dòng chứng từ ở màn "Nhập, Xuất Kho" — chỉ hiển thị dữ liệu,
// không có ViewModel riêng vì không có logic/command nào ngoài đóng cửa sổ.
public partial class WarehouseTransactionDetailWindow : Window
{
    public WarehouseTransactionDetailWindow()
    {
        InitializeComponent();
    }

    public void Initialize(WarehouseTransactionResponseDto transaction) => DataContext = transaction;

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
