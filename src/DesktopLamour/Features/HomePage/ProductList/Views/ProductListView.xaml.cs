// ProductListView.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.ProductList.ViewModels;
using System.Windows.Controls;

namespace DesktopLamour.Features.HomePage.ProductList.Views;

public partial class ProductListView : UserControl
{
    public ProductListView(ProductListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
