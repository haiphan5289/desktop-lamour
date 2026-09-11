# Chứng từ hàng bán bị trả lại — WPF Client Documentation

> Module: `Features/HomePage/SalesReturn`
> BE counterpart: `be-window-lamour/src/Lamour.Application/Features/SalesReturn/docs/sales-return.md`
> First documented: 2026-08-28 (doc mới — module trước đó chưa có `docs/` riêng phía WPF dù BE đã có)
> **Last updated: 2026-09-11 (cùng ngày, mục mới nhất) — thêm nút "Ghi sổ" trực tiếp trên danh sách**
> (toolbar + context menu + Ctrl+G), mirror nút "Bỏ ghi" đã có — xem "Update — 2026-09-11: thêm nút
> Ghi sổ trên danh sách" ngay dưới. Trước đó cùng ngày — gộp "Nháp" và "Treo" thành 1 trạng thái duy
> nhất "Treo"** — bỏ `IsDraft`, `StatusOptions` chỉ còn 3 lựa chọn (Tất cả/Đã ghi sổ/Treo), RowStyle
> gộp còn 1 trigger — xem "Update — 2026-09-11: gộp Nháp + Treo thành 1". Trước đó cùng
> ngày — **ĐẢO NGƯỢC quyết định 2026-09-10**: "Cất" = "Ghi sổ" ngay (1 lần bấm, không còn qua Treo
> trung gian/`IsArmed`) — xem "Update — 2026-09-11: Cất = Ghi sổ ngay", đè lên mô tả "2 lần bấm" ở
> các mục cũ hơn. Trước đó cùng ngày (thứ tự xảy ra, mới nhất trước): thêm
> context menu chuột phải + phím tắt trên danh sách; bỏ dialog xác nhận khi bấm "Bỏ ghi" trên danh
> sách; danh sách tô màu (TextBrand/SemiBold) cả dòng "Treo" (`IsHeld`), trước đây chỉ "Nháp"
> (`IsDraft`) được tô; bộ lọc "Trạng thái" thêm lựa chọn "Treo" — xem các mục "Update — 2026-09-11..."
> tương ứng ngay dưới. Trước đó 2026-09-10 — tách "Cất" và "Ghi sổ" thành 2 hành động riêng, tái kích
> hoạt trạng thái Treo (**mục này nay đã lỗi thời**) — xem "Update — 2026-09-10" ngay dưới. Trước đó
> 2026-09-09 — tách rời "Bỏ ghi" (chỉ đảo trạng thái) khỏi "Sửa" (mới thật sự mở
> khóa dữ liệu) — xem "Update — 2026-09-09 (đổi ý lần 2)" ngay dưới. Trước đó cùng ngày: thêm nút
> "Bỏ ghi" thật (đảo Confirmed→Draft + hoàn tác tồn kho, đảo ngược 1 phần quyết định 2026-09-07),
> cột/filter "Trạng thái" thêm lại ở danh sách (rồi đổi cột thành highlight cả dòng). Nút "In"
> chuyển sang in "PHIẾU NHẬP KHO" (tái dùng `WarehouseReceiptPrintWindow`, `SalesReturnPrintWindow`
> đã xóa hẳn); "Ghi sổ" giữ popup mở thay vì tự đóng. Trước đó: 2026-09-08 — tách "Ghi sổ" khỏi in &
> lập PN (workflow từng bước giống MISA). 2026-09-07 — bỏ hẳn vòng đời Nháp → Ghi sổ. 2026-08-31 —
> Vietkey fix, gộp thật "Ghi sổ"→"Lập PN" (in Phiếu Nhập Kho), bỏ auto-fill Diễn giải, workflow Ghi
> sổ/Bỏ ghi thật (BE thêm Draft/Confirmed), redesign màn danh sách theo MISA, đổi mặc định bộ lọc
> ngày sang "Đầu tháng đến hiện tại".

## Update — 2026-09-11: thêm nút "Ghi sổ" trên danh sách

Theo yêu cầu (screenshot toolbar danh sách thật, khoanh đỏ các nút — báo thiếu "Ghi sổ" dù "Bỏ ghi"
đã có), xác nhận phạm vi qua `AskUserQuestion`: thêm action "Ghi sổ" trực tiếp từ danh sách, mirror
y hệt "Bỏ ghi" đã có (bấm thẳng, không hỏi xác nhận, dùng chung `IConfirmSalesReturnUseCase` với
popup). Áp dụng đồng bộ cho cả SalesOrder cùng lúc.

| File | Thay đổi |
|---|---|
| `ViewModels/SalesReturnListViewModel.cs` | Inject `IConfirmSalesReturnUseCase`; thêm `CanConfirmSelected => SelectedReturn is { IsHeld: true }`; thêm `ConfirmSalesReturnAsync` command (mirror `UnconfirmSalesReturnAsync`) |
| `Views/SalesReturnListView.xaml` | Nút toolbar "📗 Ghi sổ" (trước "↩️ Bỏ ghi"); thêm vào context menu + `KeyBinding Ctrl+G` |

Verify: `dotnet build -p:EnableWindowsTargeting=true` 0 lỗi. **Chưa test thật trên UTM.**

**Xác nhận sau khi hỏi lại (cùng ngày)**: user hỏi "có check hợp lệ trước khi enable Ghi sổ không" —
ban đầu hiểu nhầm thành "kiểm tra đủ tồn kho thật trước khi bật nút" (sẽ cần thêm 1 API mới + refactor
`ConfirmSalesOrderUseCase` để tách logic check ra dùng chung — xem investigation đã làm), nhưng sau
khi hỏi lại thì ý user chỉ là **enable/disable theo đúng trạng thái vòng đời** (Treo → bật, Đã ghi sổ
→ tắt) — đúng như `CanConfirmSelected` đang làm sẵn, **không cần sửa gì thêm**. Ghi lại rõ ở đây để
không đề xuất lại tính năng "pre-check tồn kho" này lần nữa trừ khi user chủ động yêu cầu lại.

## Update — 2026-09-11: gộp "Nháp" và "Treo" thành 1 trạng thái duy nhất "Treo"

Theo yêu cầu ("gộp status nháp & treo thành 1"). Bối cảnh đầy đủ + quyết định (tên gọi "Treo", migrate
DB, áp dụng đồng bộ SalesOrder) ở `be-window-lamour/.../SalesReturn/docs/sales-return.md` mục cùng
tên. Tóm tắt phía WPF (`SalesReturnListItem.cs`, `SalesReturnListViewModel.cs`,
`SalesReturnListView.xaml`, `SalesReturnViewModel.cs`):

| Trước | Sau (2026-09-11, hôm nay) |
|---|---|
| `IsDraft`/`IsHeld` 2 property riêng biệt | **Bỏ hẳn `IsDraft`** — chỉ còn `IsHeld => Status == "Held" \|\| Status == "Draft"` (bao trọn cả 2 raw status, phòng dữ liệu cũ chưa migrate) |
| `StatusLabel`: `"Held" → "⏸ Treo"`, `"Confirmed" → "📄 Đã ghi sổ"`, `_ → "↩️ Nháp"` | `StatusLabel`: `"Confirmed" → "📄 Đã ghi sổ"`, `_ → "⏸ Treo"` — không còn nhánh "Nháp" |
| `StatusOptions = {"Tất cả", "Đã ghi sổ", "Treo", "Nháp"}` | `StatusOptions = {"Tất cả", "Đã ghi sổ", "Treo"}` |
| `FilterItem` có 2 dòng check riêng cho "Treo"/"Nháp" | Chỉ còn 1 dòng: `FilterStatus == "Treo" && !item.IsHeld` |
| `SalesReturnListView.xaml` RowStyle có 2 `DataTrigger` (IsDraft, IsHeld) cùng 1 màu | Gộp còn **1 `DataTrigger`** duy nhất trên `IsHeld` |
| `SalesReturnViewModel.IsHeld => Status == "Held"` (popup) | `IsHeld => Status is "Held" or "Draft"` |

Verify: `dotnet build -p:EnableWindowsTargeting=true` 0 lỗi. **Chưa test thật trên UTM.**

## Update — 2026-09-11: "Cất" = "Ghi sổ" ngay (ĐẢO NGƯỢC quyết định 2026-09-10)

**Lần đổi ý thứ 3 cho khúc logic này.** Xem đầy đủ bối cảnh (video quay MISA, tách frame xác nhận
hành vi trước khi sửa, chi tiết thay đổi BE) ở
`be-window-lamour/.../SalesReturn/docs/sales-return.md` mục cùng tên. Tóm tắt phía WPF
(`SalesReturnViewModel.cs`):

| Trước (2026-09-10) | Sau (2026-09-11, hôm nay) |
|---|---|
| Cờ `IsArmed` (WPF-only) — bấm toggle lần 1 chỉ đổi nhãn "Bỏ ghi"→"Ghi sổ" + mở khóa Xóa, KHÔNG gọi BE; lần 2 mới thật sự Confirm | **Xóa hẳn `IsArmed`** — `ToggleConfirmAsync` bấm 1 lần gọi Confirm/Unconfirm thật ngay |
| `UnpostButtonLabel => (!IsConfirmed && IsArmed) ? "Ghi sổ" : "Bỏ ghi"` | `UnpostButtonLabel => IsConfirmed ? "Bỏ ghi" : "Ghi sổ"` |
| `CanDeleteReturn => ... && IsReadOnly && IsArmed` | `CanDeleteReturn => CurrentReturn is not null && !IsConfirmed && IsReadOnly` (bỏ phần `IsArmed`, giữ nguyên `IsReadOnly` — Xóa vẫn tắt khi đang Sửa, không đổi ý định gốc) |
| `SaveAsync`/`InitializeAsync` reset `IsArmed=false`/set theo `Status=="Draft"` | Bỏ hẳn các dòng liên quan `IsArmed` — chỉ còn khóa form (`IsReadOnly=true`) |

Vì BE (`Create/UpdateSalesReturnUseCase`) giờ luôn trả về `Confirmed` ngay sau khi Cất (đã cộng tồn
kho), form khóa lại và nút toggle hiện sẵn "Bỏ ghi" dùng được ngay — không cần bấm gì thêm.

Không đổi XAML (`SalesReturnWindow.xaml` không đổi binding nào — `UnpostCommand`/`UnpostLabel` vẫn
trỏ đúng `ToggleConfirmCommand`/`UnpostButtonLabel` như cũ). Verify:
`dotnet build -p:EnableWindowsTargeting=true` 0 lỗi. **Chưa test thật trên UTM.**

## Update — 2026-09-11: thêm context menu chuột phải + phím tắt trên danh sách

Theo yêu cầu (screenshot menu chuột phải MISA tham chiếu, phạm vi xác nhận qua nhiều vòng
`AskUserQuestion`): thêm context menu chuột phải cho `ReturnsGrid` — **mirror đúng quyết định đã có
sẵn ở `SalesOrderListView`** (chỉ đưa vào menu những action có nghiệp vụ thật, không rập khuôn toàn
bộ menu MISA — bỏ "Ghi sổ" vì chưa có action riêng ở mức danh sách, bỏ "Lập phiếu nhập.../Sửa
mẫu..." vì không có nghiệp vụ đó).

| File | Thay đổi |
|---|---|
| `Views/SalesReturnListView.xaml` (`DataGrid.ContextMenu`, mới hoàn toàn) | 5 mục: ➕ Thêm (Ctrl+N) · 👁 Xem (Ctrl+E) · ✏️ Sửa (F2) · 🗑️ Xóa (Ctrl+D) · ↩️ Bỏ ghi (Ctrl+B) — đều bind vào command đã có sẵn (không code mới ở tầng ViewModel) |
| `Views/SalesReturnListView.xaml` (`UserControl.InputBindings`, mới) | `KeyBinding` thật cho cả 5 phím tắt trên — bấm phím chạy thẳng command, không chỉ hiển thị chữ |
| `Views/SalesReturnListView.xaml` (`DataGrid`) | Thêm `PreviewMouseRightButtonDown="ReturnsGrid_PreviewMouseRightButtonDown"` |
| `Views/SalesReturnListView.xaml.cs` | Thêm handler `ReturnsGrid_PreviewMouseRightButtonDown` + `FindAncestor<T>` — mirror y hệt `SalesOrderListView.xaml.cs` (dò `DataGridRow` dưới điểm bấm chuột phải, tự `SelectedItem` trước khi menu mở, tránh thao tác nhầm lên dòng đang chọn cũ) |

**Quyết định khác với ảnh mẫu, đã xác nhận qua hỏi đáp:**
- Không có "Ghi sổ" — chưa có action Confirm riêng ở mức danh sách (chỉ có trong popup qua toggle).
- Không có "Nhân bản"/"Gửi email, Zalo" — SalesReturn không có command tương ứng (khác SalesOrder).
- "Sửa" dùng phím **F2** (không có trong ảnh mẫu) vì Ctrl+E đã dành cho "Xem" — 2 action tồn tại
  song song trong app này (khác ảnh mẫu MISA chỉ có 1 "Xem").
- Icon emoji thêm mới cho menu (ảnh mẫu trước đây ở `SalesOrderListView` không có icon) — dùng đúng
  emoji đã có sẵn trên toolbar (➕✏️🗑️↩️👁) để nhất quán, không bịa icon mới.

Không đổi BE, không đổi ViewModel (chỉ dùng lại command sẵn có). Verify:
`dotnet build -p:EnableWindowsTargeting=true` 0 lỗi (cross-compile trên Mac). **Chưa test thật trên
UTM** — đặc biệt cần xác nhận KeyBinding không xung đột với control khác trên màn khi dùng thật.

## Update — 2026-09-11: bỏ dialog xác nhận khi bấm "Bỏ ghi" trên danh sách

Theo yêu cầu (screenshot dialog `MessageBox` "Xác nhận bỏ ghi" từ máy thật, xác nhận phạm vi qua
`AskUserQuestion`): nút "Bỏ ghi" trên toolbar danh sách bấm vào thẳng, không còn hỏi lại Yes/No.

| File | Thay đổi |
|---|---|
| `ViewModels/SalesReturnListViewModel.cs` (`UnconfirmSalesReturnAsync`) | Xóa khối `MessageBox.Show("Xác nhận bỏ ghi", ...)` + check `confirm != MessageBoxResult.Yes` — gọi thẳng `_unconfirmReturn.ExecuteAsync` |

**Chỉ áp dụng cho nút trên danh sách** — dialog xác nhận tương tự trong popup chi tiết
(`SalesReturnViewModel.ToggleConfirmAsync`, khi đang `Confirmed` bấm "Bỏ ghi") **KHÔNG đổi**, vẫn còn
cảnh báo Yes/No trước khi trừ lại tồn kho. Lúc đầu **không** đồng bộ sang SalesOrder theo yêu cầu ban
đầu — nhưng ngay sau đó đã được clone sang `SalesOrderListViewModel.UnconfirmSalesOrderAsync` (xem
`Sales/docs/sales.md` mục "Update — 2026-09-11: bỏ dialog xác nhận..." cùng ngày), nên hiện tại cả 2
module đã đồng bộ như các lần sửa trước.

Không đổi BE. Verify: `dotnet build -p:EnableWindowsTargeting=true` 0 lỗi (cross-compile trên Mac).
**Chưa test thật trên UTM.**

## Update — 2026-09-11: tô màu dòng "Treo" trên danh sách + thêm filter "Treo"

Theo phản hồi (screenshot MISA tham chiếu, xác nhận qua `AskUserQuestion`): danh sách trước đây
không phân biệt màu giữa "Treo" và "Đã ghi sổ" (chỉ "Nháp" được tô `AppColor.TextBrand`/SemiBold),
và bộ lọc "Trạng thái" không có lựa chọn "Treo".

| File | Thay đổi |
|---|---|
| `Views/SalesReturnListView.xaml` (`DataGrid.RowStyle`) | Thêm `DataTrigger Binding="{Binding IsHeld}" Value="True"` — cùng màu/độ đậm với trigger `IsDraft` đã có (khớp pattern `SalesOrderListView.xaml` đã dùng cho "⏸ Treo") |
| `ViewModels/SalesReturnListViewModel.cs` (`StatusOptions`) | `{"Tất cả", "Đã ghi sổ", "Nháp"}` → **`{"Tất cả", "Đã ghi sổ", "Treo", "Nháp"}`** |
| `ViewModels/SalesReturnListViewModel.cs` (`FilterItem`) | "Nháp" đổi từ `!item.IsConfirmed` (ngầm hiện cả Held+Draft) → `!item.IsDraft` (chỉ Draft thật); thêm dòng lọc riêng cho "Treo" → `!item.IsHeld` |

Không đổi BE — `SalesReturnStatus.Held` đã tồn tại sẵn từ 2026-09-10, `SalesReturnListItem.IsHeld`
cũng có sẵn, chỉ chưa được dùng ở UI danh sách. Verify: `dotnet build -p:EnableWindowsTargeting=true`
0 lỗi (cross-compile trên Mac). **Chưa test thật trên UTM.**

## Review Artifact — workflow cho kế toán

Trang review 2 màn hình (popup + danh sách) + luồng Cất/Ghi sổ/Bỏ ghi cho kế toán xác nhận trước
khi vận hành — xem chi tiết + link tại
`be-window-lamour/.../SalesReturn/docs/sales-return.md` mục "Review Artifact — workflow cho kế
toán". Kế toán để lại góp ý bằng comment trực tiếp trên trang.

**Export tĩnh**: artifact yêu cầu tài khoản Claude + bật chia sẻ thủ công qua Share
trên claude.ai — không public ra URL khác được. Để gửi kế toán qua Zalo/email không cần tài khoản,
có 1 bản HTML tĩnh (nội dung giống hệt, bỏ checkbox tick "đã xem qua" vì chỉ lưu
`localStorage` theo trình duyệt, đổi thành danh sách đánh số) tại
`be-window-lamour/src/Lamour.Application/Features/SalesReturn/docs/ChungTuTraHangBan-Review.html`
(chuyển vào repo BE 2026-09-11, trước đó ở `~/Desktop/` — giờ version-controlled cùng doc BE)
— không tự đồng bộ khi artifact gốc đổi, cần export lại thủ công (ghi đè file này) nếu cập nhật thêm.

**Đối chiếu lại lần 2 (2026-09-11, cùng ngày)**: sau khi bỏ nút "Lập PN" (mục "Update — 2026-09-11"
ngay trên) và fix màu Treo/filter ở danh sách, đã đọc lại code lần nữa và cập nhật cả artifact lẫn
bản export tĩnh cho khớp — mockup popup bỏ nút "🧾 Lập PN", Bước 7 đổi thành "In — tự tạo Phiếu Nhập
Kho khi cần" (verify: `PrintAsync` trong `SalesReturnViewModel.cs` vẫn gọi
`_createWarehouseReceipt.ExecuteAsync` nếu chưa có Phiếu Nhập Kho liên kết — hành vi auto-create khi
in không đổi, chỉ mất nút bấm riêng), mockup danh sách thêm ví dụ dòng "Treo" tô cùng màu "Nháp",
điểm xác nhận #3 trong 6 điểm đánh dấu ✅ Đã fix thay vì để nguyên câu hỏi lỗi thời.

## Update — 2026-09-10: tách "Cất" và "Ghi sổ", tái kích hoạt trạng thái Treo

Theo yêu cầu (áp dụng đồng bộ cho cả SalesOrder — xem `Sales/docs/sales.md` mục cùng ngày): **"Cất"
không còn tự Ghi sổ**. BE (`be-window-lamour`) thêm lại `SalesReturnStatus.Held` ("Treo"),
`CreateSalesReturnUseCase`/`UpdateSalesReturnUseCase` giờ luôn kết thúc ở `Held`, không cộng tồn kho;
thêm `POST /{id}/confirm` ("Ghi sổ" thật — cộng tồn kho) tách khỏi Create/Update. Xem
`be-window-lamour/.../SalesReturn/docs/sales-return.md` mục "Update — 2026-09-10" cho chi tiết BE
(đã test thật qua curl + psql: Create→Held(kho không đổi)→Confirm(kho đổi đúng chiều)→Unconfirm
(hoàn tác) hoạt động đúng).

Phía WPF:

- **`DocumentToolbar`** — thêm DP `UnpostLabel` (mirror `SaveLabel` có sẵn), bind
  `Text="{Binding UnpostLabel, ElementName=Self}"` cho nút "Bỏ ghi" thay vì text tĩnh.
- **`SalesReturnViewModel`** — thêm `IsHeld` (`CurrentReturn?.Status == "Held"`),
  `IConfirmSalesReturnUseCase _confirmReturn`. `UnconfirmCommand` đổi tên/gộp thành
  `ToggleConfirmCommand` (`CanExecute = IsHeld || IsConfirmed`) — 1 nút duy nhất trên toolbar: Held
  → hiện "Ghi sổ" (gọi Confirm, cộng tồn kho), Confirmed → hiện "Bỏ ghi" (gọi Unconfirm, logic hoàn
  tồn kho không đổi). `UnpostButtonLabel` computed property cấp label động. `IsEditable`/`CanEdit`
  không cần đổi công thức (`!IsConfirmed` đã tự đúng với Held).
- **`SalesReturnWindow.xaml`** — `UnpostCommand="{Binding ToggleConfirmCommand}"` +
  `UnpostLabel="{Binding UnpostButtonLabel}"`.
- **`SalesReturnListItem`** — thêm `IsHeld`, `StatusLabel` đổi thành switch 3 nhánh
  (`Held → "⏸ Treo"`, `Confirmed → "📄 Đã ghi sổ"`, còn lại `"↩️ Nháp"`) — mirror
  `SalesOrderListItem`. Không thêm filter riêng cho Treo (SalesOrder cũng không có).
- **Client-side use case chain** (`Data/Services/ISalesReturnService.cs` + impl,
  `Data/Repositories/ISalesReturnRepository.cs` + impl, `Domain/UseCases/`) — thêm `ConfirmAsync`
  (`POST /{id}/confirm`) xuyên suốt 3 lớp, `IConfirmSalesReturnUseCase`/`ConfirmSalesReturnUseCase.cs`
  mới. DI trong `HomeServiceCollectionExtensions.cs` thêm dòng tương ứng cạnh Unconfirm.

**Verify**: `dotnet build src/DesktopLamour/DesktopLamour.csproj -p:EnableWindowsTargeting=true` —
0 lỗi, 0 warning (build cross-compile được trên Mac dù target `net8.0-windows`). **Chưa test qua
UTM thật** — cần verify: mở chứng từ mới → Cất → status Treo, tồn kho không đổi → bấm "Ghi sổ" → status
Confirmed, tồn kho cộng đúng → bấm lại (giờ hiện "Bỏ ghi") → status Draft, tồn kho hoàn tác.

### ⚠️ State machine thật đã chốt lại KHÁC bản implement trên (2026-09-10, cùng ngày)

Test tay trên UTM lộ ra toggle 2 chiều đơn giản ở trên **sai** — hành vi đúng phức tạp hơn, đã chốt
qua 1 Artifact mô phỏng tương tác (user bấm thử trực tiếp, không đoán bằng lời), xem đầy đủ bảng +
lý do ở `be-window-lamour/.../SalesReturn/docs/sales-return.md` mục "Toolbar toggle — state machine
đã chốt qua mô phỏng tương tác":

**https://claude.ai/code/artifact/687a0075-a307-4379-aea5-d0dd61b17a06**

**✅ Đã sửa code (cùng ngày)** — tóm tắt state machine đã implement:
- **Đi lên (Treo → Confirmed) cần đúng 2 lần bấm toggle**: lần 1 (label đang "Bỏ ghi") CHỈ đổi label
  thành "Ghi sổ" + mở khóa thêm nút Xóa, KHÔNG đụng tồn kho, KHÔNG gọi BE; lần 2 (label đang "Ghi sổ")
  mới thật sự gọi `ConfirmAsync` (cộng tồn kho) → Confirmed.
- **Đi xuống (Confirmed → Treo) chỉ 1 lần bấm** ("Bỏ ghi") → gọi `UnconfirmAsync` ngay (trừ tồn kho),
  và hạ cánh thẳng ở trạng thái "Treo đã đụng toggle" (label sẵn "Ghi sổ", Xóa đã mở) — KHÔNG phải
  trạng thái Treo gốc (label "Bỏ ghi", Xóa khóa) như vừa Cất lần đầu.
- Cần thêm 1 cờ **WPF-only** (không map từ `CurrentReturn.Status`) đánh dấu "đã đụng toggle lần 1
  chưa" — mirror đúng cách `IsReadOnly` đã làm cho luồng Bỏ ghi/Sửa ở mục 2026-09-09 bên dưới, vì BE
  chỉ có 3 status thật (Draft/Confirmed/Held), không đủ diễn tả trạng thái UI thứ 4 này.
- **Đã đồng bộ sang `SalesOrderViewModel` cùng ngày** — mirror y hệt (Normal thay cho Confirmed) —
  xem `Sales/docs/sales.md` mục cùng ngày.

**Chưa test qua UTM thật** — chỉ verify `dotnet build -p:EnableWindowsTargeting=true` (cross-compile
trên Mac) sạch 0 lỗi/warning.

---

## Update — 2026-09-09 (đổi ý lần 2): "Bỏ ghi" chỉ đảo trạng thái — không tự mở khóa form

Theo yêu cầu: tách rời 2 hành động "Bỏ ghi" (chỉ đảo Confirmed→Draft + tồn kho ở BE) và "Sửa" (thật
sự mở khóa dữ liệu để chỉnh sửa) — trước đó (mục "Bỏ ghi thật" ngay dưới) `IsEditable` tính thẳng
từ `CurrentReturn.Status` nên bấm "Bỏ ghi" xong là form mở khóa NGAY, không qua bước "Sửa" nào.

- **`SalesReturnViewModel.cs`**: thêm `[ObservableProperty] _isReadOnly` (cờ WPF-only, KHÔNG map từ
  BE `Status` — cần vì UI có 3 trạng thái thật: Confirmed/khóa, Draft-vừa-bỏ-ghi/khóa,
  Draft-đang-sửa/mở, trong khi `Status` chỉ có 2 giá trị). `IsEditable` đổi thành
  `CurrentReturn is null || (!IsConfirmed && !IsReadOnly)`. Thêm `CanEdit`/`EditCommand`/`Edit()`
  (mirror đúng `SalesOrderViewModel.CanEdit`/`Edit()` đã có sẵn — `IsReadOnly = true` → bấm "Sửa" →
  `false` + `BeginDirtyTracking()`).
  - `UnconfirmAsync`: sau khi BE trả `Status=Draft`, set `IsReadOnly = true` (thay vì để `IsEditable`
    tự mở khóa qua Status) — bỏ luôn `BeginDirtyTracking()` cũ ở đây (dời sang `Edit()`, đúng thời
    điểm sửa THẬT bắt đầu).
  - `InitializeAsync`: cả 2 nhánh (chứng từ mới/mở lại từ danh sách) đều set `IsReadOnly = false` —
    CHỈ Bỏ ghi NGAY TRONG phiên popup hiện tại mới kích hoạt khóa chờ "Sửa"; mở lại 1 chứng từ đã
    tồn tại (Confirmed hay Draft có sẵn từ trước) từ danh sách vẫn giữ hành vi cũ, không bắt bấm
    "Sửa" 2 lần (1 ở danh sách + 1 trong popup) — ngoài phạm vi yêu cầu.
- **`SalesReturnWindow.xaml`**: `DocumentToolbar` thêm `EditCommand="{Binding EditCommand}"` — tái
  dùng nút "Sửa" built-in đã có sẵn trong `DocumentToolbar` (label cứng "Sửa", tự ẩn khi không bind
  command), không cần thêm control mới.
- Build 0 lỗi. **Chưa test thật trên UTM.**

**Bổ sung cùng ngày**: bấm "Sửa" (`Edit()`) giờ append thêm `InitialEmptyLineCount` (100) dòng
trống vào `Lines` — `PopulateFormFromCurrent` (chạy lúc `InitializeAsync` mở chứng từ có sẵn) chỉ
nạp đúng số dòng THẬT, không có dòng trống dư như `ClearForm()` (chứng từ mới). Không dùng
`Lines.Clear()` — chỉ APPEND, giữ nguyên các dòng thật đã có, để gõ thêm sản phẩm ngay không cần tự
bấm "Thêm dòng" nhiều lần.

## Update — 2026-09-09: "Bỏ ghi" thật (đảo Confirmed→Draft + hoàn tác tồn kho)

Đảo ngược 1 phần quyết định "Update — 2026-09-07" bên dưới (dòng "`ConfirmSalesReturnUseCase` /
`UnconfirmSalesReturnUseCase` ... Đã xóa"): theo yêu cầu, "Bỏ ghi" giờ là đảo trạng thái THẬT (không
chỉ mở khóa form phía client), xác nhận qua `AskUserQuestion` — xem chi tiết BE ở
`be-window-lamour/.../SalesReturn/docs/sales-return.md`, mục "Update — 2026-09-09".

- **`SalesReturnViewModel.cs`**:
  - `IsEditable`/`IsConfirmed` đổi từ cờ `[ObservableProperty] _isEditable` thủ công (set `false` sau
    `SaveAsync`) sang **computed property tính thẳng từ `CurrentReturn.Status`** — nguồn sự thật duy
    nhất giờ là status BE trả về, không phải cờ tạm phía client nữa.
  - Thêm `UnconfirmCommand`/`UnconfirmAsync` (`CanExecute = IsConfirmed`) — confirm `MessageBox`, gọi
    `IUnconfirmSalesReturnUseCase` mới, gán `CurrentReturn = result` (Status → Draft, form tự mở khóa
    qua `IsEditable`), `BeginDirtyTracking()` lại (form editable trở lại, cần track dirty từ đây).
  - `SaveAsync`: bỏ dòng set thủ công `IsEditable = false` — tự động khóa lại vì `Update`/`Create` BE
    luôn ép `Status = Confirmed`.
  - Constructor: thêm `IUnconfirmSalesReturnUseCase`.
- **Data layer mới** (mirror pattern `DeleteAsync`): `ISalesReturnService`/`SalesReturnService`
  (`POST /{id}/unconfirm`), `ISalesReturnRepository`/`SalesReturnRepository`,
  `IUnconfirmSalesReturnUseCase`/`UnconfirmSalesReturnUseCase` (Domain/UseCases — chỉ forward tới
  repository, không có logic riêng).
- **DI**: `HomeServiceCollectionExtensions.cs` — thêm `IUnconfirmSalesReturnUseCase`.
- **`SalesReturnWindow.xaml`**: `DocumentToolbar` thêm `UnpostCommand="{Binding UnconfirmCommand}"`
  (label mặc định "Bỏ ghi" của `DocumentToolbar`, không override).
- **`SalesReturnListView.xaml`/`SalesReturnListViewModel.cs`**: **thêm lại** cột "Trạng thái" (badge
  màu, brand khi Confirmed/xám khi Draft) + filter ComboBox "Trạng thái:" (`StatusOptions`,
  `FilterStatus`) đã bị bỏ lúc "mọi chứng từ luôn Đã ghi sổ" (xem mục 5 dưới) — giờ có ý nghĩa phân
  biệt thật trở lại. `SalesReturnListItem.cs` (Status/IsDraft/IsConfirmed/StatusLabel) không đổi —
  các property này vẫn còn nguyên từ trước, chỉ UI bị gỡ.
- Build `dotnet build src/DesktopLamour` 0 lỗi. **Chưa test thật trên UTM.**

**Bug phát sinh (đã fix cùng ngày, phát hiện qua test UTM thật)**: `UnconfirmAsync` thiếu
`ReturnSaved?.Invoke()` — đây là tín hiệu DUY NHẤT `SalesReturnWindow.xaml.cs` dùng để set cờ
`_hasSaved` → `DialogResult = true` khi đóng popup, kích hoạt `SalesReturnListViewModel` reload
danh sách (`if (window.ShowDialog() == true) ...`). Thiếu dòng này thì đóng popup ngay sau "Bỏ ghi"
(không Cất lại) để danh sách không reload — chứng từ vẫn hiện dữ liệu CŨ. Fix: thêm
`ReturnSaved?.Invoke()` ngay đầu khối try, trước khi gán `CurrentReturn = result`.

**Đổi UI hiển thị trạng thái (user feedback sau khi thấy cột không rõ ràng)**: bỏ hẳn cột
"Trạng thái" riêng (đã thêm rồi bỏ lại trong cùng ngày) — chuyển sang **tô đậm + đổi màu chữ (brand)
cho CẢ DÒNG** khi `IsDraft == true`, qua `DataGrid.RowStyle` với `DataTrigger`. Mirror ĐÚNG pattern
đã có sẵn cho trạng thái "⏸ Treo" ở `Sales/Views/SalesOrderListView.xaml` (set `TextElement.Foreground`
kế thừa xuống mọi `TextBlock` con, không đụng `Background` nên không xung đột màu lúc dòng
Selected) — không phải thiết kế mới. Filter ComboBox "Trạng thái:" (`FilterStatus`/`StatusOptions`)
vẫn giữ nguyên, không đổi.

**Nút "Bỏ ghi" thêm vào toolbar danh sách** (`SalesReturnListView.xaml`, giữa "Sửa" và "Xóa`): bỏ
ghi thẳng 1 dòng đã chọn mà không cần mở popup trước. `SalesReturnListViewModel.cs` — thêm
`IUnconfirmSalesReturnUseCase` (constructor, đã đăng ký DI sẵn từ trước), `CanUnconfirmSelected`
(`SelectedReturn is { IsConfirmed: true }`), `UnconfirmSalesReturnCommand`/`UnconfirmSalesReturnAsync`
(confirm `MessageBox` → gọi use case → `LoadSalesReturnsAsync` reload) — mirror đúng cấu trúc
`DeleteSalesReturnAsync` cùng file. `OnSelectedReturnChanged` thêm
`UnconfirmSalesReturnCommand.NotifyCanExecuteChanged()`.

## Update — 2026-09-09: Nút "In" → Phiếu Nhập Kho; "Ghi sổ" giữ popup mở

Giải quyết Known Gap đã flag ở bản doc trước (nút "In" vẫn trỏ "Phiếu trả lại hàng bán" cũ) — theo
ảnh mẫu MISA "PHIẾU NHẬP KHO" user cung cấp, xác nhận qua `AskUserQuestion`: chứng từ vật lý doanh
nghiệp thực sự dùng khi nhận lại hàng là Phiếu Nhập Kho, không phải 1 layout hóa đơn riêng.

- **`PrintCommand`/`PrintAsync`** (đổi từ `Print()` sync) — không còn mở `SalesReturnPrintWindow`.
  Giờ: tìm PN liên kết qua `FindExistingWarehouseReceiptAsync` (helper mới, dùng chung với "Lập PN"),
  tự tạo nếu chưa có (`ICreateSalesReturnWarehouseReceiptUseCase`), lấy đầy đủ `WarehouseReceiptResponseDto`
  qua `IGetWarehouseReceiptByIdUseCase`, mở `Warehouse/Views/WarehouseReceiptPrintWindow` (Mẫu 01-VT).
  User không cần biết/bấm "Lập PN" trước — "In" tự lo bước đó.
- **`Views/SalesReturnPrintWindow.xaml(.cs)` đã XÓA HẲN** — không còn dùng ở đâu sau khi đổi nút In.
  DI: bỏ `services.AddTransient<SalesReturnPrintWindow>()` và `Func<SalesReturnPrintWindow>` khỏi
  `HomeServiceCollectionExtensions.cs`.
- Constructor `SalesReturnViewModel`: bỏ `Func<SalesReturnPrintWindow>`, thêm `IGetWarehouseReceiptByIdUseCase`
  + `Func<WarehouseReceiptPrintWindow>` (cả 2 đã đăng ký chung sẵn cho `WarehouseReceiptFormViewModel`,
  không cần đăng ký thêm).
- **"Ghi sổ" (`SaveAsync`) không còn tự đóng popup** — đổi từ `RequestClose?.Invoke()` sang
  `IsEditable = false` (property mới, khóa `Grid IsEnabled="{Binding IsEditable}"` của form + disable
  `SaveCommand` qua `CanExecute`); nút "In" tự bật lại qua `HasExistingReturn`/`OnCurrentReturnChanged`
  không đổi. `IsEditable` reset `true` mỗi lần `InitializeAsync` (mở chứng từ mới/mở lại từ danh sách).
  **Side-effect đã tự vá**: vì popup không tự đóng sau lưu nữa, `SalesReturnWindow.xaml.cs` thêm cờ
  `_hasSaved` (set qua event `ReturnSaved`) để set `DialogResult = true` khi cuối cùng user đóng bằng
  nút "Đóng"/X — nếu không, `SalesReturnListViewModel` (check `ShowDialog() == true`) sẽ không reload
  danh sách dù đã lưu.
- Đã build 0 lỗi từ máy Mac; **chưa test thật trên UTM** cho cả 2 thay đổi này.

## Update — 2026-09-08: "Ghi sổ" chỉ lưu — In & Lập PN là bước thủ công riêng

Theo yêu cầu (kèm video mẫu MISA "Chứng từ bán hàng"): bấm **Ghi sổ** không còn tự nhảy ra màn
in. Workflow "từng bước" — Ghi sổ → (mở lại chứng từ) → bấm **Lập PN** / **In** khi cần.

- **`SalesReturnViewModel.SaveAsync`**: bỏ khối `try { await EnsureWarehouseReceiptPrintedAsync(...) } catch { ... }`
  chạy sau khi lưu. Giờ chỉ: `StopDirtyTracking()` → `ReturnSaved?.Invoke()` → `CurrentReturn = result`
  → `RequestClose?.Invoke()` (đóng form, quay về danh sách). **Không tạo `WarehouseReceipt`, không mở
  cửa sổ in.** BE vẫn cộng tồn kho ngay trong Create/Update như cũ (2026-09-07).
- **Nút "Lập PN"** (`CreateWarehouseReceiptCommand`) — giờ **CHỈ tạo Phiếu Nhập Kho**, KHÔNG tự
  mở cửa sổ in. Đã xóa hẳn `EnsureWarehouseReceiptPrintedAsync`; command tự dedup (`ReceiptType == 2
  && Reference == DocumentNumber`) → nếu đã có thì báo số PN, nếu chưa thì gọi
  `ICreateSalesReturnWarehouseReceiptUseCase` rồi `MessageBox` báo thành công + hướng dẫn vào màn
  "Nhập, Xuất Kho" để in. Bỏ luôn 2 dependency chỉ-dùng-để-in: `IGetWarehouseReceiptByIdUseCase`,
  `Func<WarehouseReceiptPrintWindow>` (DI đăng ký chung vẫn còn cho consumer khác).
- **Nút "In"** (`PrintCommand` → `SalesReturnPrintWindow` "Phiếu trả lại hàng bán") không đổi — bật
  khi mở lại 1 chứng từ đã lưu (`HasExistingReturn`).
- **In Phiếu Nhập Kho**: làm ở màn "Nhập, Xuất Kho" — mở phiếu đó (`WarehouseReceiptFormWindow`) rồi
  bấm "In" (`WarehouseReceiptFormViewModel.Print`, `CanExecute = IsConfirmed`).
- **Chứng từ bán hàng** (`SalesOrderViewModel.SaveAsync`) sửa song song cùng lý do: bỏ
  `ShowPrintPreview(result, ...)` sau khi lưu; muốn in bấm nút "In" trên toolbar (`PrintCommand`,
  vốn in được cả khi chưa Ghi sổ). Xem `Sales/docs/sales.md`.

## Update — 2026-09-07: bỏ hẳn vòng đời Nháp → Ghi sổ

Chứng từ hàng bán bị trả lại không còn trạng thái **Nháp**. Lưu = đã ghi sổ ngay, BE cộng tồn kho
luôn; sửa/xóa được mọi chứng từ đã lưu (BE tự đảo tồn kho, giống Chứng từ bán hàng).

- **Màn danh sách** (`SalesReturnListViewModel` / `SalesReturnListView.xaml`): bỏ nút "📘 Ghi sổ" và
  "↩️ Bỏ ghi" + command `GhiSoCommand`/`BoGhiCommand`; `StatusOptions` còn `{ "Tất cả", "Đã ghi sổ" }`
  (bỏ "Nháp"); Sửa/Xóa bật khi có dòng chọn (bỏ điều kiện `IsDraft`). **Cột "Trạng thái" giữ nguyên**
  (luôn hiển thị "Đã ghi sổ").
- **Popup** (`SalesReturnViewModel` / `SalesReturnWindow.xaml`): bỏ `IConfirmSalesReturnUseCase`/
  `IUnconfirmSalesReturnUseCase`, bỏ nút "Bỏ ghi" trên `DocumentToolbar`, bỏ khối auto-Confirm trong
  `SaveAsync`, `IsEditable` luôn `true`, `CanDeleteReturn` chỉ cần `CurrentReturn != null`.
- **Data layer**: xóa `Confirm/UnconfirmSalesReturnUseCase` + interfaces, `ISalesReturnService.ConfirmAsync/
  UnconfirmAsync` + impl, `ISalesReturnRepository.ConfirmAsync/UnconfirmAsync` + impl, 2 dòng DI trong
  `HomeServiceCollectionExtensions.cs`. `SalesReturnResponseDto.Status` default `"Confirmed"`.
- **BE**: xem `be-window-lamour/.../SalesReturn/docs/sales-return.md` mục cùng ngày — không có migration.
- **Hàng tổng cộng cuối lưới danh sách** (theo ảnh mẫu MISA): thêm cột "Tiền thuế GTGT" (`SalesReturnListItem.TotalTax` = Σ `lines[].tax_amount`, tính client-side vì response không có tổng thuế cấp header) + `Border` footer canh cột hiển thị `Tổng cộng: N chứng từ` và Σ Tổng tiền hàng / Tổng CK / Tiền thuế GTGT / Tổng thanh toán — mirror đúng `SalesOrderListView`. `SalesReturnListViewModel` thêm `TotalCount`/`TotalGrossSum`/`TotalDiscountSum`/`TotalTaxSum`/`TotalPaymentSum` tính trong `LoadSalesReturnsAsync` (Σ trên toàn bộ list đã tải). Export Excel thêm cột "Tiền thuế GTGT". Thuần WPF — không đụng BE.

## PRD Summary

Popup `SalesReturnWindow` quản lý chứng từ "Hàng bán bị trả lại" — layout đã được update lại nhiều lượt trong phiên 2026-08-22 → 2026-08-28 để khớp giao diện tham chiếu kiểu MISA (ảnh mẫu user cung cấp), cộng thêm workflow "Ghi sổ → In Hoá Đơn" tự động và "Lập PN → In Phiếu Nhập Kho" (nhập lại hàng vào kho khi khách trả hàng).

## Key Components

```
Features/HomePage/SalesReturn/
  Domain/Models/SalesReturnLineItem.cs      — 1 dòng sản phẩm, INotifyPropertyChanged thủ công
  ViewModels/SalesReturnViewModel.cs        — toàn bộ logic popup: Lines, 4 tab, Ghi sổ, Lập PN, In
  Views/SalesReturnWindow.xaml(.cs)         — popup chính, 2 TabControl lồng nhau
```
(Không còn `Views/SalesReturnPrintWindow.xaml(.cs)` — đã xóa 2026-09-09, xem "Update — 2026-09-09".)

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

## Update — 2026-09-11: Bug fix "In Phiếu Nhập Kho ra sản phẩm cũ sau khi sửa chứng từ"

Sửa chứng từ trả hàng (thêm/đổi dòng) SAU khi đã bấm "In" (tức đã tự lập PN) → bấm "In" lại vẫn ra PN cũ, không khớp dòng hàng hiện tại. Root cause + fix đầy đủ nằm ở phía BE — xem "Update — 2026-09-11" trong `be-window-lamour/.../SalesReturn/docs/sales-return.md`. Tóm tắt phần đổi ở WPF:

- `PrintAsync` bỏ hẳn bước tự `FindExistingWarehouseReceiptAsync` trước khi gọi Create — giờ gọi thẳng `_createWarehouseReceipt.ExecuteAsync(CurrentReturn.Id, ct)`, để BE tự so khớp dòng hàng và quyết định tái dùng PN cũ hay supersede + lập PN mới.
- `WarehouseReceiptResponseDto` thêm field `is_superseded` (khớp BE).
- `FindExistingWarehouseReceiptAsync` (chỉ còn dùng bởi lệnh "Lập PN" cũ — đã gỡ khỏi UI nhưng code còn tồn tại) lọc thêm `!IsSuperseded`.

## Known Gaps / Follow-ups

- ~~Nút "In" riêng trên toolbar `SalesReturnWindow` vẫn trỏ tới `SalesReturnPrintWindow`/"Phiếu trả lại hàng bán" cũ~~ — **đã giải quyết 2026-09-09**, xem "Update — 2026-09-09" (nút "In" giờ mở `WarehouseReceiptPrintWindow`, tự Lập PN nếu chưa có).
- Export Excel/Print (nếu có báo cáo tổng hợp dùng lại dữ liệu SalesReturn) chưa cập nhật để show cột Status/Kiêm phiếu nhập mới.
- Chưa test thật trên UTM cho đợt sửa 2026-08-31 này — chỉ verify qua `dotnet build` 0 lỗi từ máy Mac; user tự xác nhận từng bước qua screenshot trong quá trình làm (đã fix 2 bug runtime thật do đó: duplicate WarehouseReceipt, "Item belongs to another collection").
- Chưa test thật trên UTM cho toàn bộ layout redesign + 2 workflow in ngày 2026-08-22 (mục cũ, vẫn còn hiệu lực).

---

*Created 2026-08-28: doc đầu tiên cho module SalesReturn phía WPF, tổng hợp lại toàn bộ layout redesign + 2 workflow in (Ghi sổ→In Hoá Đơn, Lập PN→In Phiếu Nhập Kho) + fix bug dòng trống/NV bán hàng thực hiện trong phiên 2026-08-22.*
*Updated 2026-08-31: Vietkey fix, gộp thật "Ghi sổ"→"Lập PN"/in Phiếu Nhập Kho, bỏ auto-fill Diễn giải, thêm workflow Ghi sổ/Bỏ ghi thật (BE Draft/Confirmed), redesign màn danh sách theo MISA, đổi mặc định bộ lọc ngày sang "Đầu tháng đến hiện tại".*
