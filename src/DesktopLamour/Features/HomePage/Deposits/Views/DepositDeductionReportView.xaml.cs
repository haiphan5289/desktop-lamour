// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.ViewModels;
using System.Windows.Controls;

namespace DesktopLamour.Features.HomePage.Deposits.Views;

public partial class DepositDeductionReportView : UserControl
{
    public DepositDeductionReportView(DepositDeductionReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadCommand.ExecuteAsync(null);
    }
}
