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
        UpdateColumnHeaders();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SalesOrderReportViewModel.IsUnitColumnVisible)
                           or nameof(SalesOrderReportViewModel.IsCustomerGroupColumnVisible)
                           or nameof(SalesOrderReportViewModel.IsOuterColumnVisible))
            UpdateColumnVisibility();
        if (e.PropertyName is nameof(SalesOrderReportViewModel.InnerCodeLabel)
                           or nameof(SalesOrderReportViewModel.InnerNameLabel)
                           or nameof(SalesOrderReportViewModel.OuterCodeLabel)
                           or nameof(SalesOrderReportViewModel.OuterNameLabel))
            UpdateColumnHeaders();
    }

    private void UpdateColumnVisibility()
    {
        // Số lượng bán/SL trả lại chỉ có ý nghĩa khi report có dimension Mặt hàng — ẩn cùng lúc
        // với ĐVT (IsUnitColumnVisible) để khớp đúng bộ cột report "theo nhân viên và khách hàng"
        // của MISA (report đó hoàn toàn không có 3 cột này).
        var productMetricsVisibility = _viewModel.IsUnitColumnVisible ? Visibility.Visible : Visibility.Collapsed;
        UnitColumn.Visibility         = productMetricsVisibility;
        QuantitySoldColumn.Visibility = productMetricsVisibility;
        ReturnQuantityColumn.Visibility = productMetricsVisibility;
        CustomerGroupNameColumn.Visibility =
            _viewModel.IsCustomerGroupColumnVisible ? Visibility.Visible : Visibility.Collapsed;
        var outerVisibility = _viewModel.IsOuterColumnVisible ? Visibility.Visible : Visibility.Collapsed;
        OuterCodeColumn.Visibility = outerVisibility;
        OuterNameColumn.Visibility = outerVisibility;
    }

    // "Mã hàng"/"Tên hàng" là header cứng trong XAML nhưng dữ liệu là danh tính của dimension
    // TRONG của report type đang chọn (mặt hàng/khách hàng/nhân viên) — đổi header đúng theo đó
    // thay vì luôn hiện "Mã hàng"/"Tên hàng" dù đang xem dữ liệu nhân viên/khách hàng. Cột NGOÀI
    // ("Mã NV"/"Tên NV") chỉ có header khi report 2 chiều — rỗng thì cứ để mặc định, cột đã ẩn.
    private void UpdateColumnHeaders()
    {
        ProductCodeColumn.Header = _viewModel.InnerCodeLabel;
        ProductNameColumn.Header = _viewModel.InnerNameLabel;
        if (_viewModel.IsOuterColumnVisible)
        {
            OuterCodeColumn.Header = _viewModel.OuterCodeLabel;
            OuterNameColumn.Header = _viewModel.OuterNameLabel;
        }
    }

    private void ReportGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ReportGrid.SelectedItem is ReportDisplayRow row && _viewModel.DrillDownCommand.CanExecute(row))
            _viewModel.DrillDownCommand.Execute(row);
    }
}
