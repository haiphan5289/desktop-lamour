// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Data.Services;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Customers.ViewModels;

public partial class CustomerFormViewModel : ViewModelBase
{
    private readonly ICreateCustomerUseCase  _createUseCase;
    private readonly IUpdateCustomerUseCase  _updateUseCase;
    private readonly ICustomerService        _customerService;
    private readonly IGetEmployeesUseCase    _getEmployees;
    private readonly ILogger<CustomerFormViewModel> _logger;

    private bool   _isEditMode;
    private int    _editingId;
    private string _initialSaleCare = string.Empty;

    [ObservableProperty] private string          _windowTitle  = "Thêm khách hàng";
    [ObservableProperty] private bool            _isLoading;
    [ObservableProperty] private string          _errorMessage = string.Empty;

    // Form fields
    [ObservableProperty] private string           _code          = string.Empty;
    [ObservableProperty] private string           _name          = string.Empty;
    [ObservableProperty] private string           _phone         = string.Empty;
    [ObservableProperty] private string           _address       = string.Empty;
    [ObservableProperty] private string           _province      = string.Empty;
    [ObservableProperty] private string           _customerGroup = string.Empty;
    [ObservableProperty] private string           _taxCode       = string.Empty;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();

    public bool IsEditMode => _isEditMode;

    public event Action<bool>? RequestClose;

    public CustomerFormViewModel(
        ICreateCustomerUseCase         createUseCase,
        IUpdateCustomerUseCase         updateUseCase,
        ICustomerService               customerService,
        IGetEmployeesUseCase           getEmployees,
        ILogger<CustomerFormViewModel> logger)
    {
        _createUseCase   = createUseCase;
        _updateUseCase   = updateUseCase;
        _customerService = customerService;
        _getEmployees    = getEmployees;
        _logger          = logger;
    }

    public void Initialize(Customer? customer)
    {
        ErrorMessage     = string.Empty;
        SelectedEmployee = null;

        if (customer is null)
        {
            _isEditMode      = false;
            _editingId       = 0;
            _initialSaleCare = string.Empty;
            WindowTitle      = "Thêm khách hàng";
            Code = Name = Phone = Address = Province = CustomerGroup = TaxCode = string.Empty;
        }
        else
        {
            _isEditMode      = true;
            _editingId       = customer.Id;
            _initialSaleCare = customer.SaleCare;
            WindowTitle      = "Sửa khách hàng";
            Code          = customer.Code;
            Name          = customer.Name;
            Phone         = customer.Phone;
            Address       = customer.Address;
            Province      = customer.Province;
            CustomerGroup = customer.CustomerGroup;
            TaxCode       = customer.TaxCode;
        }

        BeginDirtyTracking();
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var employees = await _getEmployees.ExecuteAsync(ct);
            Employees = employees.Where(e => e.IsActive).Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));

            if (!string.IsNullOrWhiteSpace(_initialSaleCare))
                SelectedEmployee = Employees.FirstOrDefault(e =>
                    string.Equals(e.Name, _initialSaleCare, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load employees for CustomerFormWindow");
        }

        if (!_isEditMode)
        {
            try
            {
                Code = await _customerService.GetNextCodeAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load next customer code");
            }
        }

        IsDirty = false;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        ErrorMessage = string.Empty;
        IsLoading    = true;
        var saleCare = SelectedEmployee?.Name?.Trim() ?? string.Empty;
        try
        {
            if (!_isEditMode)
            {
                var input = new CreateCustomerInput(
                    Name.Trim(), Phone.Trim(), Address.Trim(),
                    Province.Trim(), CustomerGroup.Trim(), TaxCode.Trim(), saleCare);
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateCustomerInput(
                    _editingId, Name.Trim(), Phone.Trim(), Address.Trim(),
                    Province.Trim(), CustomerGroup.Trim(), TaxCode.Trim(), saleCare);
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
