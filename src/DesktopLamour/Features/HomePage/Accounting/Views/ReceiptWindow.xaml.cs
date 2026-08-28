// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class ReceiptWindow : Window
{
    public ReceiptViewModel ViewModel { get; }

    // Set trước khi Show() để mở thẳng vào 1 phiếu thu cụ thể (VD: "Xem" từ Sổ Kế Toán Chi Tiết
    // Quỹ Tiền Mặt) thay vì form Thêm mới trống — xem OnContentRendered.
    public string? InitialDocumentNumber { get; set; }

    public ReceiptWindow(ReceiptViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += Close;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadAsync();
        if (!string.IsNullOrEmpty(InitialDocumentNumber))
            ViewModel.NavigateToReceiptByDocumentNumber(InitialDocumentNumber);
        else
            ViewModel.AddNewCommand.Execute(null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();
}
