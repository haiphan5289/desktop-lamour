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

public partial class ExpenseCategoryListViewModel : ViewModelBase
{
    private readonly INavigationService              _navigationService;
    private readonly IGetExpenseCategoriesUseCase     _getCategories;
    private readonly IDeleteExpenseCategoryUseCase    _deleteCategory;
    private readonly Func<ExpenseCategoryFormWindow>  _formWindowFactory;

    [ObservableProperty] private bool              _isLoading;
    [ObservableProperty] private bool              _hasError;
    [ObservableProperty] private string            _errorMessage = string.Empty;
    [ObservableProperty] private bool              _hasCategories;
    [ObservableProperty] private ExpenseCategory?  _selectedCategory;

    public ObservableCollection<ExpenseCategory> Categories { get; } = new();

    private bool HasSelection => SelectedCategory is not null;

    public ExpenseCategoryListViewModel(
        INavigationService              navigationService,
        IGetExpenseCategoriesUseCase    getCategories,
        IDeleteExpenseCategoryUseCase   deleteCategory,
        Func<ExpenseCategoryFormWindow> formWindowFactory)
    {
        _navigationService  = navigationService;
        _getCategories      = getCategories;
        _deleteCategory     = deleteCategory;
        _formWindowFactory  = formWindowFactory;
    }

    partial void OnSelectedCategoryChanged(ExpenseCategory? value)
    {
        EditCategoryCommand.NotifyCanExecuteChanged();
        DeleteCategoryCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadCategoriesAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getCategories.ExecuteAsync(ct);
            Categories.Clear();
            foreach (var c in items) Categories.Add(c);
            HasCategories = Categories.Count > 0;
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
    private async Task AddCategoryAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadCategoriesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditCategoryAsync(CancellationToken ct = default)
    {
        if (SelectedCategory is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedCategory);
        if (window.ShowDialog() == true)
            await LoadCategoriesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteCategoryAsync(CancellationToken ct = default)
    {
        if (SelectedCategory is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa khoản mục chi phí '{SelectedCategory.Code} — {SelectedCategory.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteCategory.ExecuteAsync(SelectedCategory.Id, ct);
            Categories.Remove(SelectedCategory);
            SelectedCategory = null;
            HasCategories      = Categories.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
