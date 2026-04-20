// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Domain.Models;
using DesktopLamour.Features.HomePage.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Views;

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
}
