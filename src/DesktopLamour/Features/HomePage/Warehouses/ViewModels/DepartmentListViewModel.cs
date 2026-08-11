// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Views;

namespace DesktopLamour.Features.HomePage.Warehouses.ViewModels;

public partial class DepartmentListViewModel : ViewModelBase
{
    private readonly INavigationService          _navigationService;
    private readonly IGetDepartmentsUseCase      _getDepartments;
    private readonly IDeleteDepartmentUseCase    _deleteDepartment;
    private readonly Func<DepartmentFormWindow>  _formWindowFactory;

    [ObservableProperty] private bool          _isLoading;
    [ObservableProperty] private bool          _hasError;
    [ObservableProperty] private string        _errorMessage = string.Empty;
    [ObservableProperty] private bool          _hasDepartments;
    [ObservableProperty] private Department?   _selectedDepartment;

    public ObservableCollection<Department> Departments { get; } = new();

    private bool HasSelection => SelectedDepartment is not null;

    public DepartmentListViewModel(
        INavigationService          navigationService,
        IGetDepartmentsUseCase      getDepartments,
        IDeleteDepartmentUseCase    deleteDepartment,
        Func<DepartmentFormWindow>  formWindowFactory)
    {
        _navigationService  = navigationService;
        _getDepartments     = getDepartments;
        _deleteDepartment   = deleteDepartment;
        _formWindowFactory  = formWindowFactory;
    }

    partial void OnSelectedDepartmentChanged(Department? value)
    {
        EditDepartmentCommand.NotifyCanExecuteChanged();
        DeleteDepartmentCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadDepartmentsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getDepartments.ExecuteAsync(ct);
            Departments.Clear();
            foreach (var d in items) Departments.Add(d);
            HasDepartments = Departments.Count > 0;
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
    private async Task AddDepartmentAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadDepartmentsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditDepartmentAsync(CancellationToken ct = default)
    {
        if (SelectedDepartment is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedDepartment);
        if (window.ShowDialog() == true)
            await LoadDepartmentsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteDepartmentAsync(CancellationToken ct = default)
    {
        if (SelectedDepartment is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa phòng ban '{SelectedDepartment.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteDepartment.ExecuteAsync(SelectedDepartment.Id, ct);
            Departments.Remove(SelectedDepartment);
            SelectedDepartment = null;
            HasDepartments      = Departments.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
