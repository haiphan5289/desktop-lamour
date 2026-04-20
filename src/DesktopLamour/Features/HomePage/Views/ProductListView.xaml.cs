// ProductListView.xaml.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.ViewModels;
using System.Windows.Controls;

namespace DesktopLamour.Features.HomePage.Views;

public partial class ProductListView : UserControl
{
    public ProductListView(ProductListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
