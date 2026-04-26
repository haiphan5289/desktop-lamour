// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

namespace DesktopLamour.Features.HomePage.Employees.ViewModels;

public partial class EmployeeFormViewModel : ViewModelBase
{
    private readonly ICreateEmployeeUseCase _createUseCase;
    private readonly IUpdateEmployeeUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm nhân viên";
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Form fields
    [ObservableProperty] private string _name     = string.Empty;
    [ObservableProperty] private string _phone    = string.Empty;
    [ObservableProperty] private string _role     = "Cashier";
    [ObservableProperty] private string _unit     = "Spa";
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool   _isActive = true;

    public bool IsAddMode => !_isEditMode;

    public IReadOnlyList<string> Roles { get; } = new[] { "Admin", "Cashier", "Warehouse" };
    public IReadOnlyList<string> Units { get; } = new[] { "Spa", "PKD", "Spa", "GD", "Kho" };

    public event Action<bool>? RequestClose;

    public EmployeeFormViewModel(
        ICreateEmployeeUseCase createUseCase,
        IUpdateEmployeeUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(Employee? employee)
    {
        ErrorMessage = string.Empty;
        Password     = string.Empty;

        if (employee is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm nhân viên";
            Name = Phone = string.Empty;
            Role     = "Cashier";
            Unit     = "Spa";
            IsActive = true;
        }
        else
        {
            _isEditMode = true;
            _editingId  = employee.Id;
            WindowTitle = "Sửa nhân viên";
            Name        = employee.Name;
            Phone       = employee.Phone;
            Role        = employee.Role;
            Unit        = employee.Unit;
            IsActive    = employee.IsActive;
        }

        OnPropertyChanged(nameof(IsAddMode));
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
                var rawPwd = string.IsNullOrWhiteSpace(Password) ? Phone.Trim() : Password.Trim();
                await _createUseCase.ExecuteAsync(
                    new CreateEmployeeInput(Name.Trim(), Phone.Trim(), Role, Unit, rawPwd, IsActive), ct);
            }
            else
            {
                await _updateUseCase.ExecuteAsync(
                    new UpdateEmployeeInput(_editingId, Name.Trim(), Phone.Trim(), Role, Unit,
                        string.IsNullOrWhiteSpace(Password) ? null : Password.Trim(), IsActive), ct);
            }
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
    private void Cancel() => RequestClose?.Invoke(false);
}
