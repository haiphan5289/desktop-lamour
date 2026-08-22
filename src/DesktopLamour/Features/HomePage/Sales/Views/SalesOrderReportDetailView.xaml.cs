// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Controls;
using System.Windows.Input;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderReportDetailView : UserControl
{
    private readonly SalesOrderReportDetailViewModel _viewModel;

    public SalesOrderReportDetailView(SalesOrderReportDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = viewModel;
    }

    private void DetailGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DetailGrid.SelectedItem is SalesOrderReportLineItem row && _viewModel.OpenOrderCommand.CanExecute(row))
            _viewModel.OpenOrderCommand.Execute(row);
    }
}
