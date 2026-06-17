// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

namespace DesktopLamour.Features.HomePage.SalesReturn.Views;

public partial class SalesReturnWindow : Window
{
    public SalesReturnViewModel ViewModel { get; }

    private SalesReturnResponseDto? _initialReturn;

    public SalesReturnWindow(SalesReturnViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += () => { if (IsVisible) DialogResult = true; };
        LinesDataGrid.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnProductCellTextChanged));
    }

    public void Initialize(SalesReturnResponseDto? salesReturn)
        => _initialReturn = salesReturn;

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        try
        {
            await ViewModel.InitializeAsync(_initialReturn);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải dữ liệu: {ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (ViewModel.IsDirty && DialogResult is null)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đóng? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận đóng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) e.Cancel = true;
        }
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
