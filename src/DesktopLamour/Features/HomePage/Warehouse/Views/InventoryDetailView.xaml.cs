// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows.Input;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

public partial class InventoryDetailView : System.Windows.Controls.UserControl
{
    public InventoryDetailView(InventoryDetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void LinesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not InventoryDetailViewModel vm) return;
        if (LinesDataGrid.SelectedItem is not InventoryDetailLine line) return;
        if (vm.OpenDocumentCommand.CanExecute(line))
            vm.OpenDocumentCommand.Execute(line);
    }
}
