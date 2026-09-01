// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.AccountSettings.Domain.UseCases;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.Models;
using DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.SalesReturn.Views;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Features.HomePage.SalesReturn.ViewModels;

public partial class SalesReturnViewModel : ViewModelBase
{
    // Số dòng trống nạp sẵn khi mở chứng từ mới — xem ClearForm().
    private const int InitialEmptyLineCount = 100;

    public event Action? ReturnSaved;
    public event Action? RequestClose;

    private readonly ICreateSalesReturnUseCase      _createReturn;
    private readonly IUpdateSalesReturnUseCase      _updateReturn;
    private readonly IDeleteSalesReturnUseCase      _deleteReturn;
    private readonly IConfirmSalesReturnUseCase     _confirmReturn;
    private readonly IGetNextSalesReturnCodeUseCase _getNextCode;
    private readonly IGetCustomersUseCase           _getCustomers;
    private readonly IGetEmployeesUseCase           _getEmployees;
    private readonly IGetProductsUseCase            _getProducts;
    private readonly IGetWarehouseSettingsUseCase   _getWarehouses;
    private readonly IGetAccountSettingsUseCase     _getAccountSettings;
    private readonly IGetDepartmentsUseCase         _getDepartments;
    private readonly ICreateSalesReturnWarehouseReceiptUseCase _createWarehouseReceipt;
    private readonly IGetWarehouseReceiptByIdUseCase _getWarehouseReceiptById;
    private readonly IGetWarehouseReceiptsUseCase    _getWarehouseReceipts;
    private readonly Func<EmployeeFormWindow>       _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>       _customerFormWindowFactory;
    private readonly Func<SalesReturnPrintWindow>   _printWindowFactory;
    private readonly Func<WarehouseReceiptPrintWindow> _warehouseReceiptPrintWindowFactory;
    private readonly ILogger<SalesReturnViewModel>  _logger;

    // ── State ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Header ────────────────────────────────────────────────────────────
    [ObservableProperty] private ISearchableItem? _selectedCustomer;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;
    [ObservableProperty] private string?          _description;
    [ObservableProperty] private string?          _reference;

    // ── Return type ───────────────────────────────────────────────────────
    [ObservableProperty] private int    _returnType;  // 0=GiảmTrừCôngNợ, 1=TrảLạiTiềnMặt
    [ObservableProperty] private string _returnTypeLabel = "Giảm trừ công nợ";

    partial void OnReturnTypeChanged(int value)
    {
        ReturnTypeLabel = value == 1 ? "Trả lại tiền mặt" : "Giảm trừ công nợ";
        OnPropertyChanged(nameof(SelectedReturnType));
        OnPropertyChanged(nameof(IsDebtReduction));
        OnPropertyChanged(nameof(IsCashReturn));
    }

    // Bridge cho ComboBox "Loại trả hàng" trong XAML (SelectedItem cần đúng kiểu ReturnTypeItem,
    // không bind thẳng được vào ReturnType vì đó là int) — trước đây XAML bind
    // "SelectedReturnType" nhưng property này chưa từng tồn tại, khiến dropdown không hoạt động.
    public ReturnTypeItem SelectedReturnType
    {
        get => ReturnTypes.FirstOrDefault(t => t.Value == ReturnType) ?? ReturnTypes[0];
        set
        {
            if (value is not null && value.Value != ReturnType)
                ReturnType = value.Value;
        }
    }

    // Bridge cho 2 RadioButton "Giảm trừ công nợ"/"Trả lại tiền mặt" (khớp ảnh mẫu MISA — dùng
    // radio thay vì dropdown) — RadioButton.IsChecked là bool, không bind thẳng được vào ReturnType
    // (int). OnReturnTypeChanged đã notify cả 2 property này khi ReturnType đổi (kể cả đổi từ nơi
    // khác, không chỉ qua radio).
    public bool IsDebtReduction
    {
        get => ReturnType == 0;
        set { if (value) ReturnType = 0; }
    }

    public bool IsCashReturn
    {
        get => ReturnType == 1;
        set { if (value) ReturnType = 1; }
    }

    // ── Chứng từ ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _accountingDate = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate   = DateTime.Today;
    [ObservableProperty] private string   _documentNumber = "BTL00001";

    // ── Computed ──────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private decimal _totalDiscount;
    [ObservableProperty] private decimal _totalTax;
    [ObservableProperty] private decimal _totalPayment;
    [ObservableProperty] private string  _lineSummary = "Số dòng = 0";

    // ── Data ──────────────────────────────────────────────────────────────
    [ObservableProperty] private SalesReturnResponseDto? _currentReturn;

    partial void OnCurrentReturnChanged(SalesReturnResponseDto? value)
    {
        OnPropertyChanged(nameof(HasExistingReturn));
        OnPropertyChanged(nameof(CanDeleteReturn));
        PrintCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        CreateWarehouseReceiptCommand.NotifyCanExecuteChanged();
    }

    public bool HasExistingReturn => CurrentReturn is not null;

    // Xóa chỉ cho phép khi còn Nháp — khớp guard mới ở DeleteSalesReturnUseCase (BE), tránh mở popup
    // vẫn cho bấm Xóa rồi mới nhận lỗi 400 "Chỉ chứng từ ở trạng thái Nháp mới được xóa".
    public bool CanDeleteReturn => CurrentReturn is not null && CurrentReturn.Status == "Draft";

    // ── Điều hướng Trước/Sau/Thêm trong popup — gọi từ SalesReturnWindow.Initialize khi mở từ
    // 1 danh sách (SalesReturnListViewModel). Không set (mặc định rỗng) → CanNavigatePrev/Next
    // luôn false, nút Trước/Sau disable (mờ) chứ không ẩn hẳn khỏi toolbar.
    private IReadOnlyList<SalesReturnResponseDto> _siblingReturns = Array.Empty<SalesReturnResponseDto>();
    private int _siblingIndex = -1;

    // CanExecute cho NavigatePrev/NavigateNextCommand.
    public bool CanNavigatePrev => _siblingIndex > 0;
    public bool CanNavigateNext => _siblingIndex >= 0 && _siblingIndex < _siblingReturns.Count - 1;

    private void NotifyNavigationChanged()
    {
        NavigatePrevCommand.NotifyCanExecuteChanged();
        NavigateNextCommand.NotifyCanExecuteChanged();
    }

    public void SetSiblingContext(IReadOnlyList<SalesReturnResponseDto> siblings, int currentIndex)
    {
        _siblingReturns = siblings;
        _siblingIndex   = currentIndex;
        NotifyNavigationChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrev))]
    private async Task NavigatePrevAsync(CancellationToken ct = default)
    {
        if (_siblingIndex <= 0 || !ConfirmDiscardIfDirty()) return;
        _siblingIndex--;
        await InitializeAsync(_siblingReturns[_siblingIndex], ct);
        NotifyNavigationChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private async Task NavigateNextAsync(CancellationToken ct = default)
    {
        if (_siblingIndex < 0 || _siblingIndex >= _siblingReturns.Count - 1 || !ConfirmDiscardIfDirty()) return;
        _siblingIndex++;
        await InitializeAsync(_siblingReturns[_siblingIndex], ct);
        NotifyNavigationChanged();
    }

    [RelayCommand]
    private async Task AddNewAsync(CancellationToken ct = default)
    {
        if (!ConfirmDiscardIfDirty()) return;
        _siblingIndex = -1;
        await InitializeAsync(null, ct);
        NotifyNavigationChanged();
    }

    // Dùng chung cho Trước/Sau/Thêm — khớp text cảnh báo đã dùng ở SalesReturnWindow.OnClosing.
    private bool ConfirmDiscardIfDirty()
    {
        if (!IsDirty) return true;
        var r = MessageBox.Show(
            "Dữ liệu chưa lưu sẽ bị mất nếu tiếp tục. Bạn có chắc muốn tiếp tục?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        return r == MessageBoxResult.Yes;
    }

    public ObservableCollection<SalesReturnLineItem> Lines { get; } = new();

    public IReadOnlyList<ISearchableItem> Customers { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public ObservableCollection<ISearchableItem> Products { get; } = new();
    public IReadOnlyList<ISearchableItem> Warehouses { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> AccountSettings { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Departments { get; private set; } = Array.Empty<ISearchableItem>();
    private readonly List<ISearchableItem> _allProducts = new();

    public IReadOnlyList<ReturnTypeItem> ReturnTypes { get; } = new[]
    {
        new ReturnTypeItem(0, "Giảm trừ công nợ"),
        new ReturnTypeItem(1, "Trả lại tiền mặt"),
    };

    private string _nextDocumentNumber = "BTL00001";

    public SalesReturnViewModel(
        ICreateSalesReturnUseCase      createReturn,
        IUpdateSalesReturnUseCase      updateReturn,
        IDeleteSalesReturnUseCase      deleteReturn,
        IConfirmSalesReturnUseCase     confirmReturn,
        IGetNextSalesReturnCodeUseCase getNextCode,
        IGetCustomersUseCase           getCustomers,
        IGetEmployeesUseCase           getEmployees,
        IGetProductsUseCase            getProducts,
        IGetWarehouseSettingsUseCase   getWarehouses,
        IGetAccountSettingsUseCase     getAccountSettings,
        IGetDepartmentsUseCase         getDepartments,
        ICreateSalesReturnWarehouseReceiptUseCase createWarehouseReceipt,
        IGetWarehouseReceiptByIdUseCase getWarehouseReceiptById,
        IGetWarehouseReceiptsUseCase    getWarehouseReceipts,
        Func<EmployeeFormWindow>       employeeFormWindowFactory,
        Func<CustomerFormWindow>       customerFormWindowFactory,
        Func<SalesReturnPrintWindow>   printWindowFactory,
        Func<WarehouseReceiptPrintWindow> warehouseReceiptPrintWindowFactory,
        ILogger<SalesReturnViewModel>  logger)
    {
        _createReturn              = createReturn;
        _updateReturn              = updateReturn;
        _deleteReturn              = deleteReturn;
        _confirmReturn             = confirmReturn;
        _getNextCode               = getNextCode;
        _getCustomers              = getCustomers;
        _getEmployees              = getEmployees;
        _getProducts               = getProducts;
        _getWarehouses             = getWarehouses;
        _getAccountSettings        = getAccountSettings;
        _getDepartments            = getDepartments;
        _createWarehouseReceipt    = createWarehouseReceipt;
        _getWarehouseReceiptById   = getWarehouseReceiptById;
        _getWarehouseReceipts      = getWarehouseReceipts;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _customerFormWindowFactory = customerFormWindowFactory;
        _printWindowFactory        = printWindowFactory;
        _warehouseReceiptPrintWindowFactory = warehouseReceiptPrintWindowFactory;
        _logger                    = logger;

        Lines.CollectionChanged += (_, _) => RecalculateTotals();
    }

    public async Task InitializeAsync(SalesReturnResponseDto? returnDoc, CancellationToken ct = default)
    {
        IsBusy   = true;
        HasError = false;
        try
        {
            await LoadLookupsAsync(ct);

            if (returnDoc is null)
            {
                _nextDocumentNumber = await _getNextCode.ExecuteAsync(ct);
                CurrentReturn       = null;
                ClearForm();
            }
            else
            {
                CurrentReturn = returnDoc;
                PopulateFormFromCurrent();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SalesReturnViewModel");
            HasError     = true;
            ErrorMessage = $"Không thể tải dữ liệu: {ex.Message}";
        }
        finally { IsBusy = false; }

        BeginDirtyTracking();
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        var customerTask       = _getCustomers.ExecuteAsync(ct);
        var employeeTask       = _getEmployees.ExecuteAsync(ct);
        var productTask        = _getProducts.ExecuteAsync(ct);
        var warehouseTask      = _getWarehouses.ExecuteAsync(ct);
        var accountSettingTask = _getAccountSettings.ExecuteAsync(ct);
        var departmentTask     = _getDepartments.ExecuteAsync(ct);

        await Task.WhenAll(customerTask, employeeTask, productTask, warehouseTask, accountSettingTask, departmentTask);

        if (departmentTask.IsCompletedSuccessfully)
        {
            Departments = departmentTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Departments));
        }
        else _logger.LogWarning(departmentTask.Exception, "Could not preload departments");

        if (warehouseTask.IsCompletedSuccessfully)
        {
            Warehouses = warehouseTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Warehouses));
        }
        else _logger.LogWarning(warehouseTask.Exception, "Could not preload warehouses");

        if (accountSettingTask.IsCompletedSuccessfully)
        {
            AccountSettings = accountSettingTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(AccountSettings));
        }
        else _logger.LogWarning(accountSettingTask.Exception, "Could not preload account settings");

        if (customerTask.IsCompletedSuccessfully)
        {
            Customers = customerTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Customers));
        }
        else _logger.LogWarning(customerTask.Exception, "Could not preload customers");

        if (employeeTask.IsCompletedSuccessfully)
        {
            Employees = employeeTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
        }
        else _logger.LogWarning(employeeTask.Exception, "Could not preload employees");

        if (productTask.IsCompletedSuccessfully)
        {
            _allProducts.Clear();
            _allProducts.AddRange(productTask.Result.Where(p => p.IsActive).Cast<ISearchableItem>());
            ResetProductFilter();
        }
        else _logger.LogWarning(productTask.Exception, "Could not preload products");
    }

    // ── Commands ──────────────────────────────────────────────────────────

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

        if (Lines.Count(l => l.ProductId > 0) == 0)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng nhập ít nhất một mặt hàng.";
            return;
        }

        IsBusy = true;
        try
        {
            SalesReturnResponseDto result;
            if (CurrentReturn is null)
            {
                var request = BuildCreateRequest();
                result = await _createReturn.ExecuteAsync(request, ct);
                _logger.LogInformation("SalesReturn created: {DocumentNumber}", result.DocumentNumber);
            }
            else
            {
                var request = BuildUpdateRequest();
                result = await _updateReturn.ExecuteAsync(CurrentReturn.Id, request, ct);
                _logger.LogInformation("SalesReturn updated: {Id}", result.Id);
            }

            // Nút toolbar tên "Ghi sổ" phải THẬT SỰ ghi sổ — chuyển Draft → Confirmed, cộng tồn
            // kho thật (side-effect nằm ở BE ConfirmSalesReturnUseCase). Create/Update chỉ tạo
            // bản ghi ở Draft (theo thiết kế mới, xem sales-return.md 2026-08-31); không tự Confirm
            // ở đây thì chứng từ sẽ kẹt ở Nháp mãi mãi dù toolbar hiển thị y như đã "Ghi sổ" xong.
            // Update trên 1 bản ghi ĐÃ Confirmed sẽ không tới được dòng này (BE ném 400 ở bước
            // UpdateAsync phía trên rồi) — CanDeleteReturn/List-level CanExecute cũng đã chặn Sửa
            // trên bản ghi Confirmed từ trước khi mở được popup này.
            if (result.Status == "Draft")
            {
                result = await _confirmReturn.ExecuteAsync(result.Id, ct);
                _logger.LogInformation("SalesReturn confirmed (Ghi sổ): {DocumentNumber}", result.DocumentNumber);
            }

            StopDirtyTracking();
            ReturnSaved?.Invoke();
            // Gán lại CurrentReturn (trước đây bỏ trống cho tới khi InitializeAsync chạy lại) — cần
            // có Id thật để EnsureWarehouseReceiptPrintedAsync bên dưới hoạt động, đồng thời bật
            // luôn Xóa/In/Lập PN nếu bước tự động lập PN lỗi và form phải mở lại để retry thủ công.
            CurrentReturn = result;
            IsBusy = false;

            // Sau khi Ghi sổ, tự động Lập PN (tạo Phiếu Nhập Kho thật — hoặc dùng lại PN đã có nếu
            // chứng từ này đã từng Lập PN trước đó, xem EnsureWarehouseReceiptPrintedAsync) rồi in
            // luôn theo mẫu MISA "PHIẾU NHẬP KHO" — thay cho in "Phiếu trả lại hàng bán"
            // (SalesReturnPrintWindow) như trước đây, theo yêu cầu gộp workflow "Ghi sổ" và "Lập
            // PN" làm một.
            try
            {
                await EnsureWarehouseReceiptPrintedAsync(result.Id, result.DocumentNumber, ct);
            }
            catch (Exception ex)
            {
                // Chứng từ trả hàng ĐÃ Ghi sổ thành công — chỉ riêng bước Lập PN tự động bị lỗi.
                // Không đóng form (giữ nguyên để user bấm lại nút "Lập PN" thủ công, đã bật vì
                // CurrentReturn vừa được gán ở trên).
                _logger.LogError(ex, "SalesReturn saved but auto warehouse-receipt creation failed for {Id}", result.Id);
                MessageBox.Show(
                    $"Đã Ghi sổ chứng từ '{result.DocumentNumber}' thành công, nhưng không thể tự động lập phiếu nhập kho: {ex.Message}\nBạn có thể bấm \"Lập PN\" để thử lại.",
                    "Lập phiếu nhập kho thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save sales return");
            HasError     = true;
            ErrorMessage = ex.Message;
            MessageBox.Show(ex.Message, "Không thể ghi sổ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    private void ShowPrintPreview(SalesReturnResponseDto salesReturn)
    {
        var customer = SelectedCustomer as DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer;
        var printWindow = _printWindowFactory();
        // SalesReturnResponseDto không có CustomerAddress (khác SalesOrderResponseDto) — lấy thẳng
        // từ Customer đã chọn trên form, cũng không có khái niệm CustomerNameOverride nên luôn hiện SĐT.
        printWindow.Initialize(salesReturn, customer?.Phone, customer?.Address);
        printWindow.ShowDialog();
    }

    // "In" ở toolbar — in lại chứng từ đã Ghi sổ. Chỉ khả dụng sau khi CurrentReturn tồn tại
    // (khác SalesOrderWindow, form này không hỗ trợ preview cho dữ liệu chưa lưu vì
    // ShowPrintPreview cần SalesReturnResponseDto đầy đủ Id/DocumentNumber từ BE).
    [RelayCommand(CanExecute = nameof(HasExistingReturn))]
    private void Print()
    {
        if (CurrentReturn is null) return;
        ShowPrintPreview(CurrentReturn);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteReturn))]
    private async Task DeleteAsync(CancellationToken ct = default)
    {
        if (CurrentReturn is null) return;

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa chứng từ '{CurrentReturn.DocumentNumber}'?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _deleteReturn.ExecuteAsync(CurrentReturn.Id, ct);
            _logger.LogInformation("SalesReturn deleted: {Id}", CurrentReturn.Id);
            RequestClose?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete sales return");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    // BE không có FK thật giữa WarehouseReceipt và SalesReturn — CreateSalesReturnWarehouseReceiptUseCase
    // tự phát hiện "đã lập PN rồi" bằng cách so Reference (== DocumentNumber) + ReceiptType == 2
    // (ReturnedGoods, xem WarehouseReceiptListViewModel.ReceiptTypeLabel), ném DomainException nếu
    // trùng. Giá trị này khớp đúng logic đó ở phía WPF để tự KIỂM TRA TRƯỚC khi gọi Create.
    private const int ReturnedGoodsReceiptType = 2;

    // Dùng chung bởi SaveAsync (tự động, sau khi Ghi sổ) và CreateWarehouseReceiptAsync (nút "Lập
    // PN" thủ công). Tự tìm PN đã lập sẵn cho chứng từ này trước — CHỈ tạo mới khi chưa có, tránh
    // gọi Create lần 2 cho cùng 1 chứng từ (vd. Ghi sổ lại 1 chứng từ đã có PN từ lần Ghi sổ/Lập PN
    // trước) ném DomainException "Đã lập phiếu nhập kho cho chứng từ ... rồi." — thay vào đó mở lại
    // đúng PN đã có để in, không tạo trùng.
    private async Task EnsureWarehouseReceiptPrintedAsync(int salesReturnId, string documentNumber, CancellationToken ct)
    {
        var allReceipts = await _getWarehouseReceipts.ExecuteAsync(ct);
        var existing = allReceipts.FirstOrDefault(r =>
            r.ReceiptType == ReturnedGoodsReceiptType && r.Reference == documentNumber);

        var receiptId = existing?.Id
            ?? (await _createWarehouseReceipt.ExecuteAsync(salesReturnId, ct)).Id;

        // Dù vừa tạo hay đã có sẵn, luôn fetch lại đầy đủ theo Id (dòng hàng, TK Nợ/Có...) để in —
        // CreateWarehouseReceiptResultDto (khi vừa tạo) chỉ có id/receipt_number.
        var receipt = await _getWarehouseReceiptById.ExecuteAsync(receiptId, ct);
        if (receipt is null) return;

        var partner = Customers.FirstOrDefault(c => c.Id == receipt.CustomerId)
            as DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer;
        var printWindow = _warehouseReceiptPrintWindowFactory();
        printWindow.Initialize(receipt, partner?.Address);
        printWindow.Owner = Application.Current.MainWindow;
        printWindow.ShowDialog();
    }

    // "Lập PN" — dùng lại thủ công khi: (1) chứng từ cũ mở lại từ danh sách chưa có PN, hoặc
    // (2) bước tự động lập PN trong SaveAsync bị lỗi và cần retry.
    [RelayCommand(CanExecute = nameof(HasExistingReturn))]
    private async Task CreateWarehouseReceiptAsync(CancellationToken ct = default)
    {
        if (CurrentReturn is null)
        {
            MessageBox.Show(
                "Vui lòng Ghi sổ chứng từ trước khi lập phiếu nhập kho.",
                "Chưa thể lập PN", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsBusy = true;
        try
        {
            await EnsureWarehouseReceiptPrintedAsync(CurrentReturn.Id, CurrentReturn.DocumentNumber, ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create warehouse receipt from sales return {Id}", CurrentReturn.Id);
            MessageBox.Show(ex.Message, "Không thể lập phiếu nhập kho", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Cancel()
    {
        HasError = false;
        if (CurrentReturn is null)
            ClearForm();
        else
            PopulateFormFromCurrent();
    }

    [RelayCommand]
    private async Task AddEmployeeAsync(CancellationToken ct = default)
    {
        var before = Employees.Select(e => e.Id).ToHashSet();
        var window = _employeeFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var employees = await _getEmployees.ExecuteAsync(ct);
            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Employees));
            var newItem = Employees.FirstOrDefault(e => !before.Contains(e.Id));
            if (newItem is not null) SelectedEmployee = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload employees after add"); }
    }

    [RelayCommand]
    private async Task AddCustomerAsync(CancellationToken ct = default)
    {
        var before = Customers.Select(c => c.Id).ToHashSet();
        var window = _customerFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            Customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            OnPropertyChanged(nameof(Customers));
            var newItem = Customers.FirstOrDefault(c => !before.Contains(c.Id));
            if (newItem is not null) SelectedCustomer = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload customers after add"); }
    }

    [RelayCommand]
    private void AddLine()
    {
        var line = new SalesReturnLineItem();
        AttachLineHandlers(line);
        Lines.Add(line);
    }

    // Dòng mới phải thực sự rỗng (không Kho/TK/Số lượng mặc định hiển thị sẵn trên dòng chưa chọn
    // sản phẩm) — tự điền các giá trị này CHỈ khi user chọn 1 sản phẩm thật (ProductId chuyển từ
    // 0 → có giá trị). Trước đây AddLine set sẵn Warehouse/TK/Quantity=1 ngay từ đầu cho toàn bộ
    // 100 dòng trống nạp sẵn (ClearForm), khiến dòng chưa có sản phẩm vẫn hiện Kho/TK/Số lượng —
    // đây chính là bug đã báo. Giống hệt cách SalesOrderViewModel.AttachLineHandlers làm cho
    // "Chứng từ bán hàng" (không có tác dụng phụ lên dữ liệu lưu — BuildCreateRequest/
    // BuildUpdateRequest đã lọc Lines.Where(l => l.ProductId > 0) từ trước).
    //
    // 2026-08-28: lần sửa đầu (bỏ set trong AddLine) CHƯA đủ — CellTemplate hiển thị (không phải
    // CellEditingTemplate) bind THẲNG vào ReturnAccount/DebtAccount/DiscountAccount/TaxAccount/
    // CostAccount/CogsAccount (string), không qua SelectedXxxAccount — mà các string này lại có
    // default "5212"/"131"/"5211"/"33311"/"1561"/"632" ngay từ field initializer trên
    // SalesReturnLineItem, nên dòng trống vẫn hiện sẵn dù AddLine không gán gì. Đã đổi field
    // initializer về "" (xem SalesReturnLineItem.cs) và chuyển việc gán giá trị mặc định thật vào
    // đúng đây — nơi DUY NHẤT set các string này, chỉ chạy khi ProductId > 0.
    private void AttachLineHandlers(SalesReturnLineItem line)
    {
        line.PropertyChanged += (_, e) =>
        {
            RecalculateTotals();

            if (e.PropertyName == nameof(SalesReturnLineItem.ProductId) && line.ProductId > 0 && line.WarehouseId == 0)
            {
                line.Quantity        = 1;
                line.ReturnAccount   = "5212";
                line.DebtAccount     = "131";
                line.DiscountAccount = "5211";
                line.TaxAccount      = "33311";
                line.CostAccount     = "1561";
                line.CogsAccount     = "632";
                line.SetSelectedWarehouseSilent(Warehouses.FirstOrDefault());
                line.SetSelectedReturnAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == line.ReturnAccount));
                line.SetSelectedDebtAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == line.DebtAccount));
                line.SetSelectedDiscountAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == line.DiscountAccount));
                line.SetSelectedTaxAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == line.TaxAccount));
                line.SetSelectedCostAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == line.CostAccount));
                line.SetSelectedCogsAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == line.CogsAccount));
            }
        };
    }

    [RelayCommand]
    private void RemoveLine(SalesReturnLineItem line)
    {
        Lines.Remove(line);
        RecalculateTotals();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    partial void OnSelectedCustomerChanged(ISearchableItem? value)
    {
        if (value is DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer c)
        {
            // Diễn giải: để trống, user tự nhập — không auto-fill "Thu hồi hàng {Tên KH}" nữa.

            // Tự điền "NV bán hàng" theo nhân viên chăm sóc gắn sẵn trên khách hàng — khớp hành vi
            // OnSelectedCustomerChanged bên SalesOrderViewModel ("Chứng từ bán hàng").
            if (c.SaleCareEmployeeId.HasValue)
            {
                var matched = Employees.FirstOrDefault(e => e.Id == c.SaleCareEmployeeId.Value);
                if (matched is not null)
                    SelectedEmployee = matched;
            }
        }
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        SelectedEmployee = null;
        Description      = null;
        Reference        = null;
        ReturnType       = 0;
        AccountingDate   = DateTime.Today;
        DocumentDate     = DateTime.Today;
        DocumentNumber   = _nextDocumentNumber;
        Lines.Clear();
        // Chứng từ mới: nạp sẵn N dòng trống để user gõ liền — xem ghi chú tương tự ở SalesOrderViewModel.
        for (var i = 0; i < InitialEmptyLineCount; i++) AddLine();
        RecalculateTotals();
    }

    private void PopulateFormFromCurrent()
    {
        if (CurrentReturn is null) return;

        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == CurrentReturn.CustomerId);
        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == CurrentReturn.EmployeeId);
        Description      = CurrentReturn.Description;
        Reference        = CurrentReturn.Reference;
        ReturnType       = CurrentReturn.ReturnType;
        AccountingDate   = CurrentReturn.AccountingDate.ToLocalTime();
        DocumentDate     = CurrentReturn.DocumentDate.ToLocalTime();
        DocumentNumber   = CurrentReturn.DocumentNumber;

        Lines.Clear();
        foreach (var l in CurrentReturn.Lines)
        {
            var item = new SalesReturnLineItem
            {
                ProductId        = l.ProductId,
                ProductCode      = l.ProductCode,
                ProductName      = l.ProductName,
                ReturnAccount    = l.ReturnAccount,
                DebtAccount      = l.DebtAccount,
                DiscountAccount  = l.DiscountAccount,
                Unit             = l.Unit,
                Quantity         = l.Quantity,
                UnitPrice        = l.UnitPrice,
                DiscountRate     = l.DiscountRate,
                SalesOrderNumber = l.SalesOrderNumber,
                TaxRate          = l.TaxRate,
                TaxAccount       = l.TaxAccount,
                CostAccount      = l.CostAccount,
                CogsAccount      = l.CogsAccount,
                CostPrice        = l.CostPrice,
            };
            item.SetSelectedProductSilent(_allProducts.FirstOrDefault(p => p.Id == l.ProductId));
            item.SetSelectedWarehouseSilent(Warehouses.FirstOrDefault(w => w.Id == l.WarehouseId));
            item.SetSelectedReturnAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == l.ReturnAccount));
            item.SetSelectedDebtAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == l.DebtAccount));
            item.SetSelectedDiscountAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == l.DiscountAccount));
            item.SetSelectedTaxAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == l.TaxAccount));
            item.SetSelectedCostAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == l.CostAccount));
            item.SetSelectedCogsAccountSilent(AccountSettings.FirstOrDefault(a => a.Code == l.CogsAccount));
            item.SetSelectedDepartmentSilent(Departments.FirstOrDefault(d => d.Id == l.DepartmentId));
            item.PropertyChanged += (_, _) => RecalculateTotals();
            Lines.Add(item);
        }

        RecalculateTotals();
    }

    // AppSearchableComboBox tự lọc theo Code/Name khi user gõ (control TextBox tự viết, filter nội
    // bộ không đụng vào Text đang gõ) nên Products chỉ cần giữ đúng danh sách đầy đủ — không cần
    // lọc lại mỗi keystroke như ComboBox gốc trước đây (FilterProductsByCode/ByName đã bỏ, từng
    // gây lỗi gõ tiếng Việt có dấu vì ComboBox tự reset Text/caret mỗi lần ItemsSource đổi). Khớp
    // đúng cách SalesOrderViewModel.ResetProductFilter đang làm.
    public void ResetProductFilter()
    {
        Products.Clear();
        foreach (var p in _allProducts) Products.Add(p);
    }

    private void RecalculateTotals()
    {
        TotalAmount   = Lines.Sum(l => l.Amount);
        TotalDiscount = Lines.Sum(l => l.DiscountAmount);
        TotalTax      = Lines.Sum(l => l.TaxAmount);
        // KHÔNG cộng TotalTax vào đây — khớp đúng công thức BE (CreateSalesReturnUseCase.TotalPayment
        // = TotalAmount - TotalDiscount, không có tax). TotalTax chỉ là số hiển thị riêng ở footer.
        TotalPayment  = TotalAmount - TotalDiscount;
        LineSummary   = $"Số dòng = {Lines.Count(l => l.ProductId > 0)}";
    }

    private CreateSalesReturnRequestDto BuildCreateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)   ? null : Reference.Trim(),
        ReturnType     = ReturnType,
        Lines          = Lines.Where(l => l.ProductId > 0).Select(ToLineDto).ToList(),
    };

    private UpdateSalesReturnRequestDto BuildUpdateRequest() => new()
    {
        DocumentNumber = DocumentNumber.Trim(),
        AccountingDate = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified),
        DocumentDate   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified),
        CustomerId     = SelectedCustomer!.Id,
        EmployeeId     = SelectedEmployee?.Id,
        Description    = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        Reference      = string.IsNullOrWhiteSpace(Reference)   ? null : Reference.Trim(),
        ReturnType     = ReturnType,
        Lines          = Lines.Where(l => l.ProductId > 0).Select(ToLineDto).ToList(),
    };

    private static SalesReturnLineDto ToLineDto(SalesReturnLineItem item) => new()
    {
        ProductId        = item.ProductId,
        WarehouseId      = item.WarehouseId,
        ProductCode      = item.ProductCode,
        ProductName      = item.ProductName,
        ReturnAccount    = item.ReturnAccount,
        DebtAccount      = item.DebtAccount,
        DiscountAccount  = item.DiscountAccount,
        Unit             = item.Unit,
        Quantity         = item.Quantity,
        UnitPrice        = item.UnitPrice,
        Amount           = item.Amount,
        DiscountRate     = item.DiscountRate,
        DiscountAmount   = item.DiscountAmount,
        SalesOrderNumber = item.SalesOrderNumber,
        TaxRate          = item.TaxRate,
        TaxAmount        = item.TaxAmount,
        TaxAccount       = item.TaxAccount,
        CostAccount      = item.CostAccount,
        CogsAccount      = item.CogsAccount,
        CostPrice        = item.CostPrice,
        CostAmount       = item.CostAmount,
        DepartmentId     = item.DepartmentId,
    };
}

public record ReturnTypeItem(int Value, string Label);
