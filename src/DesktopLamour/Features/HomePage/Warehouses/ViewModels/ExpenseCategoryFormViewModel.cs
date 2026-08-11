// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Views;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouses.ViewModels;

public partial class ExpenseCategoryFormViewModel : ViewModelBase
{
    private readonly ICreateExpenseCategoryUseCase _createUseCase;
    private readonly IUpdateExpenseCategoryUseCase _updateUseCase;
    private readonly IGetDepartmentsUseCase        _getDepartments;
    private readonly Func<DepartmentFormWindow>    _departmentFormWindowFactory;
    private readonly ILogger<ExpenseCategoryFormViewModel> _logger;

    private bool _isEditMode;
    private int  _editingId;
    private int? _pendingDepartmentId;

    [ObservableProperty] private string  _windowTitle  = "Thêm khoản mục chi phí";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private string  _code         = string.Empty;
    [ObservableProperty] private string  _name         = string.Empty;
    [ObservableProperty] private ISearchableItem? _selectedDepartment;
    [ObservableProperty] private string? _description;

    public IReadOnlyList<ISearchableItem> Departments { get; private set; } = Array.Empty<ISearchableItem>();

    public event Action<bool>? RequestClose;

    public ExpenseCategoryFormViewModel(
        ICreateExpenseCategoryUseCase createUseCase,
        IUpdateExpenseCategoryUseCase updateUseCase,
        IGetDepartmentsUseCase getDepartments,
        Func<DepartmentFormWindow> departmentFormWindowFactory,
        ILogger<ExpenseCategoryFormViewModel> logger)
    {
        _createUseCase               = createUseCase;
        _updateUseCase               = updateUseCase;
        _getDepartments              = getDepartments;
        _departmentFormWindowFactory = departmentFormWindowFactory;
        _logger                      = logger;
    }

    public void Initialize(ExpenseCategory? category = null)
    {
        ErrorMessage = string.Empty;

        if (category is null)
        {
            _isEditMode          = false;
            _editingId            = 0;
            _pendingDepartmentId  = null;
            WindowTitle           = "Thêm khoản mục chi phí";
            Code                  = string.Empty;
            Name                  = string.Empty;
            SelectedDepartment    = null;
            Description           = null;
        }
        else
        {
            _isEditMode          = true;
            _editingId            = category.Id;
            _pendingDepartmentId  = category.DepartmentId;
            WindowTitle           = "Sửa khoản mục chi phí";
            Code                  = category.Code;
            Name                  = category.Name;
            Description           = category.Description;
        }

        BeginDirtyTracking();
        _ = LoadDepartmentsAsync();
    }

    private async Task LoadDepartmentsAsync(CancellationToken ct = default)
    {
        try
        {
            var departments = await _getDepartments.ExecuteAsync(ct);
            Departments = departments.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Departments));
            if (_pendingDepartmentId is > 0)
                SelectedDepartment = Departments.FirstOrDefault(d => d.Id == _pendingDepartmentId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load departments for expense category form");
        }
    }

    [RelayCommand]
    private async Task AddDepartmentAsync(CancellationToken ct = default)
    {
        var before = Departments.Select(d => d.Id).ToHashSet();
        var window = _departmentFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var departments = await _getDepartments.ExecuteAsync(ct);
            Departments = departments.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Departments));
            var newItem = Departments.FirstOrDefault(d => !before.Contains(d.Id));
            if (newItem is not null) SelectedDepartment = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload departments after add"); }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        ErrorMessage = string.Empty;
        IsLoading    = true;
        try
        {
            if (!_isEditMode)
            {
                var input = new CreateExpenseCategoryInput(Code.Trim(), Name.Trim(), SelectedDepartment?.Id, Description?.Trim());
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateExpenseCategoryInput(_editingId, Code.Trim(), Name.Trim(), SelectedDepartment?.Id, Description?.Trim());
                await _updateUseCase.ExecuteAsync(input, ct);
            }
            StopDirtyTracking();
            RequestClose?.Invoke(true);
        }
        catch (ValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Lưu thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsDirty)
        {
            var r = MessageBox.Show(
                "Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;
        }
        StopDirtyTracking();
        RequestClose?.Invoke(false);
    }
}
