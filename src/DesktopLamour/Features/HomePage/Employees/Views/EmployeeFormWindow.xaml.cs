// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using DesktopLamour.Features.HomePage.Employees.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Employees.Views;

public partial class EmployeeFormWindow : Window
{
    public EmployeeFormViewModel ViewModel { get; }

    public EmployeeFormWindow(EmployeeFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(Employee? employee) => ViewModel.Initialize(employee);
}
