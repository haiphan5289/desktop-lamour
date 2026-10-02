// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.Accounting.ViewModels;

// Hộp "Chọn tham số" của báo cáo "Sổ kế toán chi tiết quỹ tiền mặt" (khớp MISA): Kỳ báo cáo + Từ/Đến,
// bảng tài khoản tiền mặt có ô tick, 2 tuỳ chọn Cộng gộp / Sắp xếp theo thứ tự lập.
public partial class CashLedgerReportFilterViewModel : ViewModelBase
{
    private const string CashAccountPrefix = "111";

    private readonly IGetAccountSettingsUseCase _getAccountSettings;
    private readonly ILogger<CashLedgerReportFilterViewModel> _logger;

    // Tham số của lần xem trước (mở lại từ nút "Chọn tham số" trên báo cáo) — áp lại sau khi nạp danh mục.
    private CashLedgerReportFilter? _previous;
    private bool _syncingSelectAll;
    // true khi chính code đang gán Kỳ/Từ/Đến — để việc đổi ngày do chọn kỳ không bị coi là người dùng sửa tay.
    private bool _applyingPeriod;

    [ObservableProperty] private bool      _isLoading;
    [ObservableProperty] private bool      _dialogResult;
    [ObservableProperty] private bool      _hasError;
    [ObservableProperty] private string    _errorMessage = string.Empty;
    [ObservableProperty] private string    _selectedPeriod = CashLedgerReportPeriods.ThisMonth;
    [ObservableProperty] private DateTime? _fromDate;
    [ObservableProperty] private DateTime? _toDate;
    [ObservableProperty] private bool      _areAllAccountsSelected = true;
    [ObservableProperty] private bool      _mergeSimilar;
    [ObservableProperty] private bool      _orderByCreated;

    public IReadOnlyList<string> Periods { get; } = CashLedgerReportPeriods.All;
    public ObservableCollection<CashAccountCheckItem> Accounts { get; } = new();

    public CashLedgerReportFilterViewModel(
        IGetAccountSettingsUseCase getAccountSettings,
        ILogger<CashLedgerReportFilterViewModel> logger)
    {
        _getAccountSettings = getAccountSettings;
        _logger             = logger;
        ApplyPeriod(SelectedPeriod);
    }

    // Gọi TRƯỚC khi ShowDialog để hộp mở lại đúng tham số đang xem.
    public void Initialize(CashLedgerReportFilter? previous)
    {
        _previous = previous;
        if (previous is null) return;

        var wasApplying = _applyingPeriod;
        _applyingPeriod = true;
        SelectedPeriod  = previous.Period;
        FromDate        = previous.FromDate;    // gán sau SelectedPeriod: kỳ "Tùy chọn" giữ đúng ngày đã nhập
        ToDate          = previous.ToDate;
        _applyingPeriod = wasApplying;
        MergeSimilar   = previous.MergeSimilar;
        OrderByCreated = previous.OrderByCreated;
    }

    [RelayCommand]
    private async Task LoadLookupsAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            var accounts = await _getAccountSettings.ExecuteAsync(ct);

            foreach (var item in Accounts) item.PropertyChanged -= OnAccountChanged;
            Accounts.Clear();
            foreach (var a in accounts.Where(a => a.Code.StartsWith(CashAccountPrefix, StringComparison.Ordinal))
                                      .OrderBy(a => a.Code, StringComparer.Ordinal))
            {
                var item = new CashAccountCheckItem
                {
                    Code       = a.Code,
                    Name       = a.Description,
                    IsSelected = _previous is null || _previous.AccountCodes.Count == 0 || _previous.AccountCodes.Contains(a.Code),
                };
                item.PropertyChanged += OnAccountChanged;
                Accounts.Add(item);
            }
            SyncSelectAll();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cash accounts for report filter");
            HasError     = true;
            ErrorMessage = $"Không tải được danh mục tài khoản: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    partial void OnSelectedPeriodChanged(string value) => ApplyPeriod(value);

    private void ApplyPeriod(string period)
    {
        if (CashLedgerReportPeriods.Range(period, DateTime.Today) is not { } range) return;
        var wasApplying = _applyingPeriod;
        _applyingPeriod = true;
        FromDate = range.From;
        ToDate   = range.To;
        _applyingPeriod = wasApplying;
    }

    // Khớp MISA: tự sửa Từ ngày / Đến ngày thì ô Kỳ báo cáo chuyển sang "Tùy chọn" (khoảng ngày không còn
    // là kỳ đang chọn). Chọn lại 1 kỳ có sẵn thì ngày tự đặt lại theo kỳ đó.
    partial void OnFromDateChanged(DateTime? value) => SwitchToCustomIfEdited();
    partial void OnToDateChanged(DateTime? value)   => SwitchToCustomIfEdited();

    private void SwitchToCustomIfEdited()
    {
        if (_applyingPeriod || SelectedPeriod == CashLedgerReportPeriods.Custom) return;
        if (CashLedgerReportPeriods.Range(SelectedPeriod, DateTime.Today) is { } range
            && FromDate?.Date == range.From && ToDate?.Date == range.To)
            return;
        SelectedPeriod = CashLedgerReportPeriods.Custom;   // Range("Tùy chọn") = null nên không ghi đè ngày vừa nhập
    }

    // Ô tick ở tiêu đề cột: tick/bỏ tick tất cả.
    partial void OnAreAllAccountsSelectedChanged(bool value)
    {
        if (_syncingSelectAll) return;
        foreach (var item in Accounts) item.IsSelected = value;
    }

    private void OnAccountChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CashAccountCheckItem.IsSelected)) SyncSelectAll();
    }

    private void SyncSelectAll()
    {
        _syncingSelectAll = true;
        AreAllAccountsSelected = Accounts.Count > 0 && Accounts.All(a => a.IsSelected);
        _syncingSelectAll = false;
    }

    [RelayCommand]
    private void Submit()
    {
        HasError = false;
        if (FromDate is null || ToDate is null)
        {
            HasError = true; ErrorMessage = "Vui lòng chọn Từ ngày và Đến ngày."; return;
        }
        if (FromDate > ToDate)
        {
            HasError = true; ErrorMessage = "Từ ngày phải trước hoặc bằng Đến ngày."; return;
        }
        if (Accounts.Count > 0 && !Accounts.Any(a => a.IsSelected))
        {
            HasError = true; ErrorMessage = "Vui lòng chọn ít nhất một tài khoản."; return;
        }
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    // "Xóa điều kiện": về mặc định (Tháng này, tick hết tài khoản, bỏ 2 tuỳ chọn).
    [RelayCommand]
    private void ClearFilters()
    {
        HasError       = false;
        SelectedPeriod = CashLedgerReportPeriods.ThisMonth;
        ApplyPeriod(SelectedPeriod);   // đã ở "Tháng này" thì SelectedPeriod không đổi → vẫn phải đặt lại ngày
        MergeSimilar   = false;
        OrderByCreated = false;
        foreach (var item in Accounts) item.IsSelected = true;
    }

    public CashLedgerReportFilter BuildFilter() => new()
    {
        Period         = SelectedPeriod,
        FromDate       = (FromDate ?? DateTime.Today).Date,
        ToDate         = (ToDate ?? DateTime.Today).Date,
        // Tick hết = không lọc (BE lấy mọi TK tiền mặt, kể cả TK thêm vào danh mục sau này).
        AccountCodes   = Accounts.All(a => a.IsSelected)
            ? Array.Empty<string>()
            : Accounts.Where(a => a.IsSelected).Select(a => a.Code).ToArray(),
        MergeSimilar   = MergeSimilar,
        OrderByCreated = OrderByCreated,
    };
}
