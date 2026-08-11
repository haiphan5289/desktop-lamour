// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouses.Views;

public partial class ExpenseCategoryFormWindow : Window
{
    public ExpenseCategoryFormViewModel ViewModel { get; }

    public ExpenseCategoryFormWindow(ExpenseCategoryFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(ExpenseCategory? category = null) => ViewModel.Initialize(category);

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
            if (r != MessageBoxResult.Yes) e.Cancel = true;
        }
    }
}
