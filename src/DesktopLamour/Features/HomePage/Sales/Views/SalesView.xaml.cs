// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Input;
using DesktopLamour.Features.HomePage.Sales.Domain.Models;
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesView : System.Windows.Controls.UserControl
{
    private SalesViewModel ViewModel => (SalesViewModel)DataContext;

    public SalesView(SalesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeReportPanelCommand.ExecuteAsync(null);
    }

    private void ReportRowsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ReportRowsGrid.SelectedItem is ProductSalesSummaryRow row && ViewModel.DrillDownReportRowCommand.CanExecute(row))
            ViewModel.DrillDownReportRowCommand.Execute(row);
    }
}
