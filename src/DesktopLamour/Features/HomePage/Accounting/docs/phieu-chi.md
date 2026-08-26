# Phiếu Chi — Desktop App Documentation

> Feature: Phiếu Chi (Cash Payment)
> Module: Accounting (WPF)
> Created: 2026-04-29 (parallel to Phiếu Thu)
> **Major update: 2026-08-10** — Draft/Confirm lifecycle, TK Nợ/TK Có → Tài khoản kế toán (FK thật), Khoản mục CP, toolbar/tab UI khớp ảnh mẫu MISA, In phiếu.
> **2026-08-26** — "Đối tượng" mở rộng đa loại: Nhà cung cấp / Khách hàng / Nhân viên (trước đây chỉ Supplier).
> **2026-08-26 (tiếp, so ảnh mẫu MISA)** — bỏ combo "Loại đối tượng" thừa (không khớp ảnh mẫu), auto-copy Đối tượng xuống dòng hạch toán, thêm cột "Đối tượng" (mã) trong grid + đổi thứ tự cột, thêm nút "↩️ Hoàn", context menu Ctrl+Insert/Ctrl+Delete/Ctrl+F trên grid.

## "Đối tượng" đa loại (2026-08-26)

"Đối tượng" là **1 ô tìm kiếm chung** (`PartnerCombo`, `AppSearchableComboBox`) cho cả Nhà cung cấp/Khách hàng/Nhân viên — `PartnerItems` = `Suppliers.Concat(Customers).Concat(Employees)` (cả 3 đều implement `ISearchableItem` sẵn). **Không có** combo "chọn loại đối tượng" riêng — bản đầu có thêm combo này nhưng sau khi so ảnh mẫu MISA (chỉ 1 ô lookup duy nhất, gõ mã gì cũng tìm ra, không bắt chọn loại trước) đã bỏ đi cho khớp UX thật.

`SelectedPartner` (ISearchableItem?, đổi tên từ `SelectedSupplier`) — auto-populate `Address` khi chọn phân biệt theo type cụ thể (`Supplier`/`Customer` có field Address, `Employee` thì không, giữ nguyên `Address` hiện tại). `PartnerType` gửi lên BE (`ResolvePartnerType`, static helper trong `PaymentViewModel`) suy ra từ **kiểu runtime** của object đã chọn (`is Customer`/`is Employee`/mặc định `Supplier`) — không lưu type trên ViewModel, không cần user chọn.

Khi đổi "Đối tượng" ở header, `OnSelectedPartnerChanged` đồng bộ luôn `SubjectCode`/`SubjectName` xuống **mọi dòng hạch toán hiện có** trong `Entries`; `AddEntry()` cũng mặc định `SubjectCode`/`SubjectName` theo `SelectedPartner` cho dòng mới — khớp ảnh mẫu MISA (dòng grid có "Đối tượng"/"Tên đối tượng" trùng khớp header).

BE lưu polymorphic (`PartnerType` + `PartnerId` + `PartnerName` cache) — xem chi tiết thiết kế BE: `be-window-lamour/src/Lamour.Application/Features/Accounting/docs/phieu-chi.md` (mục "Đối tượng đa loại"). DTO đổi `supplier_id`/`supplier_name` → `partner_type`/`partner_id`/`partner_name`.

## "Hoàn" (Unconfirm) — mới (2026-08-26)

Nút toolbar **"↩️ Hoàn"** (`UnconfirmCommand`, `CanExecute = CanUnconfirm` — chỉ bật khi `CurrentPayment.Status == "Confirmed"`) — đưa phiếu đã Ghi số quay lại `Treo`, khớp nút "Hoàn" trong ảnh mẫu MISA (trước đây hoàn toàn không có, Confirmed là bất biến tuyệt đối). Gọi `IUnconfirmPaymentUseCase` → `PaymentService.UnconfirmAsync` → `POST {id}/unconfirm`. Sau khi Hoàn thành công: reload danh sách + điều hướng tới phiếu (giờ ở trạng thái Treo) — **không** đóng window (khác `ConfirmAsync`/`TreoAsync`, để user xem/sửa lại ngay).

## Context menu trên grid "Hạch toán" — mới (2026-08-26)

Gắn `DataGridLineContextMenuBehavior` (đã có sẵn, dùng chung với SalesOrder/SalesReturn/WarehouseReceipt) vào grid dòng hạch toán:

```xml
behaviors:DataGridLineContextMenuBehavior.EnableLineContextMenu="True"
behaviors:DataGridLineContextMenuBehavior.AddCommandName="AddEntryCommand"
behaviors:DataGridLineContextMenuBehavior.RemoveCommandName="RemoveEntryCommand"
behaviors:DataGridLineContextMenuBehavior.ShowProductStockMenuItem="False"
```

Behavior gốc code cứng `AddLineCommand`/`RemoveLineCommand` + khái niệm "sản phẩm/tồn kho" (Ctrl+F2) — không hợp với Payment (dùng tên command khác, dòng hạch toán không phải sản phẩm). Đã thêm 3 attached property mới (`AddCommandName`/`RemoveCommandName`/`ShowProductStockMenuItem`, default giữ nguyên hành vi cũ cho 3 form kia) để tái dùng thay vì viết lại từ đầu. Kết quả trên Phiếu Chi: **Thêm dòng (Ctrl+Insert) / Xóa dòng (Ctrl+Delete) / Sao chép dữ liệu cho các dòng dưới / Tìm kiếm (Ctrl+F)** — khớp phần lớn menu ảnh mẫu MISA, **trừ** "Định khoản"/"Xem số dư tài khoản..."/"Cắt mẫu" (không có logic/data backing, cố tình không làm placeholder giả — theo đúng nguyên tắc "ẩn action không có thật" đã áp dụng cho toolbar 2026-08-10).

**Không đổi:** `PaymentEntryItem.SubjectCode`/`SubjectName` (cột "Đối tượng"/"Tên đối tượng" trong grid dòng hạch toán) — free-text, tự copy từ header nhưng sửa tay được, khác hoàn toàn với field "Đối tượng" ở header.

## User Flow

1. User vào màn **Kho** → tile "💰 Khoản mục chi phí" (nếu cần quản lý danh mục) hoặc màn **Quỹ** (trước đây gọi "Kế toán", đã đổi tên 2026-08-10) → click **"Phiếu Chi"**
2. `PaymentWindow` mở như standalone window (không phải popup/ShowDialog)
3. User điền header info + thêm dòng hạch toán (chọn TK Nợ/TK Có/Khoản mục CP ngay trong lưới)
4. **"💾 Cất"** → lưu **Nháp** (chưa post sổ quỹ) — có thể sửa/xoá lại
5. **"📑 Ghi số"** → lưu (nếu cần) + xác nhận → tạo `CashTransaction` (post sổ quỹ) → phiếu **bất biến** từ đây, không sửa/xoá được nữa

## Trạng thái Nháp / Đã ghi số (mới 2026-08-10)

```
   Cất                Ghi số
Draft ────────► Draft ────────► Confirmed (bất biến — chỉ xem, In, không Sửa/Xoá)
```

- `PaymentViewModel.CanEdit` = `CurrentPayment is null || Status != "Confirmed"` — bind `IsEnabled` lên toàn bộ vùng "Thông tin chung", tab "Hạch toán", footer "+ Thêm dòng".
- **"Sửa"** (toolbar): nếu phiếu đã Confirmed → `MessageBox` "Phiếu chi đã ghi số, không thể sửa", không mở khoá gì cả (Draft thì luôn đã editable, không cần "mở khoá" riêng).
- **"Xóa"**/Update BE trả `DomainException` (400) nếu cố sửa/xoá phiếu đã Confirmed — WPF hiện `ErrorMessage`/`MessageBox` với đúng nội dung lỗi từ BE (đã sửa `PaymentService` dùng `EnsureSuccessOrThrowAsync` đọc `{"error": "..."}` thay vì `EnsureSuccessStatusCode()` nuốt message).

## Window Layout (2026-08-10)

```
Title: "Phiếu chi - CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR"
Size: 1100×760, WindowStartupLocation=CenterOwner

┌─ Toolbar (4 nhóm, chỉ giữ action có logic thật) ─────────────────────────────────┐
│ [◀Trước|Sau▶]  [➕Thêm|✏️Sửa|🗑️Xóa|📌Treo|📑Ghi số|↩️Hoàn]  [🔄Làm mới]  [🖨️In|✖️Đóng] │
└───────────────────────────────────────────────────────────────────────────────┘
  (Đã bỏ: Sửa nhanh / Tiện ích / Mẫu / Giúp — không có logic thật, ẩn hẳn
   thay vì hiện placeholder "đang phát triển". "↩️ Hoàn" mới 2026-08-26.)

┌─ Thông tin chung ─────────────────┐  ┌─ Chứng từ ──────┐
│ Đối tượng   [1 ô tìm chung NCC/KH/NV] │ │ Ngày hạch toán   │
│ Người nhận  [TextBox — auto]      │  │ Ngày chứng từ    │
│ Địa chỉ     [TextBox]             │  │ Số chứng từ      │
│ Lý do chi   [ComboBox] [TextBox — chi tiết, VD "thuê lái xe 21/7"] │
│ Nhân viên [ComboBox]+  Kèm theo [TextBox] "chứng từ gốc"           │
│ Tham chiếu  [TextBox] 🔍                                            │
└───────────────────────────────────┘  └──────────────────┘

┌─ Tab: [1. Hạch toán] [2. Thuế] ───────────────────────────────────┐
│ (style AppTabControl.Modern — giống popup "Thêm VTHH")            │
│ DataGrid: Diễn giải | TK Nợ | TK Có | Số tiền | Đối tượng |       │
│           Tên đối tượng | TK ngân hàng | Khoản mục CP             │
│  → TK Nợ/TK Có/Khoản mục CP là ComboBox LUÔN HIỆN SẴN             │
│    trong ô (không phải "click để sửa") — xem bug story            │
│  → Right-click / Ctrl+Insert/Ctrl+Delete/Ctrl+F — xem context menu│
└────────────────────────────────────────────────────────────────────┘

Footer: F9-Thêm nhanh, F3-Tìm nhanh, Ctrl+F-Tìm kiếm, Ctrl+Insert-Thêm dòng, Ctrl+Delete-Xóa dòng | Số dòng=N | Tổng tiền: {total}
```

## ⚠️ Bug story: ComboBox trong DataGrid (đọc trước khi sửa cột TK Nợ/TK Có/Khoản mục CP)

Cột TK Nợ/TK Có/Khoản mục CP đổi cách bind **4 lần** trước khi ổn định. Bug gốc: chọn giá trị trong dropdown xong, ô hiện **trống** cho tới khi làm gì đó khác (bấm Enter, hoặc không hiện luôn kể cả bấm Enter tuỳ giai đoạn sửa).

| # | Cách làm | Kết quả |
|---|---|---|
| 1 | `DataGridComboBoxColumn` + `SelectedValueBinding`/`SelectedValuePath="Id"`, `ItemsSource="{Binding AccountSettings}"` (không RelativeSource) | ❌ Dropdown **rỗng hoàn toàn** — property-level binding trên `DataGridColumn` không nhận DataContext của DataGrid trong app này (khác tài liệu MS) |
| 2 | `DataGridTemplateColumn` (Cell/CellEditingTemplate tách biệt) + `SelectedValue`/`SelectedValuePath="Id"` (kiểu `int?`) | ❌ `ItemsSource` load đúng, nhưng `SelectedValue` TwoWay **không đẩy được** giá trị về property `int?` khi list chỉ có 1 item — `SelectionChanged` bắn đúng, `PropertyChanged` trên entry object không bao giờ fire (xác nhận bằng debug log gắn trực tiếp vào ViewModel + behavior) |
| 3 | Đổi `SelectedValue`→`SelectedItem` (bind cả object), vẫn còn Cell/CellEditingTemplate tách biệt | ⚠️ Bấm **Enter** thì đúng, nhưng **click sang ô/cột khác thì mất** — DataGrid chỉ đẩy binding của `CellEditingTemplate` xuống nguồn khi có tín hiệu commit rõ ràng. Thử `grid.CommitEdit(DataGridEditingUnit.Cell, true)` (đồng bộ + `Dispatcher.BeginInvoke`) và thử giả lập `KeyEventArgs(Key.Enter){RoutedEvent=Keyboard.KeyDownEvent}` — **cả 2 đều không hoạt động** |
| 4 | **Fix cuối**: bỏ `CellEditingTemplate` hẳn, ComboBox nằm thẳng trong `CellTemplate`, luôn hiện sẵn, không có "chế độ sửa" riêng | ✅ Hoạt động ổn định — không còn khái niệm "commit" để mà mất |

```xml
<DataGridTemplateColumn Header="TK Nợ" Width="160">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <ComboBox ItemsSource="{Binding DataContext.AccountSettings, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                      DisplayMemberPath="DisplayText"
                      SelectedItem="{Binding SelectedDebitAccount, Mode=TwoWay}"
                      BorderThickness="0" Background="Transparent"/>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
    <!-- KHÔNG CellEditingTemplate -->
</DataGridTemplateColumn>
```

**Bài học chung**: với ComboBox trong `DataGridTemplateColumn` mà không thực sự cần phân biệt "xem" vs "sửa" (luôn cho sửa ngay), đừng dùng `CellEditingTemplate` — đặt thẳng control tương tác trong `CellTemplate` là cách rẻ và chắc ăn nhất, tránh toàn bộ lớp bug về commit-timing của DataGrid.

## Field Defaults (New Form)

| Field | Default |
|-------|---------|
| `Số chứng từ` | Auto-generated `PC{N:D5}` — finds max existing PC number + 1 |
| `Ngày hạch toán` / `Ngày chứng từ` | Today |
| `Lý do chi` | `ChiKhac` (dropdown); `ReasonDetail` (ô tự do) — trống |
| `TK Nợ` / `TK Có` (dòng mới) | Theo `ILastUsedPaymentAccountsStore` (lần chọn gần nhất trong session) — `null` nếu chưa từng chọn |
| `Khoản mục CP` (dòng mới) | `null` — không mặc định |

### Auto-Generated Document Number

`GenerateNextDocumentNumber()` trong `PaymentViewModel` — không đổi từ bản gốc: scan `_receiptListCache` tìm số "PC" lớn nhất + 1.

## Ghi nhớ TK Nợ/TK Có lần chọn gần nhất

`ILastUsedPaymentAccountsStore` / `LastUsedPaymentAccountsStore` (`Data/Storage/`, `AddSingleton`) — cập nhật mỗi khi user đổi `SelectedDebitAccount`/`SelectedCreditAccount` trên bất kỳ dòng nào; dòng mới ("+ Thêm dòng" / phím F9) tự điền theo giá trị vừa lưu.

**Chỉ lưu trong RAM** (session hiện tại của app) — mất khi tắt app. Repo này chưa có cơ chế lưu file settings nào cả (`InMemoryAuthTokenStorage` cũng không lưu JWT qua restart), nên đây là hành vi nhất quán với phần còn lại của app, không phải thiếu sót cần sửa thêm trừ khi có yêu cầu riêng.

## Dropdown Options

**Lý do chi (`PaymentReason`, dropdown cố định):**
```
ChiKhac      — Chi khác
ChiMuaHang   — Chi mua hàng
ChiTraNo     — Chi trả nợ
```
(`ChiLuong` có trong enum BE nhưng không đưa vào list dropdown WPF — giữ nguyên từ bản gốc.)

`ReasonDetail` — ô text tự do cạnh dropdown, cho lý do chi tiết (VD "thuê lái xe 21/7").

**TK Nợ / TK Có** — **không còn là list cố định**. Load từ `IGetAccountSettingsUseCase` (feature `AccountSettings`, 43 tài khoản gồm 39 gốc + 4 mới `111/112/131/334` seed riêng cho Payment — xem [`account-settings.md`](../../AccountSettings/docs/account-settings.md)).

**Khoản mục CP** — load từ `IGetExpenseCategoriesUseCase` (feature `Warehouses/ExpenseCategories`, nullable — có thể bỏ trống).

## F9 / F3 — phím tắt (mới, khớp gợi ý ảnh mẫu)

- **F9** — `KeyBinding` trên Window → `AddEntryCommand` (thêm dòng nhanh, tương đương nút "+ Thêm dòng")
- **F3** — xử lý ở code-behind (`PaymentWindow.xaml.cs`, `PreviewKeyDown`): focus vào ô "Đối tượng" (`AppSearchableComboBox`, đã có sẵn tìm-kiếm nội bộ) — không phải dialog tìm kiếm riêng, chỉ đưa focus tới ô có thể gõ tìm ngay

## In phiếu (mới)

`PaymentPrintWindow.xaml`/`.xaml.cs` — mirror `SalesOrderPrintWindow` (Sales feature): `FlowDocument` A5, `PrintDialog`, layout gồm logo công ty + tiêu đề "PHIẾU CHI" + bảng hạch toán (Diễn giải/TK Nợ/TK Có/Khoản mục CP/Số tiền) + 4 ô chữ ký (Người lập phiếu/Người nhận tiền/Thủ quỹ/Kế toán trưởng). Mở qua nút "🖨️ In" trên toolbar — chỉ enable khi `CurrentPayment != null` (`CanPrint`).

Không tự động in sau khi Ghi số — chỉ mở khi user bấm nút, tránh hành vi bất ngờ.

## Quỹ Tiền Mặt Auto-Refresh

Không đổi từ bản gốc — `PaymentSaved`/`RequestClose` events, xem code mẫu trong phiên bản trước của doc này (giữ nguyên hành vi, chỉ khác: `ConfirmAsync` cũng fire `PaymentSaved`/`RequestClose` sau khi Ghi số thành công, giống `SaveAsync`).

## Architecture

```
AccountingView.xaml           "Phiếu Chi" button → OpenPaymentCommand
        ↓
AccountingViewModel           Func<PaymentWindow> factory → window.Show()
        ↓
PaymentWindow.xaml            Standalone Window, DataContext = PaymentViewModel
        ↓
PaymentViewModel              CRUD + Confirm + Print, CanEdit gate theo Status
        ↓
ICreatePaymentUseCase / IUpdatePaymentUseCase / IDeletePaymentUseCase
IGetPaymentsUseCase / IGetPaymentByIdUseCase / IDuplicatePaymentUseCase
IConfirmPaymentUseCase / IUnconfirmPaymentUseCase   ← Unconfirm mới 2026-08-26
        ↓
PaymentService                HttpClient → http://192.168.64.1:5282
                               EnsureSuccessOrThrowAsync (đọc {"error":...})
```

## WPF Files Structure (thay đổi so với bản gốc)

```
Features/HomePage/Accounting/
  Data/Services/
    Dtos/PaymentEntryDto.cs           — + debit/credit_account_id/code/description, expense_category_id/name
    Dtos/{Create,Update}PaymentRequestDto.cs — + reason_detail
    Dtos/PaymentResponseDto.cs        — + reason_detail, status, confirmed_at
    IPaymentService.cs / PaymentService.cs — + ConfirmAsync/UnconfirmAsync, EnsureSuccessOrThrowAsync
  Data/Storage/
    ILastUsedPaymentAccountsStore.cs / LastUsedPaymentAccountsStore.cs  — mới
  Domain/Models/PaymentEntryItem.cs   — SelectedDebitAccount/SelectedCreditAccount (ISearchableItem?),
                                         SelectedExpenseCategory (ExpenseCategory?) — không còn Id/Name riêng
  Domain/UseCases/
    IConfirmPaymentUseCase.cs / ConfirmPaymentUseCase.cs  — mới
    IUnconfirmPaymentUseCase.cs / UnconfirmPaymentUseCase.cs  — mới (2026-08-26)
  ViewModels/PaymentViewModel.cs      — CanEdit, ConfirmCommand, UnconfirmCommand, EditCommand, PrintCommand,
                                         SelectedPartner/PartnerItems (Đối tượng đa loại), ResolvePartnerType
  Views/PaymentWindow.xaml(.cs)       — toolbar 4 nhóm (+ Hoàn), tab Hạch toán/Thuế, F9/F3,
                                         grid ComboBox always-visible + context menu (Ctrl+Insert/Delete/F)
  Views/PaymentPrintWindow.xaml(.cs)  — mới
  docs/phieu-chi.md                   — file này

Shared/Behaviors/DataGridLineContextMenuBehavior.cs — + AddCommandName/RemoveCommandName/
                                                        ShowProductStockMenuItem (2026-08-26, để Payment
                                                        tái dùng — xem mục "Context menu" ở trên)
```

## Backend API Endpoints

```
GET    /api/v1/accounting/payments
GET    /api/v1/accounting/payments/{id}
POST   /api/v1/accounting/payments
PUT    /api/v1/accounting/payments/{id}
DELETE /api/v1/accounting/payments/{id}
POST   /api/v1/accounting/payments/{id}/duplicate
POST   /api/v1/accounting/payments/{id}/confirm
POST   /api/v1/accounting/payments/{id}/unconfirm    ← mới (2026-08-26)
```

Chi tiết business rule/migration đầy đủ xem doc BE: `be-window-lamour/src/Lamour.Application/Features/Accounting/docs/phieu-chi.md`.

## Key Differences from Phiếu Thu

Phiếu Thu (`ReceiptWindow`) **chưa** được áp dụng các thay đổi 2026-08-10 (Draft/Confirm, AccountSetting FK, Khoản mục CP, toolbar mới) — vẫn dùng `AccountCode` enum cứng và CRUD đơn giản như Phiếu Chi bản gốc. Nếu cần đồng bộ, phải làm lại toàn bộ các bước ở trên cho `ReceiptWindow`/`ReceiptViewModel`.

## Known Limitations / Future Work

- Tab "2. Thuế" chỉ là placeholder rỗng.
- Cột "Mục thu/chi", "Đối tượng THCP", "Công trình", "Đơn vị", "Đơn đặt hàng", "Đơn mua hàng", "Hợp đồng mua", "Hợp đồng bán" (thấy trong ảnh mẫu MISA) — chưa làm, chưa có data model, đã quyết định bỏ qua.
- Context menu grid "Hạch toán" (2026-08-26) thiếu "Định khoản"/"Xem số dư tài khoản..."/"Cắt mẫu" so với ảnh mẫu — không có logic/data backing (VD "số dư tài khoản" cần tra cứu số dư luỹ kế theo tài khoản, chưa có UseCase nào), cố tình bỏ qua thay vì làm placeholder giả.
- Chưa migrate Phiếu Thu sang cùng pattern Draft/Confirm + AccountSetting FK + "Đối tượng" đa loại + Hoàn.
- `ILastUsedPaymentAccountsStore` chỉ lưu RAM, mất khi tắt app.
- Chưa có unit test cho lifecycle Draft/Confirm/Unconfirm mới.

## Testing Checklist

- [x] `payments`/`payment_entries` có `status`, `confirmed_at`, `reason_detail`, FK `AccountSetting`/`ExpenseCategory` sau migration
- [x] TK Nợ/TK Có/Khoản mục CP hiện đúng ngay sau khi chọn, không cần Enter hay click ra ngoài
- [ ] Cất phiếu mới → verify Status=Draft, chưa có `CashTransaction` trong Quỹ
- [ ] Ghi số → verify `CashTransaction` xuất hiện trong Quỹ, phiếu không sửa/xoá được nữa
- [ ] Sửa phiếu đã Ghi số → verify bị chặn với thông báo rõ ràng
- [ ] F9 thêm dòng nhanh, F3 focus vào Đối tượng
- [ ] In phiếu → verify layout A5 đúng, có đủ Khoản mục CP trong bảng in
- [ ] Chọn "Đối tượng" là Nhân viên/Khách hàng (không chỉ Supplier) → lưu → tải lại đúng
- [ ] Đổi "Đối tượng" ở header → verify SubjectCode/SubjectName tự đồng bộ xuống các dòng hạch toán hiện có
- [ ] Ghi số xong bấm "↩️ Hoàn" → verify về Treo, CashTransaction biến mất khỏi Quỹ, sửa lại được
- [ ] Ctrl+Insert/Ctrl+Delete/Ctrl+F trên grid Hạch toán hoạt động đúng (không có Ctrl+F2)
