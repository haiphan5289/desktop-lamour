# HomePage — Logic Overview

## Flow

```
App startup
    └── MainWindow boots
            └── NavigateTo("HomeView")
                    └── HomeView loads
                            └── HomeViewModel injected via DI
```

## Screen: HomeView

Hiển thị 4 navigation tiles chia thành 2 section groups. Không load data.

```
┌─────────────────────────────────────────┐
│  Tổng quan                              │
│  Chọn mục để bắt đầu quản lý           │
├─────────────────────────────────────────┤
│  Quản lý                                │
│  ┌─────────────┐   ┌─────────────────┐  │
│  │     👥      │   │      👤         │  │
│  │  Khách hàng │   │   Nhân viên     │  │
│  └─────────────┘   └─────────────────┘  │
│                                         │
│  Kho & Hàng hóa                         │
│  ┌─────────────┐   ┌─────────────────┐  │
│  │     📦      │   │      🏪         │  │
│  │  Sản phẩm   │   │  Nhà cung cấp   │  │
│  └─────────────┘   └─────────────────┘  │
└─────────────────────────────────────────┘
```

### Click actions

| Section | Item | Command | Navigates to |
|---|---|---|---|
| Quản lý | Khách hàng | `NavigateToCustomersCommand` | `CustomerListView` |
| Quản lý | Nhân viên | `NavigateToEmployeesCommand` | `EmployeeListView` |
| Kho & Hàng hóa | Sản phẩm | `NavigateToProductsCommand` | `ProductListView` |
| Kho & Hàng hóa | Nhà cung cấp | `NavigateToSuppliersCommand` | `SupplierListView` |

---

## Screen: ProductListView

Màn hình danh sách sản phẩm. Hiện tại là stub — sẽ implement đầy đủ sau.

```
┌─────────────────────────────────────────┐
│  ← Quay lại                            │
│  Danh sách sản phẩm                     │
├─────────────────────────────────────────┤
│                  📦                     │
│           Đang phát triển...            │
└─────────────────────────────────────────┘
```

| Command | Action |
|---|---|
| `GoBackCommand` | `NavigationService.GoBack()` → trở về HomeView |

---

## Screen: SupplierListView

Màn hình nhà cung cấp. Hiện tại là stub — sẽ implement đầy đủ sau.

```
┌─────────────────────────────────────────┐
│  ← Quay lại                            │
│  Nhà cung cấp                           │
├─────────────────────────────────────────┤
│                  🏪                     │
│           Đang phát triển...            │
└─────────────────────────────────────────┘
```

| Command | Action |
|---|---|
| `GoBackCommand` | `NavigationService.GoBack()` → trở về HomeView |

---

## Navigation Stack

```
[HomeView]
    ├── click Sản phẩm  → push ProductListView  → GoBack → pop → HomeView
    └── click Nhà cung cấp → push SupplierListView → GoBack → pop → HomeView
```

---

## ViewModel summary

### HomeViewModel

| Member | Type | Description |
|---|---|---|
| `NavigateToProductsCommand` | `IRelayCommand` | Navigate to ProductListView |
| `NavigateToSuppliersCommand` | `IRelayCommand` | Navigate to SupplierListView |
| `NavigateToCustomersCommand` | `IRelayCommand` | Navigate to CustomerListView |
| `NavigateToEmployeesCommand` | `IRelayCommand` | Navigate to EmployeeListView |

### ProductListViewModel

| Member | Type | Description |
|---|---|---|
| `GoBackCommand` | `IRelayCommand` | Pop back to HomeView |

### SupplierListViewModel

| Member | Type | Description |
|---|---|---|
| `GoBackCommand` | `IRelayCommand` | Pop back to HomeView |

---

## File structure

```
Features/HomePage/
├── HOMEPAGE.md                              ← this file
├── HomeServiceCollectionExtensions.cs
├── Home/
│   ├── docs/home.md
│   ├── ViewModels/HomeViewModel.cs
│   └── Views/HomeView.xaml
├── Customers/
│   ├── docs/customers.md
│   ├── Domain/ | Data/ | ViewModels/ | Views/
├── Employees/
│   ├── docs/employees.md
│   ├── Domain/ | Data/ | ViewModels/ | Views/
├── ProductList/
│   ├── docs/product-list.md
│   └── ...
└── Suppliers/
    ├── docs/suppliers.md
    └── ...
```

---

## DI Registration (`HomeServiceCollectionExtensions`)

```csharp
services.AddTransient<HomeView>();
services.AddTransient<ProductListView>();
services.AddTransient<SupplierListView>();
services.AddTransient<HomeViewModel>();
services.AddTransient<ProductListViewModel>();
services.AddTransient<SupplierListViewModel>();
```

---

## Cross-cutting: Dirty State & Unsaved-Changes Popup

Tất cả 5 form windows (Customer, Employee, Product, Supplier, WarehouseReceipt) đều có chức năng hiện popup xác nhận khi người dùng đóng form mà chưa lưu dữ liệu.

### Behavior

| Trigger | Dirty? | Kết quả |
|---|---|---|
| Nhấn **Hủy** | Không (form trống) | Đóng ngay, không hỏi |
| Nhấn **Hủy** | Có (đã nhập dữ liệu) | Hiện popup → Có = đóng, Không = ở lại |
| Nhấn **X** (title bar) | Không | Đóng ngay |
| Nhấn **X** (title bar) | Có | Hiện popup → Có = đóng, Không = ở lại |
| Lưu thành công | — | `IsDirty` reset, đóng không hỏi |

Nội dung popup:
> **"Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất."**
> (MessageBox.YesNo, Warning icon)

### Architecture

**`ViewModelBase`** (`Core/ViewModels/ViewModelBase.cs`) chứa toàn bộ dirty-tracking infrastructure:

```csharp
public abstract partial class ViewModelBase : ObservableObject
{
    // Các prop này KHÔNG trigger dirty (computed/infrastructure):
    private static readonly HashSet<string> _noDirtyProps = new()
    {
        nameof(IsDirty), "IsLoading", "HasError", "ErrorMessage", "WindowTitle",
        "IsAddMode", "IsEditMode", "TotalAmount", "SelectedReceiptType"
    };

    private bool _dirtyTracking;

    [ObservableProperty] private bool _isDirty;

    protected void BeginDirtyTracking() { _dirtyTracking = true; IsDirty = false; }
    protected void StopDirtyTracking()  { _dirtyTracking = false; IsDirty = false; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_dirtyTracking && !_noDirtyProps.Contains(e.PropertyName!))
            IsDirty = true;
    }
}
```

### ViewModel pattern (mỗi FormViewModel)

```csharp
// 1. Cuối Initialize() / LoadAsync() — sau khi set tất cả fields:
BeginDirtyTracking();

// 2. Trước RequestClose?.Invoke(true) trong SaveAsync():
StopDirtyTracking();
RequestClose?.Invoke(true);

// 3. Cancel command — check dirty trước khi đóng:
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
```

### FormWindow pattern (mỗi FormWindow.xaml.cs)

```csharp
// OnClosing intercepts X button close:
protected override void OnClosing(CancelEventArgs e)
{
    base.OnClosing(e);
    // DialogResult is null  = X button (chưa set)
    // DialogResult = false  = Cancel command đã xử lý (không hỏi 2 lần)
    // DialogResult = true   = Save thành công (không hỏi)
    if (ViewModel.IsDirty && DialogResult is null)
    {
        var r = MessageBox.Show(
            "Bạn có chắc muốn thoát? Dữ liệu chưa lưu sẽ bị mất.",
            "Xác nhận thoát",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (r != MessageBoxResult.Yes) e.Cancel = true;
    }
}
```

### Ghi chú đặc biệt

**CustomerFormViewModel** — `LoadNextCodeAsync()` chạy async sau `BeginDirtyTracking()` và tự động set field `Code` (auto-generated). Cần reset sau khi load xong để tránh false dirty:
```csharp
private async Task LoadNextCodeAsync()
{
    // ... load code from API ...
    IsDirty = false; // Code là auto-generated, không phải user input
}
```

**WarehouseReceiptFormViewModel** — `Lines` là `ObservableCollection<WarehouseReceiptLineItem>`. Collection changes không bubble qua `OnPropertyChanged` của ViewModel → cần explicit dirty marking:
```csharp
[RelayCommand]
private void AddLine()
{
    var line = new WarehouseReceiptLineItem();
    line.PropertyChanged += (_, _) => { RecalculateTotal(); IsDirty = true; };
    Lines.Add(line);
    IsDirty = true; // explicit vì ObservableCollection không tự bubble
}

[RelayCommand]
private void RemoveLine(WarehouseReceiptLineItem line)
{
    Lines.Remove(line);
    RecalculateTotal();
    IsDirty = true;
}
```

### Files đã sửa

| File | Thay đổi |
|---|---|
| `Core/ViewModels/ViewModelBase.cs` | Thêm `IsDirty`, `_dirtyTracking`, `BeginDirtyTracking()`, `StopDirtyTracking()`, override `OnPropertyChanged` |
| `Customers/ViewModels/CustomerFormViewModel.cs` | `BeginDirtyTracking()` cuối `Initialize()`, `IsDirty = false` sau `LoadNextCodeAsync()`, `StopDirtyTracking()` trước save, dirty-check Cancel |
| `Employees/ViewModels/EmployeeFormViewModel.cs` | `BeginDirtyTracking()` cuối `Initialize()`, dirty-check Cancel |
| `ProductList/ViewModels/ProductFormViewModel.cs` | `BeginDirtyTracking()` cuối `Initialize()`, dirty-check Cancel |
| `Suppliers/ViewModels/SupplierFormViewModel.cs` | `BeginDirtyTracking()` cuối `Initialize()`, dirty-check Cancel |
| `Warehouse/ViewModels/WarehouseReceiptFormViewModel.cs` | `BeginDirtyTracking()` cuối `LoadAsync()`, explicit dirty trên `AddLine`/`RemoveLine`/`line.PropertyChanged` |
| `Customers/Views/CustomerFormWindow.xaml.cs` | `OnClosing` override với `IsDirty && DialogResult is null` check |
| `Employees/Views/EmployeeFormWindow.xaml.cs` | `OnClosing` override |
| `ProductList/Views/ProductFormWindow.xaml.cs` | `OnClosing` override |
| `Suppliers/Views/SupplierFormWindow.xaml.cs` | `OnClosing` override |
| `Warehouse/Views/WarehouseReceiptFormWindow.xaml.cs` | `OnClosing` override |

---

## Next steps

- [ ] Implement `ProductListView` — fetch & display product list from API
- [ ] Implement `SupplierListView` — fetch & display supplier list from API
- [ ] Add search / filter on each list screen
- [ ] Add role-based access control (Admin / Thu ngân / Kho)
