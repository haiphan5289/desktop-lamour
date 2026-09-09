// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Controls;
using System.Windows.Input;
using DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

namespace DesktopLamour.Features.HomePage.SalesReturn.Views;

public partial class SalesReturnListView : UserControl
{
    private SalesReturnListViewModel ViewModel => (SalesReturnListViewModel)DataContext;

    public SalesReturnListView(SalesReturnListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    // Double-click mở Sửa; fallback qua Xem nếu không có dòng nào được chọn.
    private async void ReturnsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.EditSalesReturnCommand.CanExecute(null))
            await ViewModel.EditSalesReturnCommand.ExecuteAsync(null);
        else if (ViewModel.ViewSalesReturnCommand.CanExecute(null))
            await ViewModel.ViewSalesReturnCommand.ExecuteAsync(null);
    }
}
