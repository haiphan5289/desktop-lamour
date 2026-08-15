// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseReceiptListViewModel : ViewModelBase
{
    private readonly IGetWarehouseReceiptsUseCase     _getUseCase;
    private readonly IConfirmWarehouseReceiptUseCase  _confirmUseCase;
    private readonly INavigationService               _navigationService;
    private readonly Func<WarehouseReceiptFormWindow> _formWindowFactory;
    private readonly ILogger<WarehouseReceiptListViewModel> _logger;

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasItems;

    public ObservableCollection<WarehouseReceiptFlatItem> Items { get; } = new();

    public WarehouseReceiptListViewModel(
        IGetWarehouseReceiptsUseCase     getUseCase,
        IConfirmWarehouseReceiptUseCase  confirmUseCase,
        INavigationService               navigationService,
        Func<WarehouseReceiptFormWindow> formWindowFactory,
        ILogger<WarehouseReceiptListViewModel> logger)
    {
        _getUseCase        = getUseCase;
        _confirmUseCase    = confirmUseCase;
        _navigationService = navigationService;
        _formWindowFactory = formWindowFactory;
        _logger            = logger;
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private void OpenForm()
    {
        var window = _formWindowFactory();
        window.Owner = Application.Current.MainWindow;
        var result = window.ShowDialog();
        if (result == true)
            LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var receipts = await _getUseCase.ExecuteAsync(ct);
            Items.Clear();
            foreach (var r in receipts)
            {
                if (r.Lines.Count == 0)
                {
                    Items.Add(ToFlatItem(r, productCode: string.Empty, productName: string.Empty));
                    continue;
                }
                foreach (var line in r.Lines)
                    Items.Add(ToFlatItem(r, line.ProductCode, line.ProductName));
            }
            HasItems = Items.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load warehouse receipts");
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    private static WarehouseReceiptFlatItem ToFlatItem(
        WarehouseReceiptResponseDto r, string productCode, string productName)
        => new()
        {
            Id            = r.Id,
            ReceiptNumber = r.ReceiptNumber,
            ReceiptType   = r.ReceiptType,
            Status        = r.Status,
            CustomerName  = r.CustomerName,
            SupplierName  = r.SupplierName,
            EmployeeName  = r.EmployeeName,
            DocumentDate  = r.DocumentDate,
            TotalAmount   = r.TotalAmount,
            ProductCode   = productCode,
            ProductName   = productName,
        };

    [RelayCommand]
    private async Task ConfirmReceiptAsync(int id, CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            await _confirmUseCase.ExecuteAsync(id, ct);
            _logger.LogInformation("Warehouse receipt {Id} confirmed", id);
            await LoadAsync(ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm warehouse receipt {Id}", id);
            HasError     = true;
            ErrorMessage = $"Không thể ghi sổ: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
