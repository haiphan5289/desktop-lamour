// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductUnits.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.ProductUnits.Views;

public partial class ProductUnitListView : UserControl
{
    public ProductUnitListView(ProductUnitListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadUnitsCommand.ExecuteAsync(null);
    }
}
