// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Controls;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class CashLedgerDetailReportView : UserControl
{
    public CashLedgerDetailReportView(CashLedgerDetailReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
