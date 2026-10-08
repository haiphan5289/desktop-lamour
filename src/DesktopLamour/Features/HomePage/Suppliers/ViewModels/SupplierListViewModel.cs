// Copyright © 2026 DesktopLamour. All rights reserved.
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Suppliers.Views;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Suppliers.ViewModels;

public partial class SupplierListViewModel : ViewModelBase
{
    private readonly INavigationService        _navigationService;
    private readonly IGetSuppliersUseCase      _getSuppliers;
    private readonly IDeleteSupplierUseCase    _deleteSupplier;
    private readonly IDuplicateSupplierUseCase _duplicateSupplier;
    private readonly IImportExcelSuppliersUseCase _importExcel;
    private readonly Func<SupplierFormWindow>  _formWindowFactory;

    [ObservableProperty] private bool      _isLoading;
    [ObservableProperty] private bool      _hasError;
    [ObservableProperty] private string    _errorMessage = string.Empty;
    [ObservableProperty] private bool      _hasSuppliers;
    [ObservableProperty] private Supplier? _selectedSupplier;

    // ── Filter theo từng cột — nhúng ngay trong header lưới (khớp UI MISA), AND với nhau và với
    // radio Tổ chức/Cá nhân/Cả hai + dropdown "Nhóm KH, NCC".
    [ObservableProperty] private string _filterCode    = string.Empty;
    [ObservableProperty] private string _filterName    = string.Empty;
    [ObservableProperty] private string _filterAddress = string.Empty;
    [ObservableProperty] private string _filterGroup   = string.Empty;
    [ObservableProperty] private string _filterTaxCode = string.Empty;
    [ObservableProperty] private string _filterPhone   = string.Empty;
    // null = không lọc (checkbox 3 trạng thái), true = chỉ NCC ngừng theo dõi, false = chỉ NCC đang theo dõi.
    [ObservableProperty] private bool?  _filterStopTracking;

    partial void OnFilterCodeChanged(string value)        => SuppliersView.Refresh();
    partial void OnFilterNameChanged(string value)        => SuppliersView.Refresh();
    partial void OnFilterAddressChanged(string value)     => SuppliersView.Refresh();
    partial void OnFilterGroupChanged(string value)       => SuppliersView.Refresh();
    partial void OnFilterTaxCodeChanged(string value)     => SuppliersView.Refresh();
    partial void OnFilterPhoneChanged(string value)       => SuppliersView.Refresh();
    partial void OnFilterStopTrackingChanged(bool? value) => SuppliersView.Refresh();

    // Radio "Tổ chức / Cá nhân / Cả hai" — mặc định Cả hai như MISA.
    [ObservableProperty] private bool _showOrganizationOnly;
    [ObservableProperty] private bool _showIndividualOnly;
    [ObservableProperty] private bool _showBothTypes = true;

    partial void OnShowOrganizationOnlyChanged(bool value) => SuppliersView.Refresh();
    partial void OnShowIndividualOnlyChanged(bool value)   => SuppliersView.Refresh();
    partial void OnShowBothTypesChanged(bool value)        => SuppliersView.Refresh();

    // Dropdown "Nhóm KH, NCC" — lấy từ các nhóm đang có trong danh sách; mục đầu AllGroups = không lọc.
    private const string AllGroups = "Tất cả";
    public ObservableCollection<string> GroupOptions { get; } = new() { AllGroups };
    [ObservableProperty] private string _selectedGroupOption = AllGroups;

    partial void OnSelectedGroupOptionChanged(string value) => SuppliersView.Refresh();

    public ObservableCollection<Supplier> Suppliers { get; } = new();

    // View lọc live theo các Filter* per-cột — DataGrid bind vào đây thay vì Suppliers trực tiếp;
    // Suppliers vẫn là nguồn dữ liệu thật (Add/Remove/Clear ở Load/Duplicate/Delete không đổi).
    public ICollectionView SuppliersView { get; }

    // Footer — đếm đúng số dòng đang hiển thị sau lọc.
    public string RowCountText => $"Số dòng = {SuppliersView.Cast<object>().Count()}";

    private bool HasSelection => SelectedSupplier is not null;

    public SupplierListViewModel(
        INavigationService navigationService,
        IGetSuppliersUseCase getSuppliers,
        IDeleteSupplierUseCase deleteSupplier,
        IDuplicateSupplierUseCase duplicateSupplier,
        IImportExcelSuppliersUseCase importExcel,
        Func<SupplierFormWindow> formWindowFactory)
    {
        _navigationService  = navigationService;
        _getSuppliers       = getSuppliers;
        _deleteSupplier     = deleteSupplier;
        _duplicateSupplier  = duplicateSupplier;
        _importExcel        = importExcel;
        _formWindowFactory  = formWindowFactory;

        SuppliersView = CollectionViewSource.GetDefaultView(Suppliers);
        SuppliersView.Filter = FilterSupplier;
        // ICollectionView tự bắn CollectionChanged (Reset) sau mỗi Refresh() — dùng chung 1 chỗ để
        // cập nhật RowCountText, khỏi phải gọi tay ở từng OnFilterXChanged/Load/Duplicate/Delete.
        SuppliersView.CollectionChanged += (_, _) => OnPropertyChanged(nameof(RowCountText));
    }

    // Nạp lại danh sách nhóm từ dữ liệu hiện có, giữ nguyên mục "Tất cả" ở đầu và lựa chọn đang có.
    private void RefreshGroupOptions()
    {
        var selected = SelectedGroupOption;
        while (GroupOptions.Count > 1) GroupOptions.RemoveAt(GroupOptions.Count - 1);
        foreach (var g in Suppliers.Select(s => s.Group)
                     .Where(g => !string.IsNullOrWhiteSpace(g))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(g => g, StringComparer.CurrentCultureIgnoreCase))
            GroupOptions.Add(g);
        SelectedGroupOption = !string.IsNullOrEmpty(selected) && GroupOptions.Contains(selected) ? selected : AllGroups;
    }

    private bool FilterSupplier(object obj)
    {
        if (obj is not Supplier s) return false;

        if (ShowOrganizationOnly && s.IsIndividual) return false;
        if (ShowIndividualOnly && !s.IsIndividual) return false;

        if (!string.IsNullOrEmpty(SelectedGroupOption) && SelectedGroupOption != AllGroups
            && !string.Equals(s.Group, SelectedGroupOption, StringComparison.OrdinalIgnoreCase))
            return false;

        if (FilterStopTracking is { } stopTracking && s.IsStopTracking != stopTracking)
            return false;

        return Matches(s.Code, FilterCode)
            && Matches(s.Name, FilterName)
            && Matches(s.Address, FilterAddress)
            && Matches(s.Group, FilterGroup)
            && Matches(s.TaxCode, FilterTaxCode)
            && Matches(s.Phone, FilterPhone);
    }

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));

    partial void OnSelectedSupplierChanged(Supplier? value)
    {
        DuplicateSupplierCommand.NotifyCanExecuteChanged();
        EditSupplierCommand.NotifyCanExecuteChanged();
        DeleteSupplierCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadSuppliersAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getSuppliers.ExecuteAsync(ct);
            Suppliers.Clear();
            foreach (var s in items) Suppliers.Add(s);
            HasSuppliers = Suppliers.Count > 0;
            RefreshGroupOptions();
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
    private async Task AddSupplierAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadSuppliersCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task ImportExcelAsync(CancellationToken ct = default)
    {
        var dialog = new OpenFileDialog
        {
            Title  = "Chọn file Excel nhà cung cấp",
            Filter = "Excel files (*.xlsx)|*.xlsx",
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            await using var stream = File.OpenRead(dialog.FileName);
            var result = await _importExcel.ExecuteAsync(stream, Path.GetFileName(dialog.FileName), ct);

            await LoadSuppliersCommand.ExecuteAsync(null);

            var message = $"Import hoàn tất!\n\nĐã import: {result.Imported}/{result.Total} nhà cung cấp.";
            if (result.Errors.Count > 0)
            {
                var errorLines = result.Errors
                    .Take(10)
                    .Select(e => $"  Dòng {e.Row}: {e.Reason}");
                message += $"\n\nDòng lỗi ({result.Skipped}):\n{string.Join("\n", errorLines)}";
                if (result.Errors.Count > 10)
                    message += $"\n  ... và {result.Errors.Count - 10} dòng lỗi khác";
            }

            MessageBox.Show(message, "Kết quả Import", MessageBoxButton.OK,
                result.Errors.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Import thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Excel Files|*.xlsx",
                FileName = $"NhaCungCap_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Nhà cung cấp");

            string[] headers = { "Mã NCC", "Tên NCC", "Địa chỉ", "Nhóm", "Mã số thuế", "Điện thoại" };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var s in SuppliersView.Cast<Supplier>())
            {
                worksheet.Cell(row, 1).Value = s.Code;
                worksheet.Cell(row, 2).Value = s.Name;
                worksheet.Cell(row, 3).Value = s.Address;
                worksheet.Cell(row, 4).Value = s.Group;
                worksheet.Cell(row, 5).Value = s.TaxCode;
                worksheet.Cell(row, 6).Value = s.Phone;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Đã xuất file thành công.", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Xuất Excel thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DuplicateSupplierAsync(CancellationToken ct = default)
    {
        if (SelectedSupplier is null) return;
        IsLoading = true;
        try
        {
            var copy = await _duplicateSupplier.ExecuteAsync(SelectedSupplier.Id, ct);
            Suppliers.Add(copy);
            HasSuppliers     = true;
            SelectedSupplier = copy;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Nhân bản thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditSupplierAsync(CancellationToken ct = default)
    {
        if (SelectedSupplier is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedSupplier);
        if (window.ShowDialog() == true)
            await LoadSuppliersCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSupplierAsync(CancellationToken ct = default)
    {
        if (SelectedSupplier is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa nhà cung cấp '{SelectedSupplier.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteSupplier.ExecuteAsync(SelectedSupplier.Id, ct);
            Suppliers.Remove(SelectedSupplier);
            SelectedSupplier = null;
            HasSuppliers     = Suppliers.Count > 0;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
