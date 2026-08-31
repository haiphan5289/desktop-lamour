// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLamour.Core.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Shared.Controls;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Warehouse.ViewModels;

public partial class WarehouseReceiptFormViewModel : ViewModelBase
{
    // Số dòng trống nạp sẵn khi mở form (luôn là chứng từ mới — không có luồng Sửa) — xem LoadAsync().
    private const int InitialEmptyLineCount = 100;

    private readonly ICreateWarehouseReceiptUseCase       _createUseCase;
    private readonly IConfirmWarehouseReceiptUseCase      _confirmUseCase;
    private readonly IUpdateWarehouseReceiptUseCase       _updateUseCase;
    private readonly IUnconfirmWarehouseReceiptUseCase    _unconfirmUseCase;
    private readonly IGetCustomersUseCase                 _getCustomers;
    private readonly IGetSuppliersUseCase                 _getSuppliers;
    private readonly IGetEmployeesUseCase                 _getEmployees;
    private readonly IGetProductsUseCase                  _getProducts;
    private readonly Func<EmployeeFormWindow>             _employeeFormWindowFactory;
    private readonly Func<CustomerFormWindow>             _customerFormWindowFactory;
    private readonly Func<WarehouseReceiptPrintWindow>    _printWindowFactory;
    private readonly IGetWarehouseReceiptByIdUseCase      _getReceiptById;
    private readonly ILogger<WarehouseReceiptFormViewModel> _logger;

    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private bool     _hasError;
    [ObservableProperty] private string   _errorMessage   = string.Empty;
    [ObservableProperty] private DateTime _accountingDate  = DateTime.Today;
    [ObservableProperty] private DateTime _documentDate    = DateTime.Today;
    [ObservableProperty] private string   _description     = string.Empty;
    [ObservableProperty] private string   _deliveryPerson  = string.Empty;
    [ObservableProperty] private string   _reference       = string.Empty;
    [ObservableProperty] private decimal  _totalAmount;

    // Sửa phiếu đã tồn tại (mở từ "Nhập, Xuất Kho" → click 1 dòng NK): ReceiptId != null.
    // Tạo mới: ReceiptId == null (giữ nguyên hành vi cũ — Save = Create + Confirm gộp).
    [ObservableProperty] private int?     _receiptId;
    [ObservableProperty] private string   _receiptNumber = string.Empty;
    [ObservableProperty] private string   _status        = "Draft";

    private WarehouseReceiptResponseDto? _existingReceipt;

    public bool IsConfirmed => Status == "Confirmed";
    public bool IsEditable  => !IsConfirmed;

    partial void OnStatusChanged(string value)
    {
        OnPropertyChanged(nameof(IsConfirmed));
        OnPropertyChanged(nameof(IsEditable));
        UnconfirmCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    // 0-based index for ComboBox binding; maps to ReceiptType 1, 2, 3, 4
    [ObservableProperty] private int _selectedReceiptTypeIndex;

    public int SelectedReceiptType => SelectedReceiptTypeIndex + 1;

    [ObservableProperty] private ISearchableItem? _selectedObject;
    [ObservableProperty] private ISearchableItem? _selectedEmployee;

    private IReadOnlyList<ISearchableItem> _customers = Array.Empty<ISearchableItem>();
    private IReadOnlyList<ISearchableItem> _suppliers = Array.Empty<ISearchableItem>();

    public IReadOnlyList<ISearchableItem> Objects  { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Employees { get; private set; } = Array.Empty<ISearchableItem>();
    public IReadOnlyList<ISearchableItem> Products  { get; private set; } = Array.Empty<ISearchableItem>();

    public ObservableCollection<WarehouseReceiptLineItem> Lines { get; } = new();

    public event Action<bool>? RequestClose;

    public WarehouseReceiptFormViewModel(
        ICreateWarehouseReceiptUseCase       createUseCase,
        IConfirmWarehouseReceiptUseCase      confirmUseCase,
        IUpdateWarehouseReceiptUseCase       updateUseCase,
        IUnconfirmWarehouseReceiptUseCase    unconfirmUseCase,
        IGetCustomersUseCase                 getCustomers,
        IGetSuppliersUseCase                 getSuppliers,
        IGetEmployeesUseCase                 getEmployees,
        IGetProductsUseCase                  getProducts,
        Func<EmployeeFormWindow>             employeeFormWindowFactory,
        Func<CustomerFormWindow>             customerFormWindowFactory,
        Func<WarehouseReceiptPrintWindow>    printWindowFactory,
        IGetWarehouseReceiptByIdUseCase      getReceiptById,
        ILogger<WarehouseReceiptFormViewModel> logger)
    {
        _createUseCase             = createUseCase;
        _confirmUseCase            = confirmUseCase;
        _updateUseCase             = updateUseCase;
        _unconfirmUseCase          = unconfirmUseCase;
        _getCustomers              = getCustomers;
        _getSuppliers              = getSuppliers;
        _getEmployees              = getEmployees;
        _getProducts               = getProducts;
        _employeeFormWindowFactory = employeeFormWindowFactory;
        _customerFormWindowFactory = customerFormWindowFactory;
        _printWindowFactory        = printWindowFactory;
        _getReceiptById            = getReceiptById;
        _logger                    = logger;
    }

    // Gọi trước LoadAsync() để mở form ở chế độ Sửa (phiếu đã tồn tại, mở từ "Nhập, Xuất Kho").
    // Không gọi (hoặc truyền null) → form tạo mới như cũ.
    public void Initialize(WarehouseReceiptResponseDto? existing) => _existingReceipt = existing;

    // ── Điều hướng Trước/Sau/Thêm trong popup — gọi từ WarehouseReceiptFormWindow.Initialize khi
    // mở từ "Nhập, Xuất Kho" (WarehouseTransactionListViewModel). Chỉ giữ Id (không phải DTO đầy
    // đủ như Sales/SalesReturn) vì danh sách nguồn chỉ có DTO rút gọn — Trước/Sau gọi lại
    // IGetWarehouseReceiptByIdUseCase, giống hệt cách ShowDetailAsync đã làm khi mở lần đầu.
    // Không set (mặc định rỗng) → CanNavigatePrev/Next luôn false, nút Trước/Sau disable (mờ)
    // chứ không ẩn hẳn khỏi toolbar.
    private IReadOnlyList<int> _siblingReceiptIds = Array.Empty<int>();
    private int _siblingIndex = -1;

    // CanExecute cho NavigatePrev/NavigateNextCommand.
    public bool CanNavigatePrev => _siblingIndex > 0;
    public bool CanNavigateNext => _siblingIndex >= 0 && _siblingIndex < _siblingReceiptIds.Count - 1;

    private void NotifyNavigationChanged()
    {
        NavigatePrevCommand.NotifyCanExecuteChanged();
        NavigateNextCommand.NotifyCanExecuteChanged();
    }

    public void SetSiblingContext(IReadOnlyList<int> receiptIds, int currentIndex)
    {
        _siblingReceiptIds = receiptIds;
        _siblingIndex      = currentIndex;
        NotifyNavigationChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePrev))]
    private async Task NavigatePrevAsync(CancellationToken ct = default)
    {
        if (_siblingIndex <= 0 || !ConfirmDiscardIfDirty()) return;
        await NavigateToSiblingAsync(_siblingIndex - 1, ct);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateNext))]
    private async Task NavigateNextAsync(CancellationToken ct = default)
    {
        if (_siblingIndex < 0 || _siblingIndex >= _siblingReceiptIds.Count - 1 || !ConfirmDiscardIfDirty()) return;
        await NavigateToSiblingAsync(_siblingIndex + 1, ct);
    }

    private async Task NavigateToSiblingAsync(int newIndex, CancellationToken ct)
    {
        IsLoading    = true;
        HasError     = false;
        ErrorMessage = string.Empty;
        try
        {
            var receipt = await _getReceiptById.ExecuteAsync(_siblingReceiptIds[newIndex], ct);
            if (receipt is null)
            {
                HasError     = true;
                ErrorMessage = "Không tìm thấy phiếu nhập kho.";
                return;
            }
            _siblingIndex = newIndex;
            ResetFormFor(receipt);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sibling warehouse receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            NotifyNavigationChanged();
        }
    }

    [RelayCommand]
    private void AddNew()
    {
        if (!ConfirmDiscardIfDirty()) return;
        // Thêm mới không còn nằm trong danh sách anh/em cũ — reset context để Trước/Sau không
        // trỏ nhầm sang phiếu khác cho tới khi phiếu mới này được Ghi sổ và mở lại từ danh sách.
        _siblingReceiptIds = Array.Empty<int>();
        _siblingIndex      = -1;
        ResetFormFor(null);
        NotifyNavigationChanged();
    }

    // Dùng chung cho Trước/Sau/Thêm — khớp text cảnh báo đã dùng ở WarehouseReceiptFormWindow.OnClosing.
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

    // Đưa form về trạng thái phiếu mới (existing=null) hoặc nạp lại từ 1 phiếu khác (existing!=null)
    // — dùng chung bởi LoadAsync (mở lần đầu) và Trước/Sau/Thêm (nạp lại ngay trong popup đang mở).
    // PHẢI Lines.Clear() trước — PopulateFromExisting/vòng lặp N dòng trống bên dưới đều Add thêm
    // vào Lines chứ không tự dọn, gọi lại mà không Clear sẽ chồng dòng cũ.
    private void ResetFormFor(WarehouseReceiptResponseDto? existing)
    {
        Lines.Clear();
        ReceiptId                = null;
        ReceiptNumber            = string.Empty;
        Status                   = "Draft";
        SelectedReceiptTypeIndex = 0;
        AccountingDate           = DateTime.Today;
        DocumentDate             = DateTime.Today;
        Description              = string.Empty;
        DeliveryPerson           = string.Empty;
        Reference                = string.Empty;
        SelectedObject           = null;
        SelectedEmployee         = null;

        _existingReceipt = existing;
        if (existing is not null)
            PopulateFromExisting(existing);

        for (var i = 0; i < InitialEmptyLineCount; i++) AddLine();

        RecalculateTotal();
        BeginDirtyTracking();
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            var suppliers = await _getSuppliers.ExecuteAsync(ct);
            var employees = await _getEmployees.ExecuteAsync(ct);
            var products  = await _getProducts.ExecuteAsync(ct);

            _customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            _suppliers = suppliers.Select(s => (ISearchableItem)new WarehouseObjectItem(s)).ToList().AsReadOnly();
            RebuildObjects();

            Employees = employees.Cast<ISearchableItem>().ToList().AsReadOnly();
            Products  = products.Where(p => p.IsActive).Select(p => (ISearchableItem)new WarehouseProductItem(p)).ToList().AsReadOnly();

            OnPropertyChanged(nameof(Employees));
            OnPropertyChanged(nameof(Products));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not preload lookup data for WarehouseReceiptForm");
        }

        // ResetFormFor tự lo populate (nếu có existing) + nạp N dòng trống để user gõ liền, không
        // cần bấm "Thêm dòng" trước — áp dụng cho cả tạo mới lẫn sửa (cho phép thêm hàng hóa khi
        // đang sửa). Dùng chung với Trước/Sau/Thêm để form luôn ở đúng 1 trạng thái nhất quán.
        ResetFormFor(_existingReceipt);
    }

    private void PopulateFromExisting(WarehouseReceiptResponseDto existing)
    {
        ReceiptId      = existing.Id;
        ReceiptNumber  = existing.ReceiptNumber;
        Status         = existing.Status;

        SelectedReceiptTypeIndex = existing.ReceiptType - 1;
        AccountingDate           = existing.AccountingDate;
        DocumentDate             = existing.DocumentDate;
        Description              = existing.Description    ?? string.Empty;
        DeliveryPerson           = existing.DeliveryPerson  ?? string.Empty;
        Reference                = existing.Reference       ?? string.Empty;

        if (existing.CustomerId is int customerId)
            SelectedObject = Objects.FirstOrDefault(o => o.Id == customerId && o is not WarehouseObjectItem);
        else if (existing.SupplierId is int supplierId)
            SelectedObject = Objects.FirstOrDefault(o => o.Id == supplierId && o is WarehouseObjectItem { Type: WarehouseObjectType.Supplier });

        if (existing.EmployeeId is int employeeId)
            SelectedEmployee = Employees.FirstOrDefault(e => e.Id == employeeId);

        foreach (var lineDto in existing.Lines)
        {
            var line = new WarehouseReceiptLineItem();
            line.PropertyChanged += (_, _) => { RecalculateTotal(); IsDirty = true; };

            // Set SelectedProduct trước — trigger OnSelectedProductChanged tự điền giá trị mặc
            // định (Quantity=1/TK Nợ=111/TK Có=131); set lại các field bên dưới với dữ liệu thật
            // đã lưu để ghi đè mặc định đó.
            line.SelectedProduct = Products.FirstOrDefault(p => p.Id == lineDto.ProductId);
            line.Quantity        = lineDto.Quantity;
            line.UnitPrice        = lineDto.UnitPrice;
            line.DebitAccount     = lineDto.DebitAccount;
            line.CreditAccount    = lineDto.CreditAccount;
            line.CostItem              = lineDto.CostItem              ?? string.Empty;
            line.CostObject            = lineDto.CostObject            ?? string.Empty;
            line.Project                = lineDto.Project               ?? string.Empty;
            line.PurchaseOrderNumber   = lineDto.PurchaseOrderNumber   ?? string.Empty;
            line.SalesContractNumber   = lineDto.SalesContractNumber   ?? string.Empty;
            line.LoanContractNumber    = lineDto.LoanContractNumber    ?? string.Empty;
            line.StatisticsCode        = lineDto.StatisticsCode        ?? string.Empty;
            line.Amount                = lineDto.Amount;

            Lines.Add(line);
        }
    }

    private void RebuildObjects()
    {
        Objects = _customers.Concat(_suppliers).ToList().AsReadOnly();
        OnPropertyChanged(nameof(Objects));
    }

    [RelayCommand]
    private void AddLine()
    {
        var line = new WarehouseReceiptLineItem();
        line.PropertyChanged += (_, _) => { RecalculateTotal(); IsDirty = true; };
        Lines.Add(line);
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveLine(WarehouseReceiptLineItem line)
    {
        Lines.Remove(line);
        RecalculateTotal();
        IsDirty = true;
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
        var before = _customers.Select(c => c.Id).ToHashSet();
        var window = _customerFormWindowFactory();
        window.Initialize(null);
        if (window.ShowDialog() != true) return;
        try
        {
            var customers = await _getCustomers.ExecuteAsync(ct);
            _customers = customers.Cast<ISearchableItem>().ToList().AsReadOnly();
            RebuildObjects();
            var newItem = _customers.FirstOrDefault(c => !before.Contains(c.Id));
            if (newItem is not null) SelectedObject = newItem;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not reload customers after add"); }
    }

    private void RecalculateTotal()
        => TotalAmount = Lines.Sum(l => l.Amount);

    [RelayCommand(CanExecute = nameof(IsEditable))]
    private async Task SaveAsync(CancellationToken ct = default)
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        // Grid nạp sẵn N dòng trống khi mở form — dòng nào chưa chọn hàng hóa (SelectedProduct
        // null) coi như bỏ trống, không phải lỗi; chỉ validate + gửi lên BE các dòng đã điền.
        var filledLines = Lines.Where(l => l.SelectedProduct is not null).ToList();

        if (filledLines.Count == 0)
        {
            HasError     = true;
            ErrorMessage = "Vui lòng thêm ít nhất một dòng hàng hóa.";
            return;
        }

        if (filledLines.Any(l => l.Quantity <= 0))
        {
            HasError     = true;
            ErrorMessage = "Số lượng phải lớn hơn 0 cho tất cả các dòng.";
            return;
        }

        IsLoading = true;
        try
        {
            var selectedCustomerId = (SelectedObject as WarehouseObjectItem)?.Type == WarehouseObjectType.Supplier
                ? null
                : SelectedObject?.Id;
            var selectedSupplierId = (SelectedObject as WarehouseObjectItem)?.Type == WarehouseObjectType.Supplier
                ? SelectedObject?.Id
                : null;

            var accountingDateUtc = DateTime.SpecifyKind(AccountingDate.Date, DateTimeKind.Unspecified);
            var documentDateUtc   = DateTime.SpecifyKind(DocumentDate.Date,   DateTimeKind.Unspecified);
            var description       = string.IsNullOrWhiteSpace(Description)    ? null : Description.Trim();
            var deliveryPerson    = string.IsNullOrWhiteSpace(DeliveryPerson) ? null : DeliveryPerson.Trim();
            var reference         = string.IsNullOrWhiteSpace(Reference)      ? null : Reference.Trim();
            var lineDtos          = filledLines.Select(l => new CreateWarehouseReceiptLineDto
            {
                ProductId           = l.SelectedProduct!.Id,
                // Kho ngầm định của sản phẩm (HH/TB, xem cột "Kho") — fallback kho "HH" (Id=4)
                // nếu sản phẩm chưa gán kho ngầm định.
                WarehouseId         = (l.SelectedProduct as WarehouseProductItem)?.DefaultWarehouseId ?? 4,
                Quantity            = l.Quantity,
                UnitPrice           = l.UnitPrice,
                Amount              = l.Amount,
                DebitAccount        = l.DebitAccount,
                CreditAccount       = l.CreditAccount,
                CostItem            = string.IsNullOrWhiteSpace(l.CostItem)            ? null : l.CostItem.Trim(),
                CostObject          = string.IsNullOrWhiteSpace(l.CostObject)          ? null : l.CostObject.Trim(),
                Project             = string.IsNullOrWhiteSpace(l.Project)             ? null : l.Project.Trim(),
                PurchaseOrderNumber = string.IsNullOrWhiteSpace(l.PurchaseOrderNumber) ? null : l.PurchaseOrderNumber.Trim(),
                SalesContractNumber = string.IsNullOrWhiteSpace(l.SalesContractNumber) ? null : l.SalesContractNumber.Trim(),
                LoanContractNumber  = string.IsNullOrWhiteSpace(l.LoanContractNumber)  ? null : l.LoanContractNumber.Trim(),
                StatisticsCode      = string.IsNullOrWhiteSpace(l.StatisticsCode)      ? null : l.StatisticsCode.Trim(),
            }).ToList();

            if (ReceiptId is int id)
            {
                // Sửa phiếu đã tồn tại (chỉ khả thi sau khi "Bỏ ghi" — phiếu về Draft) — Update
                // rồi Ghi sổ lại, cùng ý nghĩa với "Save = Create + Confirm" ở nhánh tạo mới.
                var updateRequest = new UpdateWarehouseReceiptRequestDto
                {
                    ReceiptType    = SelectedReceiptType,
                    CustomerId     = selectedCustomerId,
                    SupplierId     = selectedSupplierId,
                    EmployeeId     = SelectedEmployee?.Id,
                    AccountingDate = accountingDateUtc,
                    DocumentDate   = documentDateUtc,
                    Description    = description,
                    DeliveryPerson = deliveryPerson,
                    Reference      = reference,
                    Lines          = lineDtos,
                };

                await _updateUseCase.ExecuteAsync(id, updateRequest, ct);
                var confirmed = await _confirmUseCase.ExecuteAsync(id, ct);
                _logger.LogInformation("Warehouse receipt updated and re-confirmed: {ReceiptNumber}", confirmed.ReceiptNumber);
            }
            else
            {
                var request = new CreateWarehouseReceiptRequestDto
                {
                    ReceiptType    = SelectedReceiptType,
                    CustomerId     = selectedCustomerId,
                    SupplierId     = selectedSupplierId,
                    EmployeeId     = SelectedEmployee?.Id,
                    AccountingDate = accountingDateUtc,
                    DocumentDate   = documentDateUtc,
                    Description    = description,
                    DeliveryPerson = deliveryPerson,
                    Reference      = reference,
                    Lines          = lineDtos,
                };

                var result = await _createUseCase.ExecuteAsync(request, ct);
                await _confirmUseCase.ExecuteAsync(result.Id, ct);
                _logger.LogInformation("Warehouse receipt created and confirmed: {ReceiptNumber}", result.ReceiptNumber);
            }

            StopDirtyTracking();
            RequestClose?.Invoke(true);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create warehouse receipt");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally { IsLoading = false; }
    }

    // "In" ở toolbar — chỉ khả dụng khi đang xem 1 phiếu đã Ghi sổ (mở từ "Nhập, Xuất Kho"),
    // dùng đúng bản đã tải qua Initialize()/PopulateFromExisting — form không cho sửa khi
    // IsConfirmed nên dữ liệu không lệch. Chưa resolve được địa chỉ đối tác (WarehouseObjectItem
    // không mang Address) — truyền null như PrintWindow đã hỗ trợ.
    [RelayCommand(CanExecute = nameof(IsConfirmed))]
    private void Print()
    {
        if (_existingReceipt is null) return;
        var window = _printWindowFactory();
        window.Initialize(_existingReceipt, null);
        window.ShowDialog();
    }

    [RelayCommand(CanExecute = nameof(IsConfirmed))]
    private async Task UnconfirmAsync(CancellationToken ct = default)
    {
        if (ReceiptId is not int id) return;

        var r = MessageBox.Show(
            "Bạn có chắc muốn bỏ ghi sổ phiếu này? Sau khi bỏ ghi, phiếu sẽ quay về trạng thái nháp để chỉnh sửa.",
            "Xác nhận bỏ ghi",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (r != MessageBoxResult.Yes) return;

        HasError     = false;
        ErrorMessage = string.Empty;
        IsLoading    = true;
        try
        {
            var result = await _unconfirmUseCase.ExecuteAsync(id, ct);
            Status = result.Status;
            _logger.LogInformation("Unconfirmed warehouse receipt {ReceiptNumber}", result.ReceiptNumber);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unconfirm warehouse receipt {Id}", id);
            HasError     = true;
            ErrorMessage = ex.Message;
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
