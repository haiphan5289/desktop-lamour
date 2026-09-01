# Chứng từ hàng bán bị trả lại — WPF Client Documentation

> Module: `Features/HomePage/SalesReturn`
> BE counterpart: `be-window-lamour/src/Lamour.Application/Features/SalesReturn/docs/sales-return.md`
> First documented: 2026-08-28 (doc mới — module trước đó chưa có `docs/` riêng phía WPF dù BE đã có)
> **Last updated: 2026-08-31** — Vietkey fix, gộp thật "Ghi sổ"→"Lập PN" (in Phiếu Nhập Kho), bỏ auto-fill Diễn giải, workflow Ghi sổ/Bỏ ghi thật (BE thêm Draft/Confirmed), redesign màn danh sách theo MISA, đổi mặc định bộ lọc ngày sang "Đầu tháng đến hiện tại" — xem mục "Update — 2026-08-31" bên dưới.

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

## Update — 2026-08-31: Vietkey fix, Ghi sổ/Lập PN merge thật, Diễn giải để trống

### 1. Fix lỗi gõ tiếng Việt (Vietkey) ở cột sản phẩm

Cột "Mã hàng"/"Tên hàng" trong `SalesReturnWindow.xaml` migrate từ `ComboBox IsEditable="True"` (PART_EditableTextBox tự reset buffer IME giữa chừng khi gõ dấu) sang `controls:AppSearchableComboBox` — **y hệt** pattern đã chốt ổn định ở `SalesOrderWindow` (xem `Sales/docs/sales.md`), không phải giải pháp mới. `SalesReturnWindow.xaml.cs` bỏ toàn bộ code-behind chắp vá cũ (`OnProductCellTextChanged`/`LinesDataGrid_PreparingCellForEdit`/`RestoreTypedTextDeferred`/`FindParent`/`LinesDataGrid_CellEditEnding`), thay bằng 1 handler duy nhất lắng nghe `AppSearchableComboBox.SelectionCommittedEvent` để `CommitEdit` cả dòng ngay khi chọn xong sản phẩm — dọn luôn mục Known Gaps cũ ("vẫn dùng pattern ComboBox cũ").

### 2. "Ghi sổ" giờ TRỰC TIẾP thực hiện "Lập PN" và in "Phiếu Nhập Kho" (đổi ý so với 2026-08-22)

**Quan trọng:** đảo ngược quyết định đã chốt ở bản doc trước — doc 2026-08-22 mô tả 2 luồng "Ghi sổ→In Hoá Đơn" (in `SalesReturnPrintWindow`, "PHIẾU TRẢ LẠI HÀNG BÁN") và "Lập PN→In Phiếu Nhập Kho" (in `WarehouseReceiptPrintWindow`) là **tách biệt, chủ đích**. User yêu cầu gộp lại thật (không chỉ restyle): bấm "Ghi sổ" giờ tự động tạo `WarehouseReceipt` thật (hoặc tái dùng nếu đã lập trước đó) và hiện thẳng "PHIẾU NHẬP KHO" — `SalesReturnPrintWindow`/"Phiếu trả lại hàng bán" không còn là kết quả của "Ghi sổ" nữa (nút "In" trên toolbar vẫn còn trỏ tới `SalesReturnPrintWindow` cũ — xem Known Gaps).

- `SalesReturnViewModel.SaveAsync` — sau khi Create/Update thành công (và Confirm — xem mục 4), gọi `EnsureWarehouseReceiptPrintedAsync(result.Id, result.DocumentNumber, ct)` thay vì `ShowPrintPreview`.
- **`EnsureWarehouseReceiptPrintedAsync`** (method mới, dùng chung bởi `SaveAsync` và `CreateWarehouseReceiptAsync`/"Lập PN") — gọi `IGetWarehouseReceiptsUseCase` trước, tự dedup theo `ReceiptType == 2 (ReturnedGoods) && Reference == DocumentNumber` (khớp đúng cách BE `CreateSalesReturnWarehouseReceiptUseCase` tự phát hiện "đã lập PN rồi" — không có FK thật giữa 2 bảng), chỉ gọi Create thật nếu chưa có; sau đó luôn mở `WarehouseReceiptPrintWindow` dù tạo mới hay tái dùng.
- **Bug phát sinh #1 (đã fix)**: merge 2 luồng khiến mỗi lần Update một chứng từ đã từng Lập PN sẽ thử tạo `WarehouseReceipt` trùng → BE ném `DomainException` ("Đã lập phiếu nhập kho cho chứng từ ... rồi."). Root cause: chưa dedup trước khi Create. Fix nằm trong `EnsureWarehouseReceiptPrintedAsync` ở trên.
- **Bug phát sinh #2 (đã fix, không liên quan tới merge)**: `WarehouseReceiptPrintWindow.BuildDocument` ném `ArgumentException: "Item belongs to another collection currently."` — lỗi FlowDocument tiềm ẩn từ trước (chưa từng chạy UI thật trên UTM), lộ ra vì đây là lần đầu 2 luồng thực sự chạm tới cửa sổ in này qua đường "Ghi sổ". Chi tiết fix (anti-pattern `CombineRow` tái dùng `TableCell` giữa 2 `TableRow`) + các fix layout MISA khác (canh giữa Ngày/Số/tiêu đề, padding Nợ/Có, để trống "Tổng số tiền" khi = 0) xem `Warehouse/docs/warehouse.md`.

### 3. "Diễn giải" không còn tự điền "Thu hồi hàng {Tên KH}"

`OnSelectedCustomerChanged` trước đây tự set `Description = $"Thu hồi hàng {c.Name}"` mỗi khi chọn khách hàng — bỏ dòng này, để trống cho user tự nhập. Giữ nguyên phần auto-link `SelectedEmployee` theo `SaleCareEmployeeId` (không liên quan).

### 4. Thêm workflow "Ghi sổ"/"Bỏ ghi" thật (Draft/Confirmed) — BE mới có Status

Trước đây `SalesReturn` **không có** khái niệm Status (`sales-return.md` phía BE ghi rõ "tạo xong là final", tồn kho cộng ngay lúc Create) — đã đổi thật theo yêu cầu, mirror đúng pattern `WarehouseReceiptStatus`/`PaymentStatus` đã có sẵn trong app. Xem BE doc để biết chi tiết endpoint/business rule; phần dưới đây chỉ nói phía WPF ăn khớp thế nào.

- **Popup `SalesReturnWindow`**: nút toolbar "Ghi sổ" (label cũ, đã có từ đợt unify toolbar) trước đây chỉ Save (Create/Update) — sau khi BE thêm Status, Save một mình chỉ tạo bản ghi ở `Draft`, không cộng tồn kho, khiến nút "Ghi sổ" nói dối cái tên của nó. Fix: `SaveAsync` sau khi Create/Update, nếu `result.Status == "Draft"` thì tự gọi thêm `IConfirmSalesReturnUseCase.ExecuteAsync(result.Id, ct)` — "Ghi sổ" giờ luôn là Save + Confirm trong 1 lần bấm, đúng nghĩa.
- **Sửa/Xóa trên popup** giờ chỉ khả dụng khi `CurrentReturn.Status == "Draft"` (`CanDeleteReturn` mới; `EditSalesReturnCommand`/`DeleteSalesReturnCommand` List-level cũng gate tương tự) — khớp guard mới ở BE (`UpdateSalesReturnUseCase`/`DeleteSalesReturnUseCase` ném 400 nếu đã `Confirmed`). Disable, không ẩn — theo đúng nguyên tắc chung của app.
- **`SalesReturnResponseDto.Status`**: `string` ("Draft"/"Confirmed") — cùng convention `PaymentResponseDto`/`WarehouseReceiptResponseDto`, không phải số nguyên.

### 5. Redesign màn danh sách "Chứng từ hàng bán bị trả lại" (`SalesReturnListView`/`SalesReturnListViewModel`) theo MISA

So ảnh mẫu MISA, màn danh sách thiếu toolbar đầy đủ, bộ lọc theo Kỳ/Trạng thái, filter theo cột, và vài cột dữ liệu. Đã thêm:

- **Toolbar**: Thêm/Xem/Sửa/Xóa/Ghi sổ/Bỏ ghi/Xuất khẩu — "Xem" mở cùng popup Sửa nhưng không đòi hỏi `Draft` (xem được cả chứng từ đã Ghi sổ, giống `AccountingViewModel.ViewEntry`); "Ghi sổ"/"Bỏ ghi" gọi thẳng `IConfirmSalesReturnUseCase`/`IUnconfirmSalesReturnUseCase` trên dòng đang chọn, không cần mở popup. Không thêm "Góp ý"/"Giúp" (không có logic thật để gắn vào).
- **Filter bar**: thêm dropdown "Kỳ" (`PeriodOptions`, mirror `AccountingViewModel`), "Trạng thái" (Tất cả/Nháp/Đã ghi sổ), "Kiêm phiếu nhập" (Tất cả/Có/Chưa).
- **Cột mới**: "Ngày hạch toán" (`AccountingDate` — đã có sẵn trên DTO, trước đó fetch về nhưng không map/hiện), "Diễn giải" (tương tự), "Trạng thái" (badge màu), "Kiêm phiếu nhập" (**tính client-side** — không có field này trên BE response, so khớp với `IGetWarehouseReceiptsUseCase` theo `ReceiptType == 2 && Reference == DocumentNumber`, y hệt logic dedup ở mục 2). Chủ động **bỏ qua** "Số hóa đơn" (field không tồn tại, không map rõ ràng vào field nào có sẵn) và "Tiền thuế GTGT" (chỉ có ở line-level, không có tổng hợp ở header) — quyết định qua `AskUserQuestion`.
- **Filter theo từng cột**: tái dùng `Shared/Models/ColumnFilterModels.cs` (đã dùng ở `SalesOrderReportDetailView`/`AccountingView`) — lọc client-side qua `ICollectionView` (`SalesReturnsView`), layer lên trên bộ lọc server-side sẵn có (Từ/Đến ngày + tìm kiếm, không đổi).
- **`SalesReturnListItem`**: thêm `Status`/`IsDraft`/`IsConfirmed`/`StatusLabel`/`HasLinkedWarehouseReceipt`(mutable, set sau `FromDto`)/`HasLinkedWarehouseReceiptLabel`, giữ field `Original` để mở lại popup.

### 6. Mặc định bộ lọc ngày đổi thành "Đầu tháng đến hiện tại" (áp dụng đồng bộ toàn app)

`FilterFromDate`/`FilterToDate` + `SelectedPeriod` đổi mặc định từ "Tùy chọn" (không lọc, hiện toàn bộ lịch sử) sang "Đầu tháng đến hiện tại" — cùng đợt đổi áp dụng cho `SalesOrderListViewModel`, `AccountingViewModel`, `BulkCustomerReceiptSearchViewModel`, `DepositDeductionReportViewModel`, `WarehouseTransactionListViewModel` (xem doc từng module để biết default cũ của từng cái). Lưu ý kỹ thuật: field initializer của `[ObservableProperty]` chạy TRƯỚC constructor nên không tự kích hoạt `OnSelectedPeriodChanged` — phải tự set `FilterFromDate`/`FilterToDate` khớp tay với `SelectedPeriod` ngay tại field initializer, không thể chỉ đổi 1 trong 2.

## Known Gaps / Follow-ups

- Nút "In" riêng trên toolbar `SalesReturnWindow` (không phải "Ghi sổ") vẫn trỏ tới `SalesReturnPrintWindow`/"Phiếu trả lại hàng bán" cũ — chưa xác nhận có cần đổi sang in lại "Phiếu Nhập Kho" đã liên kết hay không (được flag cho user, chưa yêu cầu/xác nhận sửa).
- Export Excel/Print (nếu có báo cáo tổng hợp dùng lại dữ liệu SalesReturn) chưa cập nhật để show cột Status/Kiêm phiếu nhập mới.
- Chưa test thật trên UTM cho đợt sửa 2026-08-31 này — chỉ verify qua `dotnet build` 0 lỗi từ máy Mac; user tự xác nhận từng bước qua screenshot trong quá trình làm (đã fix 2 bug runtime thật do đó: duplicate WarehouseReceipt, "Item belongs to another collection").
- Chưa test thật trên UTM cho toàn bộ layout redesign + 2 workflow in ngày 2026-08-22 (mục cũ, vẫn còn hiệu lực).

---

*Created 2026-08-28: doc đầu tiên cho module SalesReturn phía WPF, tổng hợp lại toàn bộ layout redesign + 2 workflow in (Ghi sổ→In Hoá Đơn, Lập PN→In Phiếu Nhập Kho) + fix bug dòng trống/NV bán hàng thực hiện trong phiên 2026-08-22.*
*Updated 2026-08-31: Vietkey fix, gộp thật "Ghi sổ"→"Lập PN"/in Phiếu Nhập Kho, bỏ auto-fill Diễn giải, thêm workflow Ghi sổ/Bỏ ghi thật (BE Draft/Confirmed), redesign màn danh sách theo MISA, đổi mặc định bộ lọc ngày sang "Đầu tháng đến hiện tại".*
