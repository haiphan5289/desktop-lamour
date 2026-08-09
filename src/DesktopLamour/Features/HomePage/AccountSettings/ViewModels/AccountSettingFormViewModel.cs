// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.Models;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
using System.Windows;

namespace DesktopLamour.Features.HomePage.AccountSettings.ViewModels;

public partial class AccountSettingFormViewModel : ViewModelBase
{
    private readonly ICreateAccountSettingUseCase _createUseCase;
    private readonly IUpdateAccountSettingUseCase _updateUseCase;

    private bool _isEditMode;
    private int  _editingId;

    [ObservableProperty] private string _windowTitle  = "Thêm tài khoản";
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private string  _code         = string.Empty;
    [ObservableProperty] private string  _description  = string.Empty;

    public event Action<bool>? RequestClose;

    public AccountSettingFormViewModel(ICreateAccountSettingUseCase createUseCase, IUpdateAccountSettingUseCase updateUseCase)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
    }

    public void Initialize(AccountSetting? account = null)
    {
        ErrorMessage = string.Empty;

        if (account is null)
        {
            _isEditMode = false;
            _editingId  = 0;
            WindowTitle = "Thêm tài khoản";
            Code        = string.Empty;
            Description = string.Empty;
        }
        else
        {
            _isEditMode = true;
            _editingId  = account.Id;
            WindowTitle = "Sửa tài khoản";
            Code        = account.Code;
            Description = account.Description;
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
                var input = new CreateAccountSettingInput(Code.Trim(), Description.Trim());
                await _createUseCase.ExecuteAsync(input, ct);
            }
            else
            {
                var input = new UpdateAccountSettingInput(_editingId, Code.Trim(), Description.Trim());
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
