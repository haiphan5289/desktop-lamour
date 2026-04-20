// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Domain.Models;
using DesktopLamour.Features.HomePage.Domain.UseCases;

namespace DesktopLamour.Features.HomePage.ViewModels;

public partial class SupplierFormViewModel : ViewModelBase
{
    private readonly ICreateSupplierUseCase _createUseCase;
    private readonly IUpdateSupplierUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle   = "Thêm nhà cung cấp";
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _errorMessage  = string.Empty;

    // Form fields
    [ObservableProperty] private string _code          = string.Empty;
    [ObservableProperty] private string _name          = string.Empty;
    [ObservableProperty] private string _phone         = string.Empty;
    [ObservableProperty] private string _address       = string.Empty;
    [ObservableProperty] private string _group         = string.Empty;
    [ObservableProperty] private string _taxCode       = string.Empty;
    [ObservableProperty] private bool   _isStopTracking;

    public bool IsAddMode => !_isEditMode;

    public event Action<bool>? RequestClose;

    public SupplierFormViewModel(
        ICreateSupplierUseCase createUseCase,
        IUpdateSupplierUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(Supplier? supplier)
    {
        ErrorMessage   = string.Empty;
        IsStopTracking = false;

        if (supplier is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm nhà cung cấp";
            Code = Name = Phone = Address = Group = TaxCode = string.Empty;
        }
        else
        {
            _isEditMode    = true;
            _editingId     = supplier.Id;
            WindowTitle    = "Sửa nhà cung cấp";
            Code           = supplier.Code;
            Name           = supplier.Name;
            Phone          = supplier.Phone;
            Address        = supplier.Address;
            Group          = supplier.Group;
            TaxCode        = supplier.TaxCode;
            IsStopTracking = supplier.IsStopTracking;
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
                var input = new CreateSupplierInput(
                    Code.Trim(), Name.Trim(), Phone.Trim(), Address.Trim(),
                    Group.Trim(), TaxCode.Trim(), IsStopTracking);
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateSupplierInput(
                    _editingId, Code.Trim(), Name.Trim(), Phone.Trim(), Address.Trim(),
                    Group.Trim(), TaxCode.Trim(), IsStopTracking);
                await _updateUseCase.ExecuteAsync(input, ct);
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
