// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;
using DesktopLamour.Features.HomePage.SalesReturn.Views;

namespace DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

public partial class SalesReturnListViewModel : ViewModelBase
{
    private readonly INavigationService           _navigationService;
    private readonly IGetSalesReturnsUseCase      _getReturns;
    private readonly IDeleteSalesReturnUseCase    _deleteReturn;
    private readonly Func<SalesReturnWindow>      _formWindowFactory;

    [ObservableProperty] private bool                  _isLoading;
    [ObservableProperty] private bool                  _hasError;
    [ObservableProperty] private string                _errorMessage    = string.Empty;
    [ObservableProperty] private bool                  _hasSalesReturns;
    [ObservableProperty] private SalesReturnListItem?  _selectedReturn;
    [ObservableProperty] private DateTime?             _filterFromDate;
    [ObservableProperty] private DateTime?             _filterToDate;

    // 1 ô tìm kiếm chung (AND với FilterFromDate/FilterToDate ở trên) — khớp OR trên các trường
    // text chính, không phân biệt hoa/thường.
    [ObservableProperty] private string _searchText = string.Empty;

    private readonly List<SalesReturnListItem> _allItems = new();

    public ObservableCollection<SalesReturnListItem> SalesReturns { get; } = new();

    private bool HasSelection => SelectedReturn is not null;

    public SalesReturnListViewModel(
        INavigationService          navigationService,
        IGetSalesReturnsUseCase     getReturns,
        IDeleteSalesReturnUseCase   deleteReturn,
        Func<SalesReturnWindow>     formWindowFactory)
    {
        _navigationService = navigationService;
        _getReturns        = getReturns;
        _deleteReturn      = deleteReturn;
        _formWindowFactory = formWindowFactory;
    }

    partial void OnSelectedReturnChanged(SalesReturnListItem? value)
    {
        EditSalesReturnCommand.NotifyCanExecuteChanged();
        DeleteSalesReturnCommand.NotifyCanExecuteChanged();
    }

    partial void OnFilterFromDateChanged(DateTime? value) => ApplyFilter();
    partial void OnFilterToDateChanged(DateTime? value)   => ApplyFilter();
    partial void OnSearchTextChanged(string value)        => ApplyFilter();

    // Lọc đã tự áp dụng ngay khi đổi ngày/tìm kiếm (live filter) — nút "Lọc" chỉ để người dùng có
    // affordance rõ ràng để bấm, giống hàng lọc màn Chứng từ bán hàng.
    [RelayCommand]
    private void Filter() => ApplyFilter();

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadSalesReturnsAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getReturns.ExecuteAsync(ct);
            _allItems.Clear();
            foreach (var dto in items)
                _allItems.Add(SalesReturnListItem.FromDto(dto));
            ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task AddSalesReturnAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditSalesReturnAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        var window = _formWindowFactory();
        window.Initialize(SelectedReturn.Original);
        if (window.ShowDialog() == true)
            await LoadSalesReturnsCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSalesReturnAsync(CancellationToken ct = default)
    {
        if (SelectedReturn is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{SelectedReturn.DocumentNumber}'?\nTồn kho sẽ được trừ lại.",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteReturn.ExecuteAsync(SelectedReturn.Id, ct);
            _allItems.Remove(SelectedReturn);
            ApplyFilter();
            SelectedReturn = null;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    private void ApplyFilter()
    {
        var filtered = _allItems.AsEnumerable();

        if (FilterFromDate.HasValue)
            filtered = filtered.Where(o => o.DocumentDate.Date >= FilterFromDate.Value.Date);

        if (FilterToDate.HasValue)
            filtered = filtered.Where(o => o.DocumentDate.Date <= FilterToDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(o =>
                Matches(o.ReturnTypeLabel, SearchText) ||
                Matches(o.DocumentNumber, SearchText) ||
                Matches(o.CustomerName, SearchText) ||
                Matches(o.EmployeeName, SearchText));

        SalesReturns.Clear();
        foreach (var item in filtered.OrderByDescending(o => o.DocumentDate))
            SalesReturns.Add(item);

        HasSalesReturns = SalesReturns.Count > 0;
    }

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));
}
