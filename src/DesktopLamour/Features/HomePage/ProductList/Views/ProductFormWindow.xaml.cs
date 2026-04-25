// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.ProductList.Views;

public partial class ProductFormWindow : Window
{
    public ProductFormViewModel ViewModel { get; }

    public ProductFormWindow(ProductFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(Product? product) => ViewModel.Initialize(product);
}
