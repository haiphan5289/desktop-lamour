// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class CashLedgerReportFilterWindow : Window
{
    private readonly CashLedgerReportFilterViewModel _viewModel;

    public CashLedgerReportFilterWindow(CashLedgerReportFilterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = viewModel;

        Loaded += async (_, _) => await viewModel.LoadLookupsCommand.ExecuteAsync(null);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    // Mở lại hộp với đúng tham số đang xem (nút "Chọn tham số" trên báo cáo).
    public void Initialize(CashLedgerReportFilter? previous) => _viewModel.Initialize(previous);

    public CashLedgerReportFilter BuildFilter() => _viewModel.BuildFilter();

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CashLedgerReportFilterViewModel.DialogResult)) return;
        DialogResult = _viewModel.DialogResult;
        Close();
    }
}
