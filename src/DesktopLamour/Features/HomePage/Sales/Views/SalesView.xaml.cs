// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesView : System.Windows.Controls.UserControl
{
    public SalesView(SalesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
