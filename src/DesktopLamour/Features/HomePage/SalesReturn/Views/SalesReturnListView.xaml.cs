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

    // Nháp → mở Sửa (chỉnh được); Đã ghi sổ → Sửa bị chặn CanExecute nên fallback qua Xem
    // (cùng popup, chỉ khác BE tự chặn Update khi Confirmed) — double-click luôn mở được 1 popup
    // nào đó thay vì im lặng không làm gì khi dòng đã Ghi sổ.
    private async void ReturnsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.EditSalesReturnCommand.CanExecute(null))
            await ViewModel.EditSalesReturnCommand.ExecuteAsync(null);
        else if (ViewModel.ViewSalesReturnCommand.CanExecute(null))
            await ViewModel.ViewSalesReturnCommand.ExecuteAsync(null);
    }
}
