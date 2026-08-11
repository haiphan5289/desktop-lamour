// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Warehouses.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Warehouses.Views;

public partial class ExpenseCategoryListView : UserControl
{
    public ExpenseCategoryListView(ExpenseCategoryListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }
}
