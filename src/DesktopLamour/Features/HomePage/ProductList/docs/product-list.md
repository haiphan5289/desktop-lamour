# ProductList — Feature Document (App)

> **Jira:** — | **Branch:** `dev` | **Generated:** 2026-04-25

---

## PRD Summary

> Module quản lý danh sách sản phẩm mỹ phẩm trong WPF Desktop Lamour.

- **Goal:** Cho phép quản lý hàng hóa: xem danh sách, thêm/sửa/xóa/nhân bản sản phẩm.
- **User story:** As a Lamour warehouse manager, I want to manage product inventory in the desktop app so that stock levels and pricing are always up to date.
- **Acceptance criteria:**
  - [x] Hiển thị danh sách sản phẩm với cột: Mã, Tên, Danh mục, Đơn vị, Giá vốn, Giá bán, Tồn kho, Trạng thái
  - [x] Form thêm/sửa với `code` user-entered (required, unique)
  - [x] Nhân bản với code `_COPY`
  - [x] Xóa có confirm dialog

---

## Business Rules

| Rule | Description |
|------|-------------|
| Code user-entered | Người dùng tự nhập code (khác với Customer tự sinh) |
| Code editable in Add mode | Code có thể nhập khi Thêm, read-only khi Sửa |
| Validate tại UseCase | `CreateProductUseCase` check code/name + unique |
| is_active | Sản phẩm ngừng kinh doanh không bị xóa |

---

## Architecture Overview

### Key Components

| Layer | File | Role |
|-------|------|------|
| View | `Views/ProductListView.xaml` | DataGrid + toolbar |
| View | `Views/ProductFormWindow.xaml` | Form thêm/sửa |
| ViewModel | `ViewModels/ProductListViewModel.cs` | List state + commands |
| ViewModel | `ViewModels/ProductFormViewModel.cs` | Form state + Save/Cancel |
| UseCase | `Domain/UseCases/GetProductsUseCase.cs` | Fetch list |
| UseCase | `Domain/UseCases/CreateProductUseCase.cs` | Validate + create |
| UseCase | `Domain/UseCases/UpdateProductUseCase.cs` | Validate + update |
| UseCase | `Domain/UseCases/DeleteProductUseCase.cs` | Delete |
| UseCase | `Domain/UseCases/DuplicateProductUseCase.cs` | Clone |
| Repository | `Data/Repositories/ProductRepository.cs` | DTO ↔ Domain map |
| Service | `Data/Services/ProductService.cs` | HttpClient |

### Data Flow

```
ProductListView (Loaded)
  → ProductListViewModel.LoadProductsCommand
  → IGetProductsUseCase → IProductRepository → IProductService
  → GET /api/v1/products
  ← IEnumerable<Product> → ObservableCollection<Product>
```

```mermaid
graph TD
    A[ProductListView] --> B[ProductListViewModel]
    B --> C[IGetProductsUseCase]
    B --> D[IDeleteProductUseCase]
    B --> E[IDuplicateProductUseCase]
    B --> F[ProductFormWindow]
    F --> G[ProductFormViewModel]
    G --> H[ICreateProductUseCase]
    G --> I[IUpdateProductUseCase]
    C --> J[IProductRepository]
    D --> J
    E --> J
    H --> J
    I --> J
    J --> K[IProductService → HttpClient → BE]
```

---

## Key Files & Symbols

### Presentation
- [`Views/ProductListView.xaml`](../Views/ProductListView.xaml) — DataGrid, 4 toolbar buttons
- [`Views/ProductListView.xaml.cs`](../Views/ProductListView.xaml.cs) — `Loaded` → `LoadProductsCommand`
- [`Views/ProductFormWindow.xaml`](../Views/ProductFormWindow.xaml) — Form fields
- [`Views/ProductFormWindow.xaml.cs`](../Views/ProductFormWindow.xaml.cs) — `Initialize(Product?)`
- [`ViewModels/ProductListViewModel.cs`](../ViewModels/ProductListViewModel.cs) — Commands: Load, Add, Edit, Delete, Duplicate, GoBack
- [`ViewModels/ProductFormViewModel.cs`](../ViewModels/ProductFormViewModel.cs) — Fields: Code, Name, Category, Unit, CostPrice, SellingPrice, StockQuantity, IsActive

### Domain
- [`Domain/Models/Product.cs`](../Domain/Models/Product.cs) — `Id`, `Code`, `Name`, `Category`, `Unit`, `CostPrice`, `SellingPrice`, `StockQuantity`, `IsActive`
- [`Domain/UseCases/CreateProductInput.cs`](../Domain/UseCases/CreateProductInput.cs) — record: all fields
- [`Domain/UseCases/UpdateProductInput.cs`](../Domain/UseCases/UpdateProductInput.cs) — record: `Id` + all fields

### Data
- [`Data/Services/IProductService.cs`](../Data/Services/IProductService.cs) — `GetAllAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `DuplicateAsync`
- [`Data/Services/ProductService.cs`](../Data/Services/ProductService.cs) — HttpClient typed service
- [`Data/Services/Dtos/ProductResponseDto.cs`](../Data/Services/Dtos/ProductResponseDto.cs) — snake_case JSON

---

## API Contracts

| Method | Endpoint | Output |
|--------|----------|--------|
| `GET` | `/api/v1/products` | `ProductResponseDto[]` |
| `POST` | `/api/v1/products` | `ProductResponseDto` (201) |
| `PUT` | `/api/v1/products/{id}` | `ProductResponseDto` |
| `DELETE` | `/api/v1/products/{id}` | 204 |
| `POST` | `/api/v1/products/{id}/duplicate` | `ProductResponseDto` (201) |

---

## Edge Cases & Error Handling

| Scenario | Expected Behavior | Handled? |
|----------|------------------|----------|
| BE không chạy | Error banner | ✅ |
| Code trùng | `ValidationException` → form error | ✅ |
| Tên/Code trống | `ValidationException` → form error | ✅ |
| Duplicate `_COPY` tồn tại | API 400 → error banner | ✅ |
| Confirm xóa → No | Không xóa | ✅ |

---

## Test Coverage Notes

| Component | Test File | Coverage |
|-----------|-----------|----------|
| `ProductListViewModel` | — | ❌ Missing |
| `ProductFormViewModel` | — | ❌ Missing |
| `CreateProductUseCase` | — | ❌ Missing |

**Suggested test cases:**
- [ ] Load: data từ BE → collection populated
- [ ] Create: code trùng → `ValidationException`
- [ ] Form: IsAddMode = true → Code enabled; IsAddMode = false → Code disabled

---

## Notes

- DI: `AddHttpClient<IProductService, ProductService>` base URL `http://192.168.64.1:5282`
- Navigation: `NavigationRoutes.Products.List = "ProductListView"`

---

## Changelog — 2026-08-09: Redesign `ProductFormWindow` theo popup MISA "Sửa Vật tư, hàng hoá, dịch vụ"

User gửi ảnh chụp popup tham khảo, yêu cầu áp dụng vào form Thêm/Sửa vật tư hàng hoá. Scope chốt: chỉ header + tab "Ngầm định", tách field Thuế (GTGT/NK/XK/TTĐB) ra tab riêng "Thuế".

**`ProductFormWindow.xaml`**: đổi từ 1 cột đơn giản, không tab, Width mặc định → `Width="900"`, `TabControl` 2 `TabItem` ("1. Ngầm định" 2-cột-Grid trái/phải, "2. Thuế" — y hệt layout cũ nhưng đặt trong tab). Thêm nút "💾 Cất & Thêm" cạnh "Hủy bỏ"/"💾 Cất".

**`ProductFormViewModel.cs`**: thêm ~20 `[ObservableProperty]` mới tương ứng field mới trên `Product` (xem [`products.md`](../../../../../../../be-window-lamour/src/Lamour.Application/Features/Products/docs/products.md) phía BE cho danh sách đầy đủ). Điểm đáng chú ý:
- `StopTracking` — property thủ công (không `[ObservableProperty]`) bọc nghịch đảo `IsActive`, để checkbox "Ngừng theo dõi" trong ảnh khớp đúng polarity với field `IsActive` sẵn có (không thêm cột DB mới)
- `SelectedProductUnit`/`SelectedDefaultWarehouse`/6× `Selected*Account` — tất cả đều `ISearchableItem?`, nguồn từ `ProductUnits`/`Warehouses`/`AccountSettings` (3 danh mục cài đặt build trước đó cùng batch — [`product-units.md`](../../ProductUnits/docs/product-units.md), [`account-settings.md`](../../AccountSettings/docs/account-settings.md))
- Khi lưu, `Unit` (string) tự đồng bộ từ `SelectedProductUnit?.Name` nếu có chọn — không phá vỡ nơi khác đang đọc `Product.Unit` string trực tiếp
- `SaveAsync`/`SaveAndAddNewAsync` refactor dùng chung `PersistAsync()` — khác nhau ở hành động sau khi lưu thành công (đóng dialog vs `Initialize(null)` để thêm tiếp)
- Constructor thêm 3 dependency mới: `IGetProductUnitsUseCase`, `IGetWarehouseSettingsUseCase`, `IGetAccountSettingsUseCase` + 2 window factory (`Func<ProductUnitFormWindow>`, `Func<WarehouseSettingFormWindow>`) cho nút "+" ở ĐVT chính/Kho ngầm định (giống pattern `AddCategoryCommand` có từ trước)

**`CreateProductInput`/`UpdateProductInput`**: đổi từ positional record (constructor 13 param) sang record với `init`-property — vì thêm ~20 field nữa vào constructor vị trí sẽ không đọc được. `CreateProductUseCase`/`UpdateProductUseCase` không đổi (chỉ đọc property theo tên, không phụ thuộc thứ tự).

**Model/DTOs**: `Product.cs`, `ProductResponseDto`/`CreateProductRequestDto`/`UpdateProductRequestDto` (3 file) đồng bộ 1:1 field/JSON key với phía BE.

**Converter mới**: `Shared/Converters/ProductNatureDisplayConverter.cs` (enum `ProductNature` → "Vật tư hàng hóa"/"Dịch vụ"), đăng ký trong `AppConverters.xaml`.

**Chưa làm** (ngoài phạm vi lần này): 3 tab còn lại trong ảnh gốc (Chiết khấu, Đơn vị chuyển đổi, Mã quy cách/hình ảnh) — chưa có entity/UI nào cho các tab này.

### Follow-up cùng ngày: default TK kế toán khi Thêm mới

User gửi thêm ảnh chụp popup mẫu (đã điền sẵn) và yêu cầu áp dụng đúng các giá trị mặc định cho **Thêm vật tư hàng hoá** (không áp dụng khi Sửa — Sửa luôn giữ giá trị đã lưu). `LoadLookupsAsync()`: mỗi khi `!_isEditMode`, tự chọn theo `Code` (không theo Id, vì Id seed có thể khác nhau giữa môi trường):

| Field | Default Code |
|---|---|
| Kho ngầm định | `HH` |
| Tài khoản kho | `1561` |
| TK doanh thu | `5111` |
| TK chiết khấu | `5211` |
| TK giảm giá | `5213` |
| TK trả lại | `5212` |
| TK chi phí | `632` |

3 code `5211`/`5212`/`5213` **chưa có** trong seed `account_settings` ban đầu (chỉ có dải `511x` doanh thu) — phải thêm migration BE mới (`AddDiscountReturnAccountSettings`) trước khi wire được default này, xem [`account-settings.md`](../../../../../../../be-window-lamour/src/Lamour.Application/Features/AccountSettings/docs/account-settings.md).

---

*Generated by `/ct-ai-document` on 2026-04-25*
