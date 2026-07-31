// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderReportView : UserControl
{
    private readonly SalesOrderReportViewModel _viewModel;

    public SalesOrderReportView(SalesOrderReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = viewModel;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateColumnVisibility();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SalesOrderReportViewModel.IsUnitColumnVisible))
            UpdateColumnVisibility();
    }

    private void UpdateColumnVisibility()
        => UnitColumn.Visibility = _viewModel.IsUnitColumnVisible ? Visibility.Visible : Visibility.Collapsed;

    private void ReportGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ReportGrid.SelectedItem is ReportDisplayRow row && _viewModel.DrillDownCommand.CanExecute(row))
            _viewModel.DrillDownCommand.Execute(row);
    }
}
