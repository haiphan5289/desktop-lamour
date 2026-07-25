// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;

namespace DesktopLamour.Features.HomePage.Employees.ViewModels;

public partial class EmployeeListViewModel : ViewModelBase
{
    private readonly INavigationService         _navigationService;
    private readonly IGetEmployeesUseCase       _getEmployees;
    private readonly IDeleteEmployeeUseCase     _deleteEmployee;
    private readonly IDuplicateEmployeeUseCase  _duplicateEmployee;
    private readonly Func<EmployeeFormWindow>   _formWindowFactory;

    [ObservableProperty] private bool       _isLoading;
    [ObservableProperty] private bool       _hasError;
    [ObservableProperty] private string     _errorMessage  = string.Empty;
    [ObservableProperty] private bool       _hasEmployees;
    [ObservableProperty] private Employee?  _selectedEmployee;

    public ObservableCollection<Employee> Employees { get; } = new();

    private bool HasSelection => SelectedEmployee is not null;

    public EmployeeListViewModel(
        INavigationService        navigationService,
        IGetEmployeesUseCase      getEmployees,
        IDeleteEmployeeUseCase    deleteEmployee,
        IDuplicateEmployeeUseCase duplicateEmployee,
        Func<EmployeeFormWindow>  formWindowFactory)
    {
        _navigationService = navigationService;
        _getEmployees      = getEmployees;
        _deleteEmployee    = deleteEmployee;
        _duplicateEmployee = duplicateEmployee;
        _formWindowFactory = formWindowFactory;
    }

    partial void OnSelectedEmployeeChanged(Employee? value)
    {
        DuplicateEmployeeCommand.NotifyCanExecuteChanged();
        EditEmployeeCommand.NotifyCanExecuteChanged();
        DeleteEmployeeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadEmployeesAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getEmployees.ExecuteAsync(ct);
            Employees.Clear();
            foreach (var e in items) Employees.Add(e);
            HasEmployees = Employees.Count > 0;
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
    private async Task AddEmployeeAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadEmployeesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DuplicateEmployeeAsync(CancellationToken ct = default)
    {
        if (SelectedEmployee is null) return;
        IsLoading = true;
        try
        {
            var copy = await _duplicateEmployee.ExecuteAsync(SelectedEmployee.Id, ct);
            Employees.Add(copy);
            HasEmployees     = true;
            SelectedEmployee = copy;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Nhân bản thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditEmployeeAsync(CancellationToken ct = default)
    {
        if (SelectedEmployee is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedEmployee);
        if (window.ShowDialog() == true)
            await LoadEmployeesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteEmployeeAsync(CancellationToken ct = default)
    {
        if (SelectedEmployee is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa nhân viên '{SelectedEmployee.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteEmployee.ExecuteAsync(SelectedEmployee.Id, ct);
            Employees.Remove(SelectedEmployee);
            SelectedEmployee = null;
            HasEmployees     = Employees.Count > 0;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
