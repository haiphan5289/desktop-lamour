// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using Microsoft.Win32;
using System.IO;

namespace DesktopLamour.Features.HomePage.Customers.ViewModels;

public partial class CustomerListViewModel : ViewModelBase
{
    private readonly INavigationService              _navigationService;
    private readonly IGetCustomersUseCase            _getCustomers;
    private readonly IDeleteCustomerUseCase          _deleteCustomer;
    private readonly IDuplicateCustomerUseCase       _duplicateCustomer;
    private readonly IImportExcelCustomersUseCase    _importExcel;
    private readonly Func<CustomerFormWindow>        _formWindowFactory;

    [ObservableProperty] private bool      _isLoading;
    [ObservableProperty] private bool      _hasError;
    [ObservableProperty] private string    _errorMessage = string.Empty;
    [ObservableProperty] private bool      _hasCustomers;
    [ObservableProperty] private Customer? _selectedCustomer;

    // 1 ô tìm kiếm chung — khớp OR trên các trường text chính, không phân biệt hoa/thường.
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<Customer> Customers { get; } = new();

    // View lọc live theo các Filter* per-cột — DataGrid bind vào đây thay vì Customers trực tiếp;
    // Customers vẫn là nguồn dữ liệu thật (Add/Remove/Clear ở Load/Duplicate/Delete không đổi).
    public ICollectionView CustomersView { get; }

    public string TotalCustomersText => $"Tổng: {Customers.Count} khách hàng";

    private bool HasSelection => SelectedCustomer is not null;

    public CustomerListViewModel(
        INavigationService           navigationService,
        IGetCustomersUseCase         getCustomers,
        IDeleteCustomerUseCase       deleteCustomer,
        IDuplicateCustomerUseCase    duplicateCustomer,
        IImportExcelCustomersUseCase importExcel,
        Func<CustomerFormWindow>     formWindowFactory)
    {
        _navigationService = navigationService;
        _getCustomers      = getCustomers;
        _deleteCustomer    = deleteCustomer;
        _duplicateCustomer = duplicateCustomer;
        _importExcel       = importExcel;
        _formWindowFactory = formWindowFactory;

        CustomersView = CollectionViewSource.GetDefaultView(Customers);
        CustomersView.Filter = FilterCustomer;
    }

    partial void OnSearchTextChanged(string value) => CustomersView.Refresh();

    private bool FilterCustomer(object obj)
    {
        if (obj is not Customer c) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        return Matches(c.Code, SearchText)
            || Matches(c.Name, SearchText)
            || Matches(c.Address, SearchText)
            || Matches(c.Province, SearchText)
            || Matches(c.CustomerGroup, SearchText)
            || Matches(c.TaxCode, SearchText)
            || Matches(c.Phone, SearchText)
            || Matches(c.SaleCareEmployeeName, SearchText);
    }

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        DuplicateCustomerCommand.NotifyCanExecuteChanged();
        EditCustomerCommand.NotifyCanExecuteChanged();
        DeleteCustomerCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadCustomersAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getCustomers.ExecuteAsync(ct);
            Customers.Clear();
            foreach (var c in items) Customers.Add(c);
            HasCustomers = Customers.Count > 0;
            OnPropertyChanged(nameof(TotalCustomersText));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AddCustomerAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadCustomersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DuplicateCustomerAsync(CancellationToken ct = default)
    {
        if (SelectedCustomer is null) return;
        IsLoading = true;
        try
        {
            var copy = await _duplicateCustomer.ExecuteAsync(SelectedCustomer.Id, ct);
            Customers.Add(copy);
            HasCustomers     = true;
            SelectedCustomer = copy;
            OnPropertyChanged(nameof(TotalCustomersText));
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Nhân bản thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditCustomerAsync(CancellationToken ct = default)
    {
        if (SelectedCustomer is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedCustomer);
        if (window.ShowDialog() == true)
            await LoadCustomersCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task ImportExcelAsync(CancellationToken ct = default)
    {
        var dialog = new OpenFileDialog
        {
            Title  = "Chọn file Excel khách hàng",
            Filter = "Excel files (*.xlsx)|*.xlsx",
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            await using var stream = File.OpenRead(dialog.FileName);
            var result = await _importExcel.ExecuteAsync(stream, Path.GetFileName(dialog.FileName), ct);

            await LoadCustomersCommand.ExecuteAsync(null);

            var message = $"Import hoàn tất!\n\nĐã import: {result.Imported}/{result.Total} khách hàng.";
            if (result.Errors.Count > 0)
            {
                var errorLines = result.Errors
                    .Take(10)
                    .Select(e => $"  Dòng {e.Row}: {e.Reason}");
                message += $"\n\nDòng lỗi ({result.Skipped}):\n{string.Join("\n", errorLines)}";
                if (result.Errors.Count > 10)
                    message += $"\n  ... và {result.Errors.Count - 10} dòng lỗi khác";
            }

            MessageBox.Show(message, "Kết quả Import", MessageBoxButton.OK,
                result.Errors.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Import thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    // Xuất đúng những dòng đang hiển thị trên lưới (đã áp bộ lọc Tìm kiếm), không phải toàn bộ
    // Customers — khớp kỳ vọng thông thường: xuất "cái đang thấy trên màn hình".
    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"KhachHang_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook = BuildExportWorkbook();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Header khớp đúng HeaderAliases mà ImportExcelCustomersUseCase (BE) đọc — file xuất ra có
    // thể sửa rồi import lại ngay mà không cần đổi tên cột.
    private XLWorkbook BuildExportWorkbook()
    {
        var workbook  = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Khách hàng");

        string[] headers =
        {
            "Tên khách hàng", "Địa chỉ", "Tỉnh/TP", "Nhóm KH/NCC", "Mã số thuế", "Điện thoại", "Tên nhân viên",
        };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value           = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var c in CustomersView.Cast<Customer>())
        {
            worksheet.Cell(row, 1).Value = c.Name;
            worksheet.Cell(row, 2).Value = c.Address;
            worksheet.Cell(row, 3).Value = c.Province;
            worksheet.Cell(row, 4).Value = c.CustomerGroup;
            worksheet.Cell(row, 5).Value = c.TaxCode;
            worksheet.Cell(row, 6).Value = c.Phone;
            worksheet.Cell(row, 7).Value = c.SaleCareEmployeeName;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        return workbook;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteCustomerAsync(CancellationToken ct = default)
    {
        if (SelectedCustomer is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa khách hàng '{SelectedCustomer.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteCustomer.ExecuteAsync(SelectedCustomer.Id, ct);
            Customers.Remove(SelectedCustomer);
            SelectedCustomer = null;
            HasCustomers     = Customers.Count > 0;
            OnPropertyChanged(nameof(TotalCustomersText));
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
