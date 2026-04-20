// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Views;

public partial class SupplierListView : UserControl
{
    public SupplierListView(SupplierListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadSuppliersCommand.ExecuteAsync(null);
    }
}
