// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Data.Services;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Customers.ViewModels;

public partial class CustomerFormViewModel : ViewModelBase
{
    private readonly ICreateCustomerUseCase _createUseCase;
    private readonly IUpdateCustomerUseCase _updateUseCase;
    private readonly ICustomerService       _customerService;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm khách hàng";
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Form fields
    [ObservableProperty] private string _code          = string.Empty;
    [ObservableProperty] private string _name          = string.Empty;
    [ObservableProperty] private string _phone         = string.Empty;
    [ObservableProperty] private string _address       = string.Empty;
    [ObservableProperty] private string _province      = string.Empty;
    [ObservableProperty] private string _customerGroup = string.Empty;
    [ObservableProperty] private string _taxCode       = string.Empty;
    [ObservableProperty] private string _saleCare      = string.Empty;

    public bool IsEditMode => _isEditMode;

    public event Action<bool>? RequestClose;

    public CustomerFormViewModel(
        ICreateCustomerUseCase createUseCase,
        IUpdateCustomerUseCase updateUseCase,
        ICustomerService       customerService)
    {
        _createUseCase   = createUseCase;
        _updateUseCase   = updateUseCase;
        _customerService = customerService;
    }

    public void Initialize(Customer? customer)
    {
        ErrorMessage = string.Empty;

        if (customer is null)
        {
            _isEditMode   = false;
            _editingId    = 0;
            WindowTitle   = "Thêm khách hàng";
            Code = Name = Phone = Address = Province = CustomerGroup = TaxCode = SaleCare = string.Empty;
        }
        else
        {
            _isEditMode   = true;
            _editingId    = customer.Id;
            WindowTitle   = "Sửa khách hàng";
            Code          = customer.Code;
            Name          = customer.Name;
            Phone         = customer.Phone;
            Address       = customer.Address;
            Province      = customer.Province;
            CustomerGroup = customer.CustomerGroup;
            TaxCode       = customer.TaxCode;
            SaleCare      = customer.SaleCare;
        }

        BeginDirtyTracking();
    }

    public async Task LoadNextCodeAsync(CancellationToken ct = default)
    {
        if (_isEditMode) return;
        try
        {
            Code = await _customerService.GetNextCodeAsync(ct);
        }
        catch { /* silently ignore — placeholder still shows */ }
        IsDirty = false;
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
                var input = new CreateCustomerInput(
                    Name.Trim(), Phone.Trim(), Address.Trim(),
                    Province.Trim(), CustomerGroup.Trim(), TaxCode.Trim(), SaleCare.Trim());
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateCustomerInput(
                    _editingId, Name.Trim(), Phone.Trim(), Address.Trim(),
                    Province.Trim(), CustomerGroup.Trim(), TaxCode.Trim(), SaleCare.Trim());
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
