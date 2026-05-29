// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Suppliers.Views;

public partial class SupplierFormWindow : Window
{
    public SupplierFormViewModel ViewModel { get; }

    public SupplierFormWindow(SupplierFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(Supplier? supplier) => ViewModel.Initialize(supplier);

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
