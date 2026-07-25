// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Categories.Views;

public partial class CategoryListView : UserControl
{
    public CategoryListView(CategoryListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }
}
