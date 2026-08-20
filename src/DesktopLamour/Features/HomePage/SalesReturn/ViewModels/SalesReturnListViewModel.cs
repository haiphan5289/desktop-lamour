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
using DesktopLamour.Shared.Utilities;

namespace DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

public partial class SalesReturnListViewModel : ViewModelBase
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(400);

    private readonly INavigationService           _navigationService;
    private readonly IGetSalesReturnsUseCase      _getReturns;
    private readonly IDeleteSalesReturnUseCase    _deleteReturn;
    private readonly Func<SalesReturnWindow>      _formWindowFactory;
    private readonly DebounceDispatcher           _searchDebounce = new();

    [ObservableProperty] private bool                  _isLoading;
    [ObservableProperty] private bool                  _hasError;
    [ObservableProperty] private string                _errorMessage    = string.Empty;
    [ObservableProperty] private bool                  _hasSalesReturns;
    [ObservableProperty] private SalesReturnListItem?  _selectedReturn;
    [ObservableProperty] private DateTime?             _filterFromDate;
    [ObservableProperty] private DateTime?             _filterToDate;

    // 1 ô tìm kiếm chung (AND với FilterFromDate/FilterToDate ở trên) — khớp OR trên các trường
    // text chính, không phân biệt hoa/thường. Lọc chạy dưới SQL (server-side) — xem LoadSalesReturnsAsync.
    [ObservableProperty] private string _searchText = string.Empty;

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

    // Đổi ngày là thao tác rời rạc (không gõ liên tục như SearchText) — reload ngay, không debounce.
    partial void OnFilterFromDateChanged(DateTime? value) => _ = LoadSalesReturnsCommand.ExecuteAsync(null);
    partial void OnFilterToDateChanged(DateTime? value)   => _ = LoadSalesReturnsCommand.ExecuteAsync(null);

    // Lọc giờ chạy dưới SQL (server-side) thay vì trong RAM — gõ liên tục sẽ bắn 1 HTTP request mỗi
    // ký tự nếu không debounce. Chờ người dùng ngừng gõ 400ms rồi mới gọi lại API.
    partial void OnSearchTextChanged(string value)
        => _searchDebounce.Debounce(SearchDebounceDelay, ct => LoadSalesReturnsAsync(ct));

    // Lọc đã tự áp dụng ngay khi đổi ngày/tìm kiếm (live filter) — nút "Lọc" chỉ để người dùng có
    // affordance rõ ràng để bấm, giống hàng lọc màn Chứng từ bán hàng.
    [RelayCommand]
    private async Task Filter() => await LoadSalesReturnsAsync();

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
            var items = await _getReturns.ExecuteAsync(FilterFromDate, FilterToDate, SearchText, ct);

            SalesReturns.Clear();
            foreach (var dto in items
                         .Select(SalesReturnListItem.FromDto)
                         .OrderByDescending(o => o.DocumentDate))
                SalesReturns.Add(dto);

            HasSalesReturns = SalesReturns.Count > 0;
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

        try
        {
            await _deleteReturn.ExecuteAsync(SelectedReturn.Id, ct);
            SelectedReturn = null;
            await LoadSalesReturnsAsync(ct); // tự quản lý IsLoading — reload theo đúng filter đang xem
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
    }
}
