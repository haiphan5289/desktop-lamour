// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
using System.Windows;

namespace DesktopLamour.Features.HomePage.ProductUnits.ViewModels;

public partial class ProductUnitFormViewModel : ViewModelBase
{
    private readonly ICreateProductUnitUseCase _createUseCase;
    private readonly IUpdateProductUnitUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm đơn vị tính";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private string  _name         = string.Empty;

    public event Action<bool>? RequestClose;

    public ProductUnitFormViewModel(ICreateProductUnitUseCase createUseCase, IUpdateProductUnitUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(ProductUnit? unit = null)
    {
        ErrorMessage = string.Empty;

        if (unit is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm đơn vị tính";
            Name        = string.Empty;
        }
        else
        {
            _isEditMode = true;
            _editingId  = unit.Id;
            WindowTitle = "Sửa đơn vị tính";
            Name        = unit.Name;
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
                var input = new CreateProductUnitInput(Name.Trim());
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateProductUnitInput(_editingId, Name.Trim());
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
