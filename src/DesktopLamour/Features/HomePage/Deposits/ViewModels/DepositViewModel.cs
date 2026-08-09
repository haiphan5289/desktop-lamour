// Copyright © 2026 DesktopLamour. All rights reserved.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Deposits.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Deposits.ViewModels;

public partial class DepositViewModel : ViewModelBase
{
    public event Action? DepositSaved;
    public event Action? RequestClose;

    private readonly IGetDepositsUseCase        _getDeposits;
    private readonly IGetNextDepositCodeUseCase _getNextCode;
    private readonly ICreateDepositUseCase      _createDeposit;
    private readonly IUpdateDepositUseCase      _updateDeposit;
    private readonly IDeleteDepositUseCase      _deleteDeposit;
    private readonly IGetCustomersUseCase       _getCustomers;
    private readonly IGetEmployeesUseCase       _getEmployees;
    private readonly ILogger<DepositViewModel>  _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Header — Thông tin chung ──────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;
    [ObservableProperty] private string?          _description;
    [ObservableProperty] private string?          _reference;
    [ObservableProperty] private decimal          _amount;

    // ── Chứng từ ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "DC00001";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private DepositResponseDto? _currentDeposit;
    [ObservableProperty] private decimal             _remainingBalance;

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();

    private bool CanEditAmount => CurrentDeposit is null || CurrentDeposit.RemainingBalance == CurrentDeposit.Amount;

    private List<DepositResponseDto> _depositListCache = new();
    private int _currentIndex = -1;

    public DepositViewModel(
        IGetDepositsUseCase        getDeposits,
        IGetNextDepositCodeUseCase getNextCode,
        ICreateDepositUseCase      createDeposit,
        IUpdateDepositUseCase      updateDeposit,
        IDeleteDepositUseCase      deleteDeposit,
        IGetCustomersUseCase       getCustomers,
        IGetEmployeesUseCase       getEmployees,
        ILogger<DepositViewModel>  logger)
    {
        _getDeposits    = getDeposits;
        _getNextCode    = getNextCode;
        _createDeposit  = createDeposit;
        _updateDeposit  = updateDeposit;
        _deleteDeposit  = deleteDeposit;
        _getCustomers   = getCustomers;
        _getEmployees   = getEmployees;
        _logger         = logger;
    }

    // ── Init ──────────────────────────────────────────────────────────────

    public async Task LoadAsync(CancellationToken ct = default)
    {
        await LoadLookupsAsync(ct);
        await LoadDepositsAsync(ct);
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            var employees = await _getEmployees.ExecuteAsync(ct);

            Customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();

            OnPropertyChanged(nameof(Customers));
            OnPropertyChanged(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload lookup data for DepositWindow");
        }
    }

    [RelayCommand]
    private async Task LoadAsync2(CancellationToken ct = default)
        => await LoadDepositsAsync(ct);

    private async Task LoadDepositsAsync(CancellationToken ct)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            var list = await _getDeposits.ExecuteAsync(ct);
            _depositListCache = list.OrderByDescending(d => d.CreatedAt).ToList();

            if (_depositListCache.Count > 0)
            {
                _currentIndex  = 0;
                CurrentDeposit = _depositListCache[0];
                PopulateFormFromCurrent();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deposits");
            HasError     = true;
            ErrorMessage = $"Không thể tải danh sách cọc: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddNewAsync(CancellationToken ct = default)
    {
        CurrentDeposit = null;
        _currentIndex  = -1;
        ClearForm();
        DocumentNumber = await _getNextCode.ExecuteAsync(ct);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        if (SelectedCustomer is null)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng chọn khách hàng.";
            return;
        }

        if (Amount <= 0)
        {
            HasError     = true;
            ErrorMessage = "Số tiền cọc phải lớn hơn 0.";
            return;
        }

        IsBusy = true;
        try
        {
            if (CurrentDeposit is null)
            {
                var request = BuildCreateRequest();
                var result  = await _createDeposit.ExecuteAsync(request, ct);
                _logger.LogInformation("Deposit created: {DocumentNumber}", result.DocumentNumber);
                await LoadDepositsAsync(ct);
                NavigateToDeposit(result.Id);
            }
            else
            {
                var request = BuildUpdateRequest();
                var result  = await _updateDeposit.ExecuteAsync(CurrentDeposit.Id, request, ct);
                _logger.LogInformation("Deposit updated: {Id}", result.Id);
                await LoadDepositsAsync(ct);
                NavigateToDeposit(result.Id);
            }

            DepositSaved?.Invoke();
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save deposit");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentDeposit is null) return;

        IsBusy = true;
        try
        {
            await _deleteDeposit.ExecuteAsync(CurrentDeposit.Id, ct);
            _logger.LogInformation("Deposit deleted: {Id}", CurrentDeposit.Id);
            await LoadDepositsAsync(ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete deposit");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void NavigatePrev()
    {
        if (_depositListCache.Count == 0 || _currentIndex <= 0) return;
        _currentIndex--;
        CurrentDeposit = _depositListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand]
    private void NavigateNext()
    {
        if (_depositListCache.Count == 0 || _currentIndex >= _depositListCache.Count - 1) return;
        _currentIndex++;
        CurrentDeposit = _depositListCache[_currentIndex];
        PopulateFormFromCurrent();
    }

    [RelayCommand]
    private async Task CancelAsync(CancellationToken ct = default)
    {
        HasError = false;
        await LoadDepositsAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void ClearForm()
    {
        SelectedCustomer  = null;
        SelectedEmployee  = null;
        Description       = null;
        Reference         = null;
        Amount            = 0;
        RemainingBalance  = 0;
        AccountingDate    = DateTime.Today;
        DocumentDate      = DateTime.Today;
        DocumentNumber    = "DC00001";
    }

    private void PopulateFormFromCurrent()
    {
        if (CurrentDeposit is null) return;

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentDeposit.CustomerId);
        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == CurrentDeposit.EmployeeId);
        Description      = CurrentDeposit.Description;
        Reference         = CurrentDeposit.Reference;
        Amount            = CurrentDeposit.Amount;
        RemainingBalance  = CurrentDeposit.RemainingBalance;
        AccountingDate    = CurrentDeposit.AccountingDate.ToLocalTime();
        DocumentDate      = CurrentDeposit.DocumentDate.ToLocalTime();
        DocumentNumber    = CurrentDeposit.DocumentNumber;
    }

    private CreateDepositRequestDto BuildCreateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)   ? null : Reference.Trim(),
        Amount         = Amount,
    };

    private UpdateDepositRequestDto BuildUpdateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)   ? null : Reference.Trim(),
        Amount         = Amount,
    };

    private void NavigateToDeposit(int id)
    {
        var idx = _depositListCache.FindIndex(d => d.Id == id);
        if (idx >= 0)
        {
            _currentIndex  = idx;
            CurrentDeposit = _depositListCache[idx];
            PopulateFormFromCurrent();
        }
    }
}
