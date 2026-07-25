// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
using DesktopLamour.Features.HomePage.Categories.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Categories.Views;

public partial class CategoryFormWindow : Window
{
    public CategoryFormViewModel ViewModel { get; }

    public CategoryFormWindow(CategoryFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(Category? category = null) => ViewModel.Initialize(category);

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
