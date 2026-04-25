// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.ViewModels;
using System.Windows.Controls;
namespace DesktopLamour.Features.HomePage.Employees.Views;

public partial class EmployeeListView : UserControl
{
    public EmployeeListView(EmployeeListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadEmployeesCommand.ExecuteAsync(null);
    }
}
