// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouses.ViewModels;

public partial class DepartmentFormViewModel : ViewModelBase
{
    private readonly ICreateDepartmentUseCase _createUseCase;
    private readonly IUpdateDepartmentUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm phòng ban";
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _name          = string.Empty;

    public event Action<bool>? RequestClose;

    public DepartmentFormViewModel(ICreateDepartmentUseCase createUseCase, IUpdateDepartmentUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(Department? department = null)
    {
        ErrorMessage = string.Empty;

        if (department is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm phòng ban";
            Name        = string.Empty;
        }
        else
        {
            _isEditMode = true;
            _editingId  = department.Id;
            WindowTitle = "Sửa phòng ban";
            Name        = department.Name;
        }

        BeginDirtyTracking();
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
                var input = new CreateDepartmentInput(Name.Trim());
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateDepartmentInput(_editingId, Name.Trim());
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
