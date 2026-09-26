// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class BulkCustomerReceiptSearchWindow : Window
{
    public BulkCustomerReceiptSearchViewModel ViewModel { get; }

    public BulkCustomerReceiptSearchWindow(BulkCustomerReceiptSearchViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.HostWindow = this;
        // 2026-09-26: popup giờ chỉ là bộ chọn — true = đã chọn hợp lệ (bấm "✔ Thu tiền"), false =
        // "Hủy bỏ". Nơi gọi (BulkCustomerReceiptViewModel.AddNewAsync) đọc ViewModel.SelectedItems +
        // PaymentMethod/BankAccount/SelectedEmployee/CollectionDate SAU KHI ShowDialog() trả về true.
        ViewModel.RequestClose += ok => { DialogResult = ok; Close(); };
        Loaded += async (_, _) => await ViewModel.InitializeCommand.ExecuteAsync(null);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
