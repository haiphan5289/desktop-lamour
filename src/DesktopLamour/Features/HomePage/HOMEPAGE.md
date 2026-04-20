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

Hiển thị 2 navigation item (icon + text). Không load data.

```
┌─────────────────────────────────────────┐
│  Tổng quan                              │
│  Chọn mục để bắt đầu quản lý           │
├─────────────────────────────────────────┤
│  ┌─────────────┐   ┌─────────────────┐  │
│  │     📦      │   │      🏪         │  │
│  │  Sản phẩm   │   │  Nhà cung cấp   │  │
│  └─────────────┘   └─────────────────┘  │
└─────────────────────────────────────────┘
```

### Click actions

| Item | Command | Navigates to |
|---|---|---|
| Sản phẩm | `NavigateToProductsCommand` | `ProductListView` |
| Nhà cung cấp | `NavigateToSuppliersCommand` | `SupplierListView` |

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
├── Domain/
│   └── (reserved for future data models)
├── Data/
│   └── (reserved for future API calls)
├── ViewModels/
│   ├── HomeViewModel.cs
│   ├── ProductListViewModel.cs
│   └── SupplierListViewModel.cs
└── Views/
    ├── HomeView.xaml / .xaml.cs
    ├── ProductListView.xaml / .xaml.cs
    └── SupplierListView.xaml / .xaml.cs
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
