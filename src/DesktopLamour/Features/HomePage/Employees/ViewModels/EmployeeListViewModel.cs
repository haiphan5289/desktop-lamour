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
using DesktopLamour.Features.HomePage.Employees.Domain.Models;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using Microsoft.Win32;

namespace DesktopLamour.Features.HomePage.Employees.ViewModels;

public partial class EmployeeListViewModel : ViewModelBase
{
    private readonly INavigationService         _navigationService;
    private readonly IGetEmployeesUseCase       _getEmployees;
    private readonly IDeleteEmployeeUseCase     _deleteEmployee;
    private readonly IDuplicateEmployeeUseCase  _duplicateEmployee;
    private readonly IImportExcelEmployeesUseCase _importExcel;
    private readonly Func<EmployeeFormWindow>   _formWindowFactory;

    [ObservableProperty] private bool       _isLoading;
    [ObservableProperty] private bool       _hasError;
    [ObservableProperty] private string     _errorMessage  = string.Empty;
    [ObservableProperty] private bool       _hasEmployees;
    [ObservableProperty] private Employee?  _selectedEmployee;

    // 1 ô tìm kiếm chung — khớp OR trên các trường text chính, không phân biệt hoa/thường.
    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<Employee> Employees { get; } = new();

    // View lọc live theo các Filter* per-cột — DataGrid bind vào đây thay vì Employees trực tiếp;
    // Employees vẫn là nguồn dữ liệu thật (Add/Remove/Clear ở Load/Duplicate/Delete không đổi).
    public ICollectionView EmployeesView { get; }

    private bool HasSelection => SelectedEmployee is not null;

    public EmployeeListViewModel(
        INavigationService        navigationService,
        IGetEmployeesUseCase      getEmployees,
        IDeleteEmployeeUseCase    deleteEmployee,
        IDuplicateEmployeeUseCase duplicateEmployee,
        IImportExcelEmployeesUseCase importExcel,
        Func<EmployeeFormWindow>  formWindowFactory)
    {
        _navigationService = navigationService;
        _getEmployees      = getEmployees;
        _deleteEmployee    = deleteEmployee;
        _duplicateEmployee = duplicateEmployee;
        _importExcel       = importExcel;
        _formWindowFactory = formWindowFactory;

        EmployeesView = CollectionViewSource.GetDefaultView(Employees);
        EmployeesView.Filter = FilterEmployee;
    }

    partial void OnSearchTextChanged(string value) => EmployeesView.Refresh();

    private bool FilterEmployee(object obj)
    {
        if (obj is not Employee e) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        return Matches(e.Code, SearchText)
            || Matches(e.Name, SearchText)
            || Matches(e.Gender, SearchText)
            || Matches(e.Phone, SearchText)
            || Matches(e.Role, SearchText)
            || Matches(e.Unit, SearchText)
            || Matches(e.JobTitle, SearchText)
            || Matches(e.BankAccountNumber, SearchText)
            || Matches(e.BankName, SearchText);
    }

    private static bool Matches(string? value, string filter)
        => string.IsNullOrWhiteSpace(filter) || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));

    partial void OnSelectedEmployeeChanged(Employee? value)
    {
        DuplicateEmployeeCommand.NotifyCanExecuteChanged();
        EditEmployeeCommand.NotifyCanExecuteChanged();
        DeleteEmployeeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    [RelayCommand]
    private void DismissError() => HasError = false;

    [RelayCommand]
    private async Task LoadEmployeesAsync(CancellationToken ct = default)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _getEmployees.ExecuteAsync(ct);
            Employees.Clear();
            foreach (var e in items) Employees.Add(e);
            HasEmployees = Employees.Count > 0;
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
    private async Task AddEmployeeAsync(CancellationToken ct = default)
    {
        var window = _formWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() == true)
            await LoadEmployeesCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task ImportExcelAsync(CancellationToken ct = default)
    {
        var dialog = new OpenFileDialog
        {
            Title  = "Chọn file Excel nhân viên",
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

            await LoadEmployeesCommand.ExecuteAsync(null);

            var message = $"Import hoàn tất!\n\nĐã import: {result.Imported}/{result.Total} nhân viên.";
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
                FileName = $"NhanVien_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dialog.ShowDialog() != true) return;

            using var workbook  = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Nhân viên");

            // Không xuất mật khẩu (PasswordHash) — dữ liệu nhạy cảm, cũng không hiển thị ở đâu trong UI.
            string[] headers = { "Mã NV", "Tên nhân viên", "Giới tính", "Điện thoại", "Vai trò", "Đơn vị", "Chức danh", "Số tài khoản", "Ngân hàng" };
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value           = headers[i];
                cell.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var e in EmployeesView.Cast<Employee>())
            {
                worksheet.Cell(row, 1).Value = e.Code;
                worksheet.Cell(row, 2).Value = e.Name;
                worksheet.Cell(row, 3).Value = e.Gender;
                worksheet.Cell(row, 4).Value = e.Phone;
                worksheet.Cell(row, 5).Value = e.Role;
                worksheet.Cell(row, 6).Value = e.Unit;
                worksheet.Cell(row, 7).Value = e.JobTitle;
                worksheet.Cell(row, 8).Value = e.BankAccountNumber;
                worksheet.Cell(row, 9).Value = e.BankName;
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
    private async Task DuplicateEmployeeAsync(CancellationToken ct = default)
    {
        if (SelectedEmployee is null) return;
        IsLoading = true;
        try
        {
            var copy = await _duplicateEmployee.ExecuteAsync(SelectedEmployee.Id, ct);
            Employees.Add(copy);
            HasEmployees     = true;
            SelectedEmployee = copy;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Nhân bản thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditEmployeeAsync(CancellationToken ct = default)
    {
        if (SelectedEmployee is null) return;
        var window = _formWindowFactory();
        window.Initialize(SelectedEmployee);
        if (window.ShowDialog() == true)
            await LoadEmployeesCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteEmployeeAsync(CancellationToken ct = default)
    {
        if (SelectedEmployee is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa nhân viên '{SelectedEmployee.Name}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _deleteEmployee.ExecuteAsync(SelectedEmployee.Id, ct);
            Employees.Remove(SelectedEmployee);
            SelectedEmployee = null;
            HasEmployees     = Employees.Count > 0;
        }
        catch (Exception ex)
        {
            HasError     = true;
            ErrorMessage = $"Xóa thất bại: {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
