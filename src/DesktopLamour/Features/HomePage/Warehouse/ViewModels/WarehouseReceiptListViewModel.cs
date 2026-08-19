// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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

    // 1 ô tìm kiếm chung — khớp OR trên các trường text chính, không phân biệt hoa/thường.
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<WarehouseReceiptFlatItem> Items { get; } = new();

    // View lọc live theo các Filter* per-cột — DataGrid bind vào đây thay vì Items trực tiếp;
    // Items vẫn là nguồn dữ liệu thật (Clear/Add ở Load không đổi).
    public ICollectionView ItemsView { get; }

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

        ItemsView = CollectionViewSource.GetDefaultView(Items);
        ItemsView.Filter = FilterItem;
    }

    partial void OnSearchTextChanged(string value) => ItemsView.Refresh();

    private bool FilterItem(object obj)
    {
        if (obj is not WarehouseReceiptFlatItem item) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        return Matches(item.ReceiptNumber, SearchText)
            || Matches(item.ProductCode, SearchText)
            || Matches(item.ProductName, SearchText)
            || Matches(ReceiptTypeLabel(item.ReceiptType), SearchText)
            || Matches(item.ObjectName, SearchText)
            || Matches(item.EmployeeName, SearchText);
    }

    // Khớp đúng nhãn hiển thị trong DataTrigger của WarehouseReceiptListView.xaml (cột "Loại phiếu")
    // để filter theo text khớp với cái user nhìn thấy trên lưới, không phải giá trị int thô.
    private static string ReceiptTypeLabel(int receiptType) => receiptType switch
    {
        1 => "Thành phẩm sản xuất",
        2 => "Hàng bán bị trả lại",
        3 => "Khác (NVL thừa, HH thuê gia công,...)",
        4 => "Hàng nhận gia công",
        _ => string.Empty,
    };

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));

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
