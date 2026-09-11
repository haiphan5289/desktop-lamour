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
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
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

    // Kho mặc định khi chọn Mã hàng cho 1 dòng mới — khớp Kho ngầm định "HH" dùng cho vật tư hàng
    // hoá (mirror SalesOrderViewModel.DefaultWarehouseCode cùng ngày) thay vì lấy đại kho đầu tiên
    // trong danh sách (trước đây luôn là "Kho chính", giờ đã ngưng hoạt động).
    private const string DefaultWarehouseCode = "HH";

    public event Action? ReturnSaved;
    public event Action? RequestClose;

    private readonly ICreateSalesReturnUseCase      _createReturn;
    private readonly IUpdateSalesReturnUseCase      _updateReturn;
    private readonly IDeleteSalesReturnUseCase      _deleteReturn;
    private readonly IConfirmSalesReturnUseCase     _confirmReturn;
    private readonly IUnconfirmSalesReturnUseCase   _unconfirmReturn;
    private readonly IGetNextSalesReturnCodeUseCase _getNextCode;
    private readonly IGetCustomersUseCase           _getCustomers;
    private readonly IGetEmployeesUseCase           _getEmployees;
    private readonly IGetProductsUseCase            _getProducts;
    private readonly IGetWarehouseSettingsUseCase   _getWarehouses;
    private readonly IGetAccountSettingsUseCase     _getAccountSettings;
    private readonly IGetDepartmentsUseCase         _getDepartments;
    private readonly ICreateSalesReturnWarehouseReceiptUseCase _createWarehouseReceipt;
    private readonly IGetWarehouseReceiptsUseCase    _getWarehouseReceipts;
    private readonly IGetWarehouseReceiptByIdUseCase _getWarehouseReceiptById;
    private readonly Func<EmployeeFormWindow>       _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>       _customerFormWindowFactory;
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
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(IsConfirmed));
        OnPropertyChanged(nameof(IsHeld));
        OnPropertyChanged(nameof(UnpostButtonLabel));
        PrintCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        CreateWarehouseReceiptCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        ToggleConfirmCommand.NotifyCanExecuteChanged();
        EditCommand.NotifyCanExecuteChanged();
    }

    public bool HasExistingReturn => CurrentReturn is not null;

    // 2026-09-11: đảo ngược quyết định 2026-09-10 ("Ghi sổ" cần 2 lần bấm qua cờ IsArmed) — "Cất"
    // giờ = "Ghi sổ" ngay (khớp hành vi MISA), nên khái niệm "Treo đã đụng toggle" không còn ý nghĩa.
    // Xóa đơn giản chỉ bật khi chứng từ đã tồn tại và CHƯA Confirmed (đang Nháp, sau khi Bỏ ghi).
    public bool CanDeleteReturn => CurrentReturn is not null && !IsConfirmed && IsReadOnly;

    // 2026-09-09 (đổi ý lần 2): "Bỏ ghi" giờ CHỈ đảo trạng thái + tồn kho ở BE — KHÔNG tự mở khóa
    // form nữa. Form vẫn read-only sau Bỏ ghi cho tới khi user bấm "Sửa" riêng trên toolbar (nút
    // built-in EditCommand/EditVisibility của DocumentToolbar, mirror đúng pattern
    // SalesOrderViewModel.IsReadOnly/CanEdit/Edit()). Cần thêm cờ _isReadOnly (WPF-only, KHÔNG map
    // từ CurrentReturn.Status) vì Status chỉ có 2 giá trị (Draft/Confirmed) không đủ diễn tả 3 trạng
    // thái UI thật: Confirmed (khóa) / Draft-vừa-bỏ-ghi-chưa-bấm-Sửa (khóa) / Draft-đang-sửa (mở).
    // 2026-09-10: giờ CŨNG khóa (IsReadOnly=true) ngay sau MỌI lần Cất, không chỉ sau Bỏ ghi — xem
    // SaveAsync/ToggleConfirmAsync (khớp bảng đã chốt: Cất tắt ngay sau khi lưu, phải bấm "Sửa" lại).
    [ObservableProperty] private bool _isReadOnly;

    partial void OnIsReadOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(CanDeleteReturn));
        SaveCommand.NotifyCanExecuteChanged();
        EditCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        ToggleConfirmCommand.NotifyCanExecuteChanged();
    }

    // Nút "In" không đổi, vẫn bật qua HasExistingReturn/PrintCommand bất kể Held/Draft hay Confirmed
    // (Phiếu Nhập Kho liên kết vẫn in được dù chứng từ gốc đang bị bỏ ghi).
    public bool IsEditable  => CurrentReturn is null || (!IsConfirmed && !IsReadOnly);
    public bool IsConfirmed => CurrentReturn?.Status == "Confirmed";
    // 2026-09-11: gộp "Nháp" và "Treo" thành 1 — coi cả 2 raw status là "Treo" (mirror
    // SalesReturnListItem.IsHeld cùng ngày). "Ghi sổ"/"Bỏ ghi" dùng chung 1 nút toggle (xem
    // ToggleConfirmCommand/UnpostButtonLabel bên dưới).
    public bool IsHeld => CurrentReturn?.Status is "Held" or "Draft";

    // "Sửa" — chỉ hiện/bật khi chứng từ đang Held/Draft (chưa Confirmed) VÀ còn bị khóa (_isReadOnly);
    // Confirmed thì phải Bỏ ghi trước, không có đường tắt. Bấm vào mới thật sự mở khóa form + bắt
    // đầu track dirty (trước đó form bị disable nên không phát sinh thay đổi thật để track).
    private bool CanEdit => CurrentReturn is not null && !IsConfirmed && IsReadOnly;

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        IsReadOnly = false;
        BeginDirtyTracking();

        // PopulateFormFromCurrent (InitializeAsync) chỉ nạp đúng số dòng THẬT của chứng từ — không
        // có dòng trống dư như ClearForm() cho chứng từ mới. Bấm "Sửa" xong thêm luôn
        // InitialEmptyLineCount dòng trống để gõ thêm sản phẩm ngay, không phải tự bấm "Thêm dòng"
        // 100 lần. Không dùng lại Lines.Clear() — chỉ APPEND, giữ nguyên các dòng thật đã có.
        for (var i = 0; i < InitialEmptyLineCount; i++) AddLine();
    }

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
    // 2026-09-11 (fix bug "Kho bị mất khi mở lại chứng từ"): `Warehouses` ở trên đã lọc IsActive
    // (chỉ hiện Kho active cho combobox chọn MỚI) — nhưng 1 dòng ĐÃ LƯU trước đó có thể tham chiếu
    // 1 Kho vừa bị ngưng hoạt động (vd "Kho chính" sau migration DeactivateExtraWarehouses). Nếu
    // lookup SelectedWarehouse cho dòng cũ chỉ tìm trong `Warehouses` (đã lọc), sẽ không thấy →
    // SelectedWarehouse=null → cột Kho hiện trống trơn. Danh sách KHÔNG lọc này chỉ dùng để RESOLVE
    // giá trị đã lưu, không dùng làm ItemsSource cho combobox (AppSearchableComboBox.SelectedItem
    // không cần nằm trong ItemsSource vẫn hiện đúng DisplayText — xem AppSearchableComboBox.xaml.cs).
    private IReadOnlyList<ISearchableItem> _allWarehouses = Array.Empty<ISearchableItem>();
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
        IUnconfirmSalesReturnUseCase   unconfirmReturn,
        IGetNextSalesReturnCodeUseCase getNextCode,
        IGetCustomersUseCase           getCustomers,
        IGetEmployeesUseCase           getEmployees,
        IGetProductsUseCase            getProducts,
        IGetWarehouseSettingsUseCase   getWarehouses,
        IGetAccountSettingsUseCase     getAccountSettings,
        IGetDepartmentsUseCase         getDepartments,
        ICreateSalesReturnWarehouseReceiptUseCase createWarehouseReceipt,
        IGetWarehouseReceiptsUseCase    getWarehouseReceipts,
        IGetWarehouseReceiptByIdUseCase getWarehouseReceiptById,
        Func<EmployeeFormWindow>       employeeFormWindowFactory,
        Func<CustomerFormWindow>       customerFormWindowFactory,
        Func<WarehouseReceiptPrintWindow> warehouseReceiptPrintWindowFactory,
        ILogger<SalesReturnViewModel>  logger)
    {
        _createReturn              = createReturn;
        _updateReturn              = updateReturn;
        _deleteReturn              = deleteReturn;
        _confirmReturn             = confirmReturn;
        _unconfirmReturn           = unconfirmReturn;
        _getNextCode               = getNextCode;
        _getCustomers              = getCustomers;
        _getEmployees              = getEmployees;
        _getProducts               = getProducts;
        _getWarehouses             = getWarehouses;
        _getAccountSettings        = getAccountSettings;
        _getDepartments            = getDepartments;
        _createWarehouseReceipt    = createWarehouseReceipt;
        _getWarehouseReceipts      = getWarehouseReceipts;
        _getWarehouseReceiptById   = getWarehouseReceiptById;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _customerFormWindowFactory = customerFormWindowFactory;
        _warehouseReceiptPrintWindowFactory = warehouseReceiptPrintWindowFactory;
        _logger                    = logger;

        Lines.CollectionChanged += (_, _) => RecalculateTotals();
    }

    public async Task InitializeAsync(SalesReturnResponseDto? returnDoc, CancellationToken ct = default)
    {
        IsBusy      = true;
        HasError    = false;
        try
        {
            await LoadLookupsAsync(ct);

            if (returnDoc is null)
            {
                _nextDocumentNumber = await _getNextCode.ExecuteAsync(ct);
                CurrentReturn       = null;
                IsReadOnly          = false; // chứng từ mới, chưa có gì để bảo vệ — sửa được ngay
                ClearForm();
            }
            else
            {
                CurrentReturn = returnDoc;
                // 2026-09-10 (chốt qua mô phỏng tương tác): mở lại BẤT KỲ chứng từ đã tồn tại nào
                // từ danh sách đều khóa form ngay — phải bấm "Sửa" mới sửa được, khớp đúng hành vi
                // "mọi lần round-trip BE đều khóa lại" áp dụng nhất quán trong popup.
                IsReadOnly = true;
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
            // 2026-09-11: chỉ hiện Kho đang hoạt động — trước đây không lọc IsActive nên "Kho
            // chính"/"Kho chi nhánh Q.1" (đã ngưng hoạt động, xem migration
            // DeactivateExtraWarehouses) vẫn hiện trong combobox chọn Kho ở dòng sản phẩm — mirror
            // SalesOrderViewModel cùng ngày.
            _allWarehouses = warehouseTask.Result.Cast<ISearchableItem>().ToList().AsReadOnly();
            Warehouses = warehouseTask.Result.Where(w => w.IsActive).Cast<ISearchableItem>().ToList().AsReadOnly();
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

    [RelayCommand(CanExecute = nameof(IsEditable))]
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

            // 2026-09-11: "Cất" = "Ghi sổ" ngay — đảo ngược quyết định 2026-09-10. BE
            // (Create/UpdateSalesReturnUseCase) giờ luôn lưu chứng từ ở Confirmed + cộng tồn kho
            // ngay trong cùng lần gọi, không còn qua Held/"tự tin hoá" nữa.

            StopDirtyTracking();
            ReturnSaved?.Invoke();
            CurrentReturn = result;
            // Cất xong luôn khóa form lại — phải bấm "Sửa" mới sửa tiếp được.
            IsReadOnly = true;
            IsBusy = false;

            // Giữ popup MỞ sau khi Cất (không RequestClose nữa) để user bấm "In" ngay — nút In đã tự
            // bật qua OnCurrentReturnChanged → PrintCommand.NotifyCanExecuteChanged().
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

    // ── Ghi sổ / Bỏ ghi (nút toggle) ─────────────────────────────────────────
    // 2026-09-11 (đảo ngược quyết định 2026-09-10 "cần 2 lần bấm" — khớp hành vi MISA đối chiếu qua
    // video): "Ghi sổ" và "Bỏ ghi" dùng CHUNG 1 nút (UnpostCommand), MỖI CHIỀU chỉ cần đúng 1 lần
    // bấm — không còn khái niệm "tự tin hoá"/IsArmed nữa.
    // Bật/tắt dựa theo IsReadOnly (khóa) chứ không phải theo status riêng — tắt khi đang Sửa.
    private bool CanToggleConfirm => CurrentReturn is not null && IsReadOnly;
    public string UnpostButtonLabel => IsConfirmed ? "Bỏ ghi" : "Ghi sổ";

    [RelayCommand(CanExecute = nameof(CanToggleConfirm))]
    private async Task ToggleConfirmAsync(CancellationToken ct = default)
    {
        if (CurrentReturn is null) return;

        if (IsConfirmed)
        {
            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn bỏ ghi chứng từ '{CurrentReturn.DocumentNumber}'? Tồn kho đã cộng lúc ghi sổ sẽ được hoàn tác.",
                "Xác nhận bỏ ghi", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
        }

        IsBusy = true;
        try
        {
            var wasConfirmed = IsConfirmed;
            var result = wasConfirmed
                ? await _unconfirmReturn.ExecuteAsync(CurrentReturn.Id, ct)
                : await _confirmReturn.ExecuteAsync(CurrentReturn.Id, ct);
            // Bắn ReturnSaved dù không phải "Save" — đây là tín hiệu DUY NHẤT SalesReturnWindow.xaml.cs
            // dùng để set cờ _hasSaved → DialogResult=true khi đóng popup, để SalesReturnListViewModel
            // reload danh sách.
            ReturnSaved?.Invoke();
            CurrentReturn = result;
            IsReadOnly    = true; // form vẫn khóa tới khi bấm "Sửa" (xem Edit())
            _logger.LogInformation("SalesReturn {Id} toggled confirm — new status {Status}", result.Id, result.Status);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle confirm for sales return {Id}", CurrentReturn.Id);
            MessageBox.Show(ex.Message, "Thao tác thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
    }

    // "In" ở toolbar — in "PHIẾU NHẬP KHO" (mẫu 01-VT) của Phiếu Nhập Kho LIÊN KẾT với chứng từ này,
    // tái dùng nguyên Warehouse/Views/WarehouseReceiptPrintWindow thay vì layout "Phiếu trả lại hàng
    // bán" riêng cũ (SalesReturnPrintWindow — đã xóa hẳn, xem sales-return.md) — khớp đúng phiếu vật
    // lý doanh nghiệp dùng khi nhận lại hàng vào kho (ảnh mẫu user cung cấp).
    //
    // 2026-09-11 (bug fix "In ra sản phẩm cũ sau khi sửa chứng từ"): trước đây tự tìm PN qua
    // FindExistingWarehouseReceiptAsync rồi CHỈ gọi Create khi chưa có — nếu chứng từ bị sửa (thêm/
    // đổi dòng) SAU khi đã lập PN, bước tìm này vẫn thấy PN cũ và tái dùng luôn, in ra dữ liệu cũ.
    // Giờ luôn gọi thẳng _createWarehouseReceipt.ExecuteAsync — BE tự so khớp dòng hàng mỗi lần gọi
    // (tái dùng PN nếu khớp, tự supersede PN cũ + lập PN mới nếu lệch), nên WPF không cần tự
    // tiền-kiểm tra nữa.
    [RelayCommand(CanExecute = nameof(HasExistingReturn))]
    private async Task PrintAsync(CancellationToken ct = default)
    {
        if (CurrentReturn is null) return;

        IsBusy = true;
        try
        {
            var receiptId = (await _createWarehouseReceipt.ExecuteAsync(CurrentReturn.Id, ct)).Id;

            var receipt = await _getWarehouseReceiptById.ExecuteAsync(receiptId, ct);
            if (receipt is null)
            {
                MessageBox.Show("Không tìm thấy phiếu nhập kho vừa lập.", "Không thể in",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // WarehouseReceiptResponseDto không mang địa chỉ đối tác riêng — lấy từ Customer đã chọn
            // trên form SalesReturn, giống cách SalesReturnPrintWindow cũ resolve.
            var customer = SelectedCustomer as DesktopLamour.Features.HomePage.Customers.Domain.Models.Customer;
            var printWindow = _warehouseReceiptPrintWindowFactory();
            printWindow.Initialize(receipt, customer?.Address);
            printWindow.ShowDialog();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to print warehouse receipt for sales return {Id}", CurrentReturn.Id);
            MessageBox.Show(ex.Message, "Không thể in phiếu nhập kho", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { IsBusy = false; }
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

    // Dùng bởi "Lập PN" (CreateWarehouseReceiptAsync) — kiểm tra chứng từ này đã có Phiếu Nhập Kho
    // liên kết còn hiệu lực hay chưa. PrintAsync không dùng hàm này nữa (xem PrintAsync ở trên) —
    // luôn gọi thẳng BE, tự so khớp/supersede. Lọc !IsSuperseded vì PN đã bị thay thế (do chứng từ
    // sửa sau khi lập PN) không còn là bản ghi hiện hành của chứng từ này nữa.
    private async Task<WarehouseReceiptResponseDto?> FindExistingWarehouseReceiptAsync(string documentNumber, CancellationToken ct)
    {
        var allReceipts = await _getWarehouseReceipts.ExecuteAsync(ct);
        return allReceipts.FirstOrDefault(r =>
            r.ReceiptType == ReturnedGoodsReceiptType && r.Reference == documentNumber && !r.IsSuperseded);
    }

    // "Lập PN" — bước thủ công: CHỈ tạo Phiếu Nhập Kho (không tự mở cửa sổ in — bấm "In" riêng nếu
    // cần in ngay). Tự tìm PN đã lập sẵn cho chứng từ này trước — chỉ tạo mới khi chưa có, tránh gọi
    // Create lần 2 cho cùng 1 chứng từ (vd. bấm "Lập PN" 2 lần) ném DomainException "Đã lập phiếu
    // nhập kho cho chứng từ ... rồi.".
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
            var existing = await FindExistingWarehouseReceiptAsync(CurrentReturn.DocumentNumber, ct);

            if (existing is not null)
            {
                MessageBox.Show(
                    $"Chứng từ '{CurrentReturn.DocumentNumber}' đã có phiếu nhập kho '{existing.ReceiptNumber}'.",
                    "Đã lập PN", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var created = await _createWarehouseReceipt.ExecuteAsync(CurrentReturn.Id, ct);
            _logger.LogInformation("Warehouse receipt {ReceiptNumber} created from sales return {Id}",
                created.ReceiptNumber, CurrentReturn.Id);
            MessageBox.Show(
                $"Đã lập phiếu nhập kho '{created.ReceiptNumber}'. Bấm \"In\" ngay trên popup này để in Phiếu Nhập Kho.",
                "Lập PN thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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
                line.SetSelectedWarehouseSilent(
                    Warehouses.FirstOrDefault(w => w.Code == DefaultWarehouseCode) ?? Warehouses.FirstOrDefault());
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
            // _allWarehouses (không lọc IsActive) — dòng đã lưu có thể trỏ tới 1 Kho đã ngưng hoạt
            // động, xem ghi chú tại khai báo `_allWarehouses`.
            item.SetSelectedWarehouseSilent(_allWarehouses.FirstOrDefault(w => w.Id == l.WarehouseId));
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
