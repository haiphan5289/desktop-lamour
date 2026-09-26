// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class BulkCustomerReceiptWindow : Window
{
    public BulkCustomerReceiptViewModel ViewModel { get; }

    public BulkCustomerReceiptWindow(BulkCustomerReceiptViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += Close;
        viewModel.HostWindow    = this;
    }

    // 2026-09-26 (khớp MISA): KHÔNG còn tự nạp + tự bấm Thêm lúc cửa sổ hiện lên. Nơi mở
    // (AccountingViewModel.OpenBulkCustomerReceiptAsync) gọi ViewModel.StartNewAsync() TRƯỚC khi
    // Show() — bộ chọn chứng từ hiện đầu tiên, cửa sổ này chỉ xuất hiện sau khi bấm "✔ Thu tiền".

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
