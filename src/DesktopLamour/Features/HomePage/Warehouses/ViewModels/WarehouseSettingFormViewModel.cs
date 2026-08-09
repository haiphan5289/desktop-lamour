// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouses.ViewModels;

public partial class WarehouseSettingFormViewModel : ViewModelBase
{
    private readonly ICreateWarehouseSettingUseCase _createUseCase;
    private readonly IUpdateWarehouseSettingUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm kho";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private string  _code         = string.Empty;
    [ObservableProperty] private string  _name         = string.Empty;
    [ObservableProperty] private bool    _isActive     = true;

    public event Action<bool>? RequestClose;

    public WarehouseSettingFormViewModel(ICreateWarehouseSettingUseCase createUseCase, IUpdateWarehouseSettingUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(WarehouseSetting? warehouse = null)
    {
        ErrorMessage = string.Empty;

        if (warehouse is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm kho";
            Code        = string.Empty;
            Name        = string.Empty;
            IsActive    = true;
        }
        else
        {
            _isEditMode = true;
            _editingId  = warehouse.Id;
            WindowTitle = "Sửa kho";
            Code        = warehouse.Code;
            Name        = warehouse.Name;
            IsActive    = warehouse.IsActive;
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
                var input = new CreateWarehouseSettingInput(Code.Trim(), Name.Trim(), IsActive);
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateWarehouseSettingInput(_editingId, Code.Trim(), Name.Trim(), IsActive);
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
