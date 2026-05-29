// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
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

    public ObservableCollection<Customer> Customers { get; } = new();

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
    }

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        DuplicateCustomerCommand.NotifyCanExecuteChanged();
        EditCustomerCommand.NotifyCanExecuteChanged();
        DeleteCustomerCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

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
