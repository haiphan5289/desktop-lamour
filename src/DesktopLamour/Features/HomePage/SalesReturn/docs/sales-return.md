# Chứng từ hàng bán bị trả lại — WPF Client Documentation

> Module: `Features/HomePage/SalesReturn`
> BE counterpart: `be-window-lamour/src/Lamour.Application/Features/SalesReturn/docs/sales-return.md`
> First documented: 2026-08-28 (doc mới — module trước đó chưa có `docs/` riêng phía WPF dù BE đã có)

## PRD Summary

Popup `SalesReturnWindow` quản lý chứng từ "Hàng bán bị trả lại" — layout đã được update lại nhiều lượt trong phiên 2026-08-22 → 2026-08-28 để khớp giao diện tham chiếu kiểu MISA (ảnh mẫu user cung cấp), cộng thêm workflow "Ghi sổ → In Hoá Đơn" tự động và "Lập PN → In Phiếu Nhập Kho" (nhập lại hàng vào kho khi khách trả hàng).

## Key Components

```
Features/HomePage/SalesReturn/
  Domain/Models/SalesReturnLineItem.cs      — 1 dòng sản phẩm, INotifyPropertyChanged thủ công
  ViewModels/SalesReturnViewModel.cs        — toàn bộ logic popup: Lines, 4 tab, Ghi sổ, Lập PN, In
  Views/SalesReturnWindow.xaml(.cs)         — popup chính, 2 TabControl lồng nhau
  Views/SalesReturnPrintWindow.xaml(.cs)    — MỚI (2026-08-22) — "PHIẾU TRẢ LẠI HÀNG BÁN"
```

Phụ thuộc chéo module khác (không sửa, chỉ gọi):
- `Warehouse/Views/WarehouseReceiptPrintWindow.xaml(.cs)` — dùng chung cho luồng "Lập PN" (xem `Warehouse/docs/warehouse.md`, changelog 2026-08-28).
- `Shared/Helpers/VietnameseNumberToWordsHelper.cs` — "Số tiền bằng chữ" trên cả 2 cửa sổ in.
- `Shared/Controls/BlankPreserveConverter` — hiển thị số 0/rỗng đúng chuẩn trên mọi cột số của 5 tab.

## Layout Redesign (2026-08-22, nhiều vòng, theo ảnh mẫu MISA)

`SalesReturnWindow.xaml` được redesign toàn diện qua nhiều request liên tiếp trong cùng phiên — tất cả đã hỏi qua `AskUserQuestion`/flipped-interaction trước khi build (nguyên tắc xuyên suốt: **không thêm cột/nút giả không có logic thật**, ví dụ đã từ chối thêm "Nhóm HHDV mua vào"/"Dự án đầu tư" ở tab Thuế và "Số lô"/"Hạn sử dụng" ở tab Giá vốn vì hệ thống không model các field này).

- **`Window.Resources`**: `AppTabItem.Modern`/`AppTabControl.Modern` (style pill-tab, copy từ `PaymentWindow.xaml` — quy ước hiện tại của app là khai lại per-window, không có ResourceDictionary chung), `RibbonButton` (icon-top/text-bottom, nền trong suốt, hover highlight), `GridHeaderBrush`/`GridHeaderStyle` (`#BDD7EE`, tái dùng pattern từ `WarehouseTransactionListView.xaml`/`AccountingView.xaml`).
- **Header**: thu gọn từ block branding lớn xuống 1 dòng compact.
- **Toolbar**: đổi hẳn sang ribbon-style, **chỉ giữ 4 nút có logic thật** — Ghi sổ / Lập PN / Xóa / Đóng (chủ động KHÔNG thêm Trước/Sau/Hoàn/Nạp/Tiện ích/Mẫu/Giúp — quyết định qua `AskUserQuestion`, chọn "chỉ restyle nút có logic thật").
- **"Loại trả hàng"**: ComboBox → 2 `RadioButton` bind `IsDebitReduction`/`IsCashReturn` (bool bridge property mới trên ViewModel, `OnReturnTypeChanged` đồng bộ cả hai).
- **Tab "1. Hàng tiền"**: cột reorder thành `Mã hàng, Tên hàng, Kho, TK trả lại, TK công nợ, ĐVT, Số lượng, Đơn giá, Thành tiền, Tỷ lệ CK(%), Tiền CK, TK CK, Số CT bán hàng`.
- **Tab "2. Thuế"**: layout theo ảnh mẫu — cột "Diễn giải thuế" tự sinh từ tên sản phẩm (xem `TaxDescription`, mục Bug Fixes bên dưới).
- **Tab "3. Giá vốn"**: layout theo 3 ảnh mẫu tham chiếu.
- **Tab "4. Thống kê"**: chỉ giữ "Đơn vị" (Department) — 6 field khác của MISA (Mã quy cách/Số lô/Hạn sử dụng/Số khế ước/...) không có data model, bỏ qua theo đúng nguyên tắc chung của dự án.
- **Footer**: 2×2 grid — `Tổng tiền hàng`/`Tiền thuế GTGT` hàng trên, `Tổng chiết khấu`/`Tổng tiền thanh toán` hàng dưới.
- **Grid style chung cho cả 4 tab**: `ColumnHeaderStyle="{StaticResource GridHeaderStyle}"`; mọi cột số (Số lượng/Đơn giá/Thành tiền/Tỷ lệ CK/Tiền CK/% thuế GTGT/Tiền thuế GTGT/Đơn giá vốn/Tiền vốn) đổi từ `StringFormat=` sang `Converter={StaticResource BlankPreserveConverter}` — hiện trống thay vì "0"/"0.00" cho dòng chưa có sản phẩm; `GridLinesVisibility="Horizontal"` → `"All"` + `HorizontalGridLinesBrush`/`VerticalGridLinesBrush="#9CB8D4"` để kẻ đường phân biệt rõ giữa các cột (yêu cầu riêng, sau khi 4 tab đã đủ dữ liệu).

## Workflow "Ghi sổ → In Hoá Đơn" (2026-08-22)

- `SalesReturnViewModel.SaveAsync` — sau khi Ghi sổ thành công, tự gọi `ShowPrintPreview(result)` (không cần bấm nút In riêng).
- `ShowPrintPreview(SalesReturnResponseDto)` — resolve `Customer` từ `SelectedCustomer`, mở `SalesReturnPrintWindow` (mới).
- **`SalesReturnPrintWindow.xaml(.cs)`** — "PHIẾU TRẢ LẠI HÀNG BÁN", tái dùng nguyên `ProductTableColumnWidths = { 26, 84, 26, 62, 42, 82, 84, 84 }` đã chốt ổn định từ `SalesOrderPrintWindow` (xem `Sales/docs/sales.md`, changelog cột-độ-rộng). Khác `SalesOrderPrintWindow` ở 2 điểm: field "Loại trả hàng" thay cho "PT giao hàng/thanh toán"; chữ ký "Người viết phiếu" thay vì "Người lập hóa đơn".

## Workflow "Lập PN → In Phiếu Nhập Kho" (2026-08-22)

- `CreateWarehouseReceiptAsync` — đổi từ hiện `MessageBox` báo thành công đơn thuần sang: gọi `IGetWarehouseReceiptByIdUseCase.ExecuteAsync(result.Id, ct)` lấy lại phiếu đầy đủ, resolve địa chỉ đối tác từ `Customers` list theo `receipt.CustomerId`, mở `Warehouse/Views/WarehouseReceiptPrintWindow` (Mẫu 01-VT chính thức — xem `Warehouse/docs/warehouse.md`).
- Constructor `SalesReturnViewModel` thêm `IGetWarehouseReceiptByIdUseCase`, `Func<WarehouseReceiptPrintWindow>` (factory, đăng ký trong `HomeServiceCollectionExtensions.cs`).

## Bug Fixes (2026-08-22)

### 1. Dòng trống vẫn hiện dữ liệu mặc định — 2 lượt fix (lượt đầu chưa đủ)

**Báo cáo lần 1** — dòng trống trong 5 tab hiện sẵn mã TK (5212/131/5211/33311/1561/632). **Lượt fix đầu tiên** chỉ sửa `AddLine()` để không gán các mã này ngay khi tạo dòng — KHÔNG đủ, vì `SalesReturnLineItem` có **field initializer** đặt sẵn các giá trị này (`_returnAccount = "5212"` v.v.) và `CellTemplate` (hiển thị) bind THẲNG vào các string này, không qua `SelectedXxxAccount` — nên dù `AddLine()` không gán gì, field vẫn có giá trị mặc định ngay từ lúc khởi tạo object.

**Báo cáo lần 2** (user gửi lại đúng ảnh cũ, xác nhận bug chưa hết) — root-cause đúng, fix bằng:
- `SalesReturnLineItem`: đổi field initializer `_returnAccount`/`_debtAccount`/`_discountAccount`/`_taxAccount`/`_costAccount`/`_cogsAccount` từ giá trị mặc định thật → `""`.
- `SalesReturnViewModel.AttachLineHandlers(line)` — helper mới, gắn vào `PropertyChanged` của mỗi dòng: khi `ProductId` chuyển từ `0` → có giá trị thật (tức user vừa chọn 1 sản phẩm) MỚI gán các mã TK mặc định thật (`5212`/`131`/`5211`/`33311`/`1561`/`632`) + kho mặc định — dòng còn trống (`ProductId == 0`) không bao giờ có giá trị TK hiển thị.

**Báo cáo lần 3** — sau fix trên, cột TK đã đúng nhưng Số lượng/Đơn giá/Thành tiền/Tỷ lệ CK/Tiền CK/... vẫn hiện "0"/"0.00" (đây là field kiểu số, khác field kiểu string ở trên — field initializer mặc định `0` là hành vi C# tự nhiên, không phải bug logic, nhưng WPF hiển thị "0" gây cảm giác "còn dữ liệu"). Fix: áp `BlankPreserveConverter` (converter **đã có sẵn trong codebase**, dùng y hệt ở `SalesOrderWindow.xaml` — không viết converter mới) cho toàn bộ cột số ở cả 4 tab.

**Báo cáo lần 4** — tab "Thuế", cột "Diễn giải thuế" hiện chữ `"Thuế GTGT -"` (thiếu tên sản phẩm) ngay cả ở dòng trống. Nguyên nhân: XAML dùng `Binding Path="ProductName" StringFormat="Thuế GTGT - {0}"` — `StringFormat` của WPF **luôn** in phần chữ tĩnh dù giá trị bind rỗng, không có cách tự ẩn phần tĩnh khi rỗng. Fix: thêm computed property `TaxDescription` trên `SalesReturnLineItem` (`=> string.IsNullOrEmpty(ProductName) ? "" : $"Thuế GTGT - {ProductName}"`, tự `OnPropertyChanged` khi `ProductName` đổi), đổi binding XAML từ `StringFormat` sang thẳng `{Binding TaxDescription}`.

### 2. "NV bán hàng" không tự liên kết khi chọn khách hàng

`OnSelectedCustomerChanged` thêm lookup `SaleCareEmployeeId` để tự set `SelectedEmployee` — mirror đúng hành vi đã có sẵn ở `SalesOrderViewModel` (Sales module), trước đó `SalesReturnViewModel` thiếu bước này.

## Known Gaps / Follow-ups

- `SalesReturnWindow` cột "Mã hàng"/"Tên hàng" vẫn dùng pattern `ComboBox` cũ + code-behind chắp vá (giống `SalesOrderWindow` **trước khi** migrate sang `AppSearchableComboBox` ở 2026-08-22) — candidate migrate tương tự nếu gặp lại bug "gõ tên không hiện list" (xem `Sales/docs/sales.md`).
- Chưa test thật trên UTM cho toàn bộ layout redesign + 2 workflow in — chỉ verify qua `dotnet build` 0 lỗi từ máy Mac (không chạy được UI WPF trên macOS).
- Không có BE change nào trong toàn bộ đợt sửa 2026-08-22 này — mọi thay đổi (layout, in ấn, fix bug dòng trống, auto-link NV bán hàng) đều thuần WPF-side, dùng nguyên contract API đã có sẵn từ trước.

---

*Created 2026-08-28: doc đầu tiên cho module SalesReturn phía WPF, tổng hợp lại toàn bộ layout redesign + 2 workflow in (Ghi sổ→In Hoá Đơn, Lập PN→In Phiếu Nhập Kho) + fix bug dòng trống/NV bán hàng thực hiện trong phiên 2026-08-22.*
