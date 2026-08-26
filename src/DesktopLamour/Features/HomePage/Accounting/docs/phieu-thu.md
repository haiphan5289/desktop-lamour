# Phiếu Thu — Desktop App Documentation

> Feature: Phiếu Thu (Cash Receipt)
> Module: Accounting (WPF)
> Rebuilt: 2026-04-29 (replaced PaymentReceiptWindow popup → standalone ReceiptWindow)

## User Flow

1. User vào màn **Kế Toán** (AccountingView)
2. Click nút **"Phiếu Thu"**
3. `ReceiptWindow` mở như standalone window (không phải popup/ShowDialog)
4. User điền header info + thêm dòng hạch toán
5. Click **"Ghi số"** → POST/PUT lên BE → `AccountingView` (Quỹ Tiền Mặt) tự refresh

## Window Layout

```
Title: "Phiếu thu - CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR"
Size: 1100×720, WindowStartupLocation=CenterOwner

┌─ Toolbar ─────────────────────────────────────────────────────────────┐
│ [◀ Trước | Sau ▶]  [➕ Thêm | 🗑️ Xóa | ↩️ Hoàn | ✖️ Đóng]    [💾 Ghi số] │
│  Navigation group    Action group (grouped)          Primary action   │
└───────────────────────────────────────────────────────────────────────┘

┌─ Thông tin chung ───────────────┐  ┌─ Chứng từ ──────┐
│ Đối tượng   [Customer ComboBox] │  │ Ngày hạch toán   │
│ Người nộp   [TextBox — auto]    │  │ [DatePicker]     │
│ Địa chỉ     [TextBox]           │  │ Ngày chứng từ    │
│ Lý do nộp   [ComboBox]          │  │ [DatePicker]     │
│ Nhân viên thu [Employee ComboBox]│  │ Số chứng từ      │
│ Kèm theo    [TextBox]           │  │ [TextBox]        │
│ Tham chiếu  [TextBox]           │  └──────────────────┘
└─────────────────────────────────┘

┌─ 1. Hạch toán ────────────────────────────────────────┐
│ DataGrid: Diễn giải | TK Nợ | TK Có | Số tiền |       │
│           Đối tượng | Tên đối tượng | TK ngân hàng    │
└───────────────────────────────────────────────────────┘

Footer: Số dòng = N                          {total}
```

## Field Defaults (New Form)

| Field | Default |
|-------|---------|
| `Số chứng từ` | `"PT00067"` — user edits as needed |
| `Ngày hạch toán` | Today |
| `Ngày chứng từ` | Today |
| `Lý do nộp` | `ThuKhac` |
| `TK Nợ` (new entry row) | `Cash111` (111) |
| `TK Có` (new entry row) | `Receivable131` (131) |

## Window Open Behavior

Window **luôn mở ở chế độ tạo mới** (blank form). `OnContentRendered` gọi `LoadAsync()` để load lookups + danh sách, sau đó tự gọi `AddNewCommand` để clear form. Dùng **Trước / Sau** để xem phiếu cũ.

## Dropdown Options

**Lý do nộp (`PaymentReason`):**
```
ThuKhac      — Thu khác
ThuTienHang  — Thu tiền hàng
ThuCongNo    — Thu công nợ
```

**TK Nợ / TK Có (`AccountCode`):**
```
Cash111        — 111 Tiền mặt
Bank112        — 112 Tiền gửi ngân hàng
Receivable131  — 131 Phải thu khách hàng
Payroll334     — 334 Phải trả người lao động
```

## Quỹ Tiền Mặt Auto-Refresh

Sau khi save thành công:
1. `ReceiptViewModel` fires `ReceiptSaved` → `AccountingViewModel.LoadAsync()` → Quỹ Tiền Mặt reload
2. `ReceiptViewModel` fires `RequestClose` → `ReceiptWindow.Close()` → cửa sổ tự đóng

```csharp
// AccountingViewModel.OpenReceipt()
window.ViewModel.ReceiptSaved  += () => _ = LoadAsync(CancellationToken.None);

// ReceiptWindow constructor (code-behind)
viewModel.RequestClose += Close;
```

Cột **Diễn giải** trong Quỹ Tiền Mặt hiện **chỉ tên người nộp** — không có prefix.

## Architecture

```
AccountingView.xaml           "Phiếu Thu" button → OpenReceiptCommand
        ↓
AccountingViewModel           Func<ReceiptWindow> factory → window.Show()
                              subscribes ReceiptSaved → LoadAsync()
        ↓
ReceiptWindow.xaml            Standalone Window, DataContext = ReceiptViewModel
        ↓
ReceiptViewModel              Full CRUD: Load, AddNew, Save, Delete, NavigatePrev/Next
                              fires ReceiptSaved after successful save
        ↓
ICreateReceiptUseCase         → IReceiptService.CreateAsync()
IUpdateReceiptUseCase         → IReceiptService.UpdateAsync()
IDeleteReceiptUseCase         → IReceiptService.DeleteAsync()
IGetReceiptsUseCase           → IReceiptService.GetAllAsync()
IGetReceiptByIdUseCase        → IReceiptService.GetByIdAsync()
        ↓
ReceiptService                HttpClient → http://192.168.64.1:5282
```

## DI Registration (HomeServiceCollectionExtensions.cs)

```csharp
services.AddTransient<ReceiptWindow>();
services.AddTransient<ReceiptViewModel>();
services.AddTransient<IGetReceiptsUseCase, GetReceiptsUseCase>();
services.AddTransient<IGetReceiptByIdUseCase, GetReceiptByIdUseCase>();
services.AddTransient<ICreateReceiptUseCase, CreateReceiptUseCase>();
services.AddTransient<IUpdateReceiptUseCase, UpdateReceiptUseCase>();
services.AddTransient<IDeleteReceiptUseCase, DeleteReceiptUseCase>();
services.AddTransient<Func<ReceiptWindow>>(sp => () => sp.GetRequiredService<ReceiptWindow>());
services.AddHttpClient<IReceiptService, ReceiptService>(client =>
{
    client.BaseAddress = new Uri("http://192.168.64.1:5282");
    client.Timeout     = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

## Files

```
Features/HomePage/Accounting/
  Data/Services/
    Dtos/ReceiptEntryDto.cs
    Dtos/ReceiptResponseDto.cs
    Dtos/CreateReceiptRequestDto.cs
    Dtos/UpdateReceiptRequestDto.cs
    IReceiptService.cs
    ReceiptService.cs
  Domain/
    Models/ReceiptEntryItem.cs          (INotifyPropertyChanged — DataGrid binding)
    UseCases/IGetReceiptsUseCase.cs + GetReceiptsUseCase.cs
    UseCases/IGetReceiptByIdUseCase.cs + GetReceiptByIdUseCase.cs
    UseCases/ICreateReceiptUseCase.cs + CreateReceiptUseCase.cs
    UseCases/IUpdateReceiptUseCase.cs + UpdateReceiptUseCase.cs
    UseCases/IDeleteReceiptUseCase.cs + DeleteReceiptUseCase.cs
  ViewModels/
    ReceiptViewModel.cs                 (CRUD, navigation, ReceiptSaved event)
    AccountingViewModel.cs              (updated — OpenReceiptCommand, ReceiptSaved subscription)
  Views/
    ReceiptWindow.xaml                  (standalone Window)
    ReceiptWindow.xaml.cs
    AccountingView.xaml                 (updated — button → OpenReceiptCommand)

HomeServiceCollectionExtensions.cs      (updated)
```

## Removed (replaced by this rebuild)

- `PaymentReceiptWindow.xaml` — popup dialog (replaced by `ReceiptWindow` standalone)
- `PaymentReceiptViewModel.cs`
- `IPaymentReceiptService` / `PaymentReceiptService`
- `CreatePaymentReceiptRequestDto` / `PaymentReceiptResponseDto`
- `PaymentReceiptLineItem.cs`
- `ICreatePaymentReceiptUseCase` / `CreatePaymentReceiptUseCase`
- All related DI registrations in `HomeServiceCollectionExtensions.cs`

## Auto-Population Features

When a customer is selected from the **Đối tượng** dropdown:
- **Người nộp** is automatically populated with the customer's name
- **Địa chỉ** is automatically populated with the customer's address

This reduces manual data entry and ensures consistency with customer master data.

## Phiếu thu tiền khách hàng hàng loạt (2026-08-26 — so ảnh mẫu MISA)

`AccountingViewModel.OpenBulkCustomerReceiptCommand` → `BulkCustomerReceiptSearchWindow` (tìm chứng từ bán hàng còn nợ, tick chọn nhiều dòng — có thể nhiều khách hàng khác nhau) → "✔ Thu tiền" mở `BulkCustomerReceiptWindow` (popup xác nhận, sửa số tiền từng dòng) → "💾 Cất" → tạo **1 phiếu thu duy nhất** trên BE (xem doc BE `be-window-lamour/.../Accounting/docs/phieu-thu.md`, mục cùng tên — trước 2026-08-26 tạo N phiếu, mỗi khách hàng 1 phiếu riêng).

**`BulkCustomerReceiptWindow` — rebuild toàn bộ để khớp ảnh mẫu** (trước đây chỉ có 1 grid + nút Cất, không có header/tab):

- Thêm đầy đủ "Thông tin chung": `PayerName` (Người nộp, editable — auto-fill từ tên NV thu nợ chọn nếu còn trống, **không phải** tên khách hàng vì phiếu gộp nhiều khách), `Address`, `ReasonLabel` (Lý do nộp, cố định "Thu tiền khách hàng", read-only — chỉ 1 lý do khả dĩ cho luồng này), `SelectedCollectorEmployee` (NV thu nợ), `Attachment` (Kèm theo), `Reference` (Tham chiếu, read-only — tự nối `DocumentNumber` các đơn đã chọn).
- Thêm khối "Chứng từ": `AccountingDate`/`DocumentDate` (editable) + `DocumentNumber` (read-only, **dự đoán** qua `IGetNextReceiptCodeUseCase.ExecuteAsync()` lúc `Initialize()` — số thật do BE gán lúc Cất, giống pattern `GenerateNextDocumentNumber()` ở các form khác, có thể lệch nếu có phiếu khác tạo song song).
- Tab **"1. Hạch toán"**: grid cũ (Diễn giải/Mã KH/Tên KH/Số tiền) + thêm cột **TK Nợ**/**TK Có** (`BulkReceiptLineItem.DebitAccountDisplay`/`CreditAccountDisplay` — gán 1 lần từ "Phương thức thanh toán" chọn ở popup tìm kiếm, hiển thị tĩnh, không sửa riêng từng dòng vì cả phiếu dùng chung 1 cặp TK).
- Tab **"2. Chứng từ"** (mới) — Ngày chứng từ/Số chứng từ/Mã KH/Tên KH/Hạn thanh toán/Số phải thu/Số chưa thu/Số thu/TK phải thu/Điều khoản TT — `Hạn thanh toán`/`Điều khoản TT`/`Số phải thu` lấy từ `SalesOrder.PaymentDueDate`/`PaymentTerms`/`GrandTotal` có sẵn (BE bổ sung vào `OutstandingSalesOrderDto`, không phải field mới). **Không có** "Tỷ lệ CK (%)"/"Tiền chiết khấu"/"TK chiết khấu" — `SalesOrder` không có field chiết khấu thanh toán sớm ở mức chứng từ (khác `SalesOrderLine.DiscountRate` per-dòng sản phẩm đã có).
- Toolbar: chỉ **"💾 Cất"** + **"Hủy bỏ"** — **không thêm "↩️ Hoàn"**: nút "Hoàn" trên `ReceiptWindow` (đơn lẻ) thực chất chỉ là "huỷ sửa, tải lại danh sách" (`CancelAsync` → `LoadReceiptsAsync`), không có ý nghĩa với popup 1-lần-rồi-đóng như `BulkCustomerReceiptWindow` (không có khái niệm "danh sách đang duyệt" để quay lại).
- `BulkCustomerReceiptSearchWindow` footer: thêm `LineSummary` ("Số dòng = N") — trước đây không đếm dòng.

**Cố tình không làm** (đã ghi trong doc BE, không lặp lại lý do ở đây): cột "Số hóa đơn", màn danh sách "Thu tiền khách hàng hàng loạt" riêng (đã có sẵn trong Sổ Kế Toán Chi Tiết Quỹ Tiền Mặt — mọi phiếu hàng loạt vẫn post 1 `CashTransaction` như phiếu thường nên tự hiện ở đó), Draft/Treo/Confirmed/"Hoàn" thật cho Receipt.

## Known Limitations / Future Work

- `Số chứng từ` là free-text — không validate uniqueness trên WPF
- `Tham chiếu` search button chưa có lookup action
- `+` button cạnh `Đối tượng` và `Nhân viên thu` chưa có quick-add flow
