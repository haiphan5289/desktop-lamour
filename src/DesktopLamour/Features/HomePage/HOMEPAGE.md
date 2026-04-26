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

## Next steps

- [ ] Implement `ProductListView` — fetch & display product list from API
- [ ] Implement `SupplierListView` — fetch & display supplier list from API
- [ ] Add search / filter on each list screen
- [ ] Add role-based access control (Admin / Thu ngân / Kho)
