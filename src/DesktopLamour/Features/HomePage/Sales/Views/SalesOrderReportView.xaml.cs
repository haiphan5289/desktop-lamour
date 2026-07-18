// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
}
