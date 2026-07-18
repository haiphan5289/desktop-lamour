// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderReportFilterWindow : Window
{
    private readonly SalesOrderReportFilterViewModel _viewModel;

    public SalesOrderReportFilterWindow(SalesOrderReportFilterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = viewModel;

        Loaded += async (_, _) => await viewModel.LoadLookupsCommand.ExecuteAsync(null);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public SalesOrderReportFilter BuildFilter() => _viewModel.BuildFilter();

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SalesOrderReportFilterViewModel.DialogResult)) return;
        DialogResult = _viewModel.DialogResult;
        Close();
    }
}
