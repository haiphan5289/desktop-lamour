// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Categories.ViewModels;

public partial class CategoryFormViewModel : ViewModelBase
{
    private readonly ICreateCategoryUseCase _createUseCase;
    private readonly IUpdateCategoryUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm danh mục";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private string  _name         = string.Empty;

    public event Action<bool>? RequestClose;

    public CategoryFormViewModel(ICreateCategoryUseCase createUseCase, IUpdateCategoryUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(Category? category = null)
    {
        ErrorMessage = string.Empty;

        if (category is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm danh mục";
            Name        = string.Empty;
        }
        else
        {
            _isEditMode = true;
            _editingId  = category.Id;
            WindowTitle = "Sửa danh mục";
            Name        = category.Name;
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
                var input = new CreateCategoryInput(Name.Trim());
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateCategoryInput(_editingId, Name.Trim());
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
