// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Controls;
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderReportDetailView : UserControl
{
    public SalesOrderReportDetailView(SalesOrderReportDetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
