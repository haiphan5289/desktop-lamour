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
- [ ] Form: ô Mã luôn read-only; Add mode hiện mã dự kiến từ `next-code`

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

## Changelog — 2026-10-05: Lưới "Vật tư hàng hóa" khớp màn MISA

> Yêu cầu qua `/ct-be-to-desktop` kèm ảnh màn "Vật tư hàng hóa" của MISA. **Chỉ đổi WPF** — toàn bộ dữ liệu đã có sẵn trong `GET /api/v1/products`, không đổi BE, không migration.

- **Tiêu đề** "Danh sách sản phẩm" → "Vật tư hàng hóa".
- **Cột lưới** (thay bộ cột cũ): Mã · Tên · Tính chất (`Nature`) · Nhóm VTHH (`CategoryName`) · ĐVT chính (`Unit`) · Số lượng tồn (`StockQuantity`) · Giá trị tồn · Giảm thuế theo QĐ (`TaxReductionType`) · Ngừng theo dõi. **Bỏ** 2 cột Giá nhập/Giá bán (vẫn xem/sửa trong popup Sửa).
- **Giá trị tồn** = `StockQuantity × CostPrice`, tính tại client (`Product.StockValue`) — sản phẩm chưa nhập giá sẽ ra 0.
- **Ngừng theo dõi** = đảo của `IsActive` (`Product.IsStopTracking`). Cột bind `Mode=OneWay` vì property chỉ có getter.
- **Dòng lọc theo từng cột** nhúng trong header (pattern `CustomerListView`): cột chữ lọc Contains; 2 cột số dùng `NumericColumnFilter` (toán tử + giá trị); Ngừng theo dõi dùng checkbox 3 trạng thái (▪ tất cả / ✓ chỉ ngừng / trống chỉ đang theo dõi). Tất cả AND với nhau và với dropdown Nhóm VTHH. (Ô "Tìm kiếm vật tư hàng hóa..." chung đã bỏ 2026-10-10 vì dư so với ô lọc từng cột — `SearchText` trong `ProductListViewModel` đã xóa.)
- **Dropdown "Nhóm vật tư, hàng hóa, dịch vụ"** — nạp từ `IGetCategoriesUseCase` (inject thêm vào `ProductListViewModel`), mục đầu "Tất cả".
- **Footer**: "Số dòng = N" + Tổng số lượng tồn + Tổng giá trị tồn, tính trên các dòng đang hiển thị sau lọc.
- **Nút "🔄 Nạp"** — gọi lại `LoadProductsCommand`.
- **Không đổi**: Xuất/Nhập khẩu Excel vẫn giữ bộ cột cũ (Mã sản phẩm/Tên sản phẩm/Danh mục/Đơn vị/Giá nhập/Giá bán/Tồn kho) để file xuất ra nhập lại được qua `import-excel`. Chưa làm các nút Sửa hàng loạt / Tra cứu mặt hàng giảm thuế / In / Góp ý / Giúp.

---

## Changelog — 2026-10-05 (2): Mã vật tư hàng hóa tự động tăng

> Yêu cầu: "chỗ Mã là tự động tăng số luôn, chứ mình không tự nhập". Chốt: dạng **số nối tiếp dãy MISA** (mã số lớn nhất + 1, vd 69 → 70), ô Mã **khoá cả Thêm và Sửa**.

- `ProductFormWindow.xaml`: ô "Mã" → nhãn "Mã (tự động tăng)", bỏ dấu `*`, `IsReadOnly="True"` + binding `OneWay` (dùng `IsReadOnly` thay `IsEnabled="False"` để chữ không bị theme làm mờ).
- `ProductFormViewModel`: inject `IProductService`; Add mode gọi `GetNextCodeAsync` (đầu `LoadLookupsAsync`) để hiện **mã dự kiến**. Khi Cất gửi `Code = ""` — BE mới là nơi cấp mã thật, nên 2 máy cùng mở form không bị báo trùng mã ở client.
- `IProductService`/`ProductService`: thêm `GetNextCodeAsync` → `GET /api/v1/products/next-code`.
- Nhân bản: BE cấp mã số kế tiếp (không còn đuôi `_COPY`).
- Nhập khẩu Excel **không đổi** — vẫn lấy mã từ cột "Mã sản phẩm" trong file.

---

*Generated by `/ct-ai-document` on 2026-04-25*


*Updated 2026-10-08 (danh sách Vật tư hàng hóa — nhấp đúp dòng mở form Sửa như MISA): lưới gắn `DataGridRowDoubleClickBehavior.Command` (`Shared/Behaviors`) → `EditProductCommand`, cùng hành vi nút "Sửa" (đóng form xong tải lại danh sách). Chỉ kích hoạt khi nhấp đúp trúng `DataGridRow` — nhấp đúp tiêu đề cột/ô lọc/vùng trống không mở gì. Cùng behavior được gắn cho Vật tư hàng hóa, Nhà cung cấp, Khách hàng, Nhân viên. Không đổi BE.*
