// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Exceptions;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Backups.Domain.Models;
using DesktopLamour.Features.HomePage.Backups.Domain.UseCases;
using DesktopLamour.Features.HomePage.Backups.Views;
using DesktopLamour.Features.Realtime;
using System.Diagnostics;
using System.Linq;

namespace DesktopLamour.Features.HomePage.Backups.ViewModels;

public partial class BackupViewModel : ViewModelBase
{
    private readonly INavigationService           _navigationService;
    private readonly IGetBackupsUseCase           _getBackups;
    private readonly ICreateBackupUseCase         _createBackup;
    private readonly IDeleteBackupUseCase         _deleteBackup;
    private readonly IRestoreBackupUseCase        _restoreBackup;
    private readonly IGetBackupScheduleUseCase    _getSchedule;
    private readonly IUpdateBackupScheduleUseCase _updateSchedule;
    private readonly Func<RestoreConfirmWindow>   _restoreConfirmWindowFactory;
    private readonly IAuthTokenStorage            _tokenStorage;
    private readonly IPostLoginSyncService        _postLoginSync;

    [ObservableProperty] private bool        _isLoading;
    [ObservableProperty] private bool        _hasError;
    [ObservableProperty] private string      _errorMessage = string.Empty;
    [ObservableProperty] private bool        _hasBackups;
    [ObservableProperty] private BackupInfo? _selectedBackup;

    [ObservableProperty] private bool        _isScheduleEnabled;
    [ObservableProperty] private int         _scheduleHour = 2;
    [ObservableProperty] private int         _scheduleMinute;
    [ObservableProperty] private int         _scheduleIntervalDays = 1;
    [ObservableProperty] private int         _scheduleRetentionDays = 30;
    [ObservableProperty] private string      _scheduleDirectory = string.Empty;
    [ObservableProperty] private DateTime?   _scheduleLastRunAt;

    public ObservableCollection<BackupInfo> Backups { get; } = new();

    private bool HasSelection => SelectedBackup is not null;
    public bool HasScheduleLastRun => ScheduleLastRunAt.HasValue;

    partial void OnScheduleLastRunAtChanged(DateTime? value)
        => OnPropertyChanged(nameof(HasScheduleLastRun));

    public BackupViewModel(
        INavigationService           navigationService,
        IGetBackupsUseCase           getBackups,
        ICreateBackupUseCase         createBackup,
        IDeleteBackupUseCase         deleteBackup,
        IRestoreBackupUseCase        restoreBackup,
        IGetBackupScheduleUseCase    getSchedule,
        IUpdateBackupScheduleUseCase updateSchedule,
        Func<RestoreConfirmWindow>   restoreConfirmWindowFactory,
        IAuthTokenStorage            tokenStorage,
        IPostLoginSyncService        postLoginSync)
    {
        _navigationService           = navigationService;
        _getBackups                  = getBackups;
        _createBackup                = createBackup;
        _deleteBackup                = deleteBackup;
        _restoreBackup               = restoreBackup;
        _getSchedule                 = getSchedule;
        _updateSchedule              = updateSchedule;
        _restoreConfirmWindowFactory = restoreConfirmWindowFactory;
        _tokenStorage                = tokenStorage;
        _postLoginSync               = postLoginSync;
    }

    partial void OnSelectedBackupChanged(BackupInfo? value)
    {
        DeleteBackupCommand.NotifyCanExecuteChanged();
        RestoreBackupCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadBackupsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getBackups.ExecuteAsync(ct);
            Backups.Clear();
            foreach (var b in items) Backups.Add(b);
            HasBackups = Backups.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải danh sách bản sao lưu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task CreateBackupAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var created = await _createBackup.ExecuteAsync(ct);
            Backups.Insert(0, created);
            HasBackups = true;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Tạo bản sao lưu thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoadScheduleAsync(CancellationToken ct = default)
    {
        try
        {
            var schedule = await _getSchedule.ExecuteAsync(ct);
            IsScheduleEnabled     = schedule.IsEnabled;
            var parts             = schedule.TimeOfDay.Split(':');
            ScheduleHour          = int.TryParse(parts.ElementAtOrDefault(0), out var h) ? h : 2;
            ScheduleMinute        = int.TryParse(parts.ElementAtOrDefault(1), out var m) ? m : 0;
            ScheduleIntervalDays  = schedule.IntervalDays;
            ScheduleRetentionDays = schedule.RetentionDays;
            ScheduleDirectory     = schedule.Directory;
            ScheduleLastRunAt     = schedule.LastRunAt;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải cấu hình lịch tự động: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenDirectory()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName        = ScheduleDirectory,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể mở thư mục trên máy này: {ex.Message}\n\nLưu ý: thư mục này nằm trên máy chạy BE — chỉ mở được nếu WPF và BE đang chạy chung 1 máy.",
                "Không thể mở thư mục",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task SaveScheduleAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;
        IsLoading    = true;
        try
        {
            var input = new BackupSchedule
            {
                IsEnabled     = IsScheduleEnabled,
                TimeOfDay     = $"{ScheduleHour:D2}:{ScheduleMinute:D2}",
                IntervalDays  = ScheduleIntervalDays,
                RetentionDays = ScheduleRetentionDays,
                Directory     = ScheduleDirectory.Trim(),
            };
            var updated = await _updateSchedule.ExecuteAsync(input, ct);
            IsScheduleEnabled     = updated.IsEnabled;
            var parts             = updated.TimeOfDay.Split(':');
            ScheduleHour          = int.TryParse(parts.ElementAtOrDefault(0), out var h) ? h : 2;
            ScheduleMinute        = int.TryParse(parts.ElementAtOrDefault(1), out var m) ? m : 0;
            ScheduleIntervalDays  = updated.IntervalDays;
            ScheduleRetentionDays = updated.RetentionDays;
            ScheduleDirectory     = updated.Directory;
            ScheduleLastRunAt     = updated.LastRunAt;
            MessageBox.Show("Đã lưu cấu hình lịch tự động.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (ValidationException ex)
        {
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Lưu cấu hình thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteBackupAsync(CancellationToken ct = default)
    {
        if (SelectedBackup is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa bản sao lưu '{SelectedBackup.FileName}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteBackup.ExecuteAsync(SelectedBackup.FileName, ct);
            Backups.Remove(SelectedBackup);
            SelectedBackup = null;
            HasBackups      = Backups.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task RestoreBackupAsync(CancellationToken ct = default)
    {
        if (SelectedBackup is null) return;

        var window = _restoreConfirmWindowFactory();
        window.Initialize(SelectedBackup.FileName);
        if (window.ShowDialog() != true) return;

        IsLoading = true;
        try
        {
            await _restoreBackup.ExecuteAsync(SelectedBackup.FileName, window.Password, ct);
            MessageBox.Show(
                "Phục hồi dữ liệu thành công. Bạn sẽ được đăng xuất để tải lại dữ liệu mới.",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _tokenStorage.Clear();
            await _postLoginSync.ShutdownAsync();
            _navigationService.NavigateToLogin();
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Phục hồi thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
