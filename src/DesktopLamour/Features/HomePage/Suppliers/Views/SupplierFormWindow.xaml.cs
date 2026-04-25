// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.ViewModels;
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
}
