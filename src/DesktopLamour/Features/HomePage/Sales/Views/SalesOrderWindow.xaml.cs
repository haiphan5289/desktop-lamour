// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Sales.ViewModels;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderWindow : Window
{
    public SalesOrderViewModel ViewModel { get; }

    private SalesOrderResponseDto? _initialOrder;

    public SalesOrderWindow(SalesOrderViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };
        LinesDataGrid.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnProductCellTextChanged));
    }

    public void Initialize(SalesOrderResponseDto? order)
        => _initialOrder = order;

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.InitializeAsync(_initialOrder);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
        => Close();

    private void OnProductCellTextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.OriginalSource is not TextBox textBox || !textBox.IsKeyboardFocused) return;

        var cell = FindParent<DataGridCell>(textBox);
        if (cell is null) return;

        switch (cell.Column?.Header?.ToString())
        {
            case "Mã hàng":
                ViewModel.FilterProductsByCode(textBox.Text);
                if (FindParent<ComboBox>(textBox) is { } combo)
                    combo.IsDropDownOpen = true;
                break;
            case "Tên hàng":
                ViewModel.FilterProductsByName(textBox.Text);
                if (FindParent<ComboBox>(textBox) is { } comboName)
                    comboName.IsDropDownOpen = true;
                break;
        }
    }

    private void LinesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        => ViewModel.ResetProductFilter();

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent is null) return null;
        return parent is T p ? p : FindParent<T>(parent);
    }
}
