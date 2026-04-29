# Phiếu Chi — Desktop App Documentation

> Feature: Phiếu Chi (Cash Payment)
> Module: Accounting (WPF)
> Created: 2026-04-29 (parallel to Phiếu Thu)

## User Flow

1. User vào màn **Kế Toán** (AccountingView)
2. Click nút **"Phiếu Chi"**
3. `PaymentWindow` mở như standalone window (không phải popup/ShowDialog)
4. User điền header info + thêm dòng hạch toán
5. Click **"Ghi số"** → POST/PUT lên BE → `AccountingView` (Quỹ Tiền Mặt) tự refresh

## Window Layout

```
Title: "Phiếu chi - CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR"
Size: 1100×720, WindowStartupLocation=CenterOwner

┌─ Toolbar ─────────────────────────────────────────────────────────────┐
│ [◀ Trước | Sau ▶]  [➕ Thêm | 🗑️ Xóa | ↩️ Hoàn | ✖️ Đóng]    [💾 Ghi số] │
│  Navigation group    Action group (grouped)          Primary action   │
└───────────────────────────────────────────────────────────────────────┘

┌─ Thông tin chung ───────────────┐  ┌─ Chứng từ ──────┐
│ Đối tượng   [Supplier ComboBox] │  │ Ngày hạch toán   │
│ Người nhận  [TextBox — auto]    │  │ [DatePicker]     │
│ Địa chỉ     [TextBox]           │  │ Ngày chứng từ    │
│ Lý do chi   [ComboBox]          │  │ [DatePicker]     │
│ Nhân viên chi [Employee ComboBox]│  │ Số chứng từ      │
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
| `Số chứng từ` | `"PC00001"` — user edits as needed |
| `Ngày hạch toán` | Today |
| `Ngày chứng từ` | Today |
| `Lý do chi` | `ChiKhac` |
| `TK Nợ` (new entry row) | `Receivable131` (131) |
| `TK Có` (new entry row) | `Cash111` (111) |

## Window Open Behavior

Window **luôn mở ở chế độ tạo mới** (blank form). `OnContentRendered` gọi `LoadAsync()` để load lookups + danh sách, sau đó tự gọi `AddNewCommand` để clear form. Dùng **Trước / Sau** để xem phiếu cũ.

## Dropdown Options

**Lý do chi (`PaymentReason`):**
```
ChiKhac      — Chi khác
ChiMuaHang   — Chi mua hàng
ChiTraNo     — Chi trả nợ
ChiLuong     — Chi lương
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
1. `PaymentViewModel` fires `PaymentSaved` → `AccountingViewModel.LoadAsync()` → Quỹ Tiền Mặt reload
2. `PaymentViewModel` fires `RequestClose` → `PaymentWindow.Close()` → cửa sổ tự đóng

```csharp
// AccountingViewModel.OpenPayment()
window.ViewModel.PaymentSaved  += () => _ = LoadAsync(CancellationToken.None);

// PaymentWindow constructor (code-behind)
viewModel.RequestClose += Close;
```

Cột **Diễn giải** trong Quỹ Tiền Mặt hiện **chỉ tên người nhận** — không có prefix.

## Architecture

```
AccountingView.xaml           "Phiếu Chi" button → OpenPaymentCommand
        ↓
AccountingViewModel           Func<PaymentWindow> factory → window.Show()
                              subscribes PaymentSaved → LoadAsync()
        ↓
PaymentWindow.xaml            Standalone Window, DataContext = PaymentViewModel
        ↓
PaymentViewModel              Full CRUD: Load, AddNew, Save, Delete, NavigatePrev/Next
                              fires PaymentSaved after successful save
        ↓
ICreatePaymentUseCase         → IPaymentService.CreateAsync()
IUpdatePaymentUseCase         → IPaymentService.UpdateAsync()
IDeletePaymentUseCase         → IPaymentService.DeleteAsync()
IGetPaymentsUseCase           → IPaymentService.GetAllAsync()
IGetPaymentByIdUseCase        → IPaymentService.GetByIdAsync()
IDuplicatePaymentUseCase      → IPaymentService.DuplicateAsync()
        ↓
PaymentService                HttpClient → http://192.168.64.1:5282
```

## DI Registration (HomeServiceCollectionExtensions.cs)

```csharp
services.AddTransient<PaymentWindow>();
services.AddTransient<PaymentViewModel>();
services.AddTransient<IGetPaymentsUseCase, GetPaymentsUseCase>();
services.AddTransient<IGetPaymentByIdUseCase, GetPaymentByIdUseCase>();
services.AddTransient<ICreatePaymentUseCase, CreatePaymentUseCase>();
services.AddTransient<IUpdatePaymentUseCase, UpdatePaymentUseCase>();
services.AddTransient<IDeletePaymentUseCase, DeletePaymentUseCase>();
services.AddTransient<IDuplicatePaymentUseCase, DuplicatePaymentUseCase>();
services.AddTransient<Func<PaymentWindow>>(sp => () => sp.GetRequiredService<PaymentWindow>());
services.AddHttpClient<IPaymentService, PaymentService>(client =>
{
    client.BaseAddress = new Uri("http://192.168.64.1:5282");
    client.Timeout     = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

## WPF Files Structure

```
desktop-lamour/src/DesktopLamour/Features/HomePage/Accounting/
  Data/Services/
    Dtos/
      PaymentEntryDto.cs                    — Line item DTO (mirrors BE)
      PaymentResponseDto.cs                 — Full payment DTO (mirrors BE)
      CreatePaymentRequestDto.cs            — Create request DTO (mirrors BE)
      UpdatePaymentRequestDto.cs            — Update request DTO (mirrors BE)
    IPaymentService.cs                      — Service interface
    PaymentService.cs                       — HttpClient implementation
  
  Domain/
    Models/
      PaymentEntryItem.cs                   — INotifyPropertyChanged wrapper for DataGrid
    UseCases/
      IGetPaymentsUseCase.cs                — Interface: Get all payments
      GetPaymentsUseCase.cs                 — Implementation
      IGetPaymentByIdUseCase.cs             — Interface: Get payment by ID
      GetPaymentByIdUseCase.cs              — Implementation
      ICreatePaymentUseCase.cs              — Interface: Create payment
      CreatePaymentUseCase.cs               — Implementation
      IUpdatePaymentUseCase.cs              — Interface: Update payment
      UpdatePaymentUseCase.cs               — Implementation
      IDeletePaymentUseCase.cs              — Interface: Delete payment
      DeletePaymentUseCase.cs               — Implementation
      IDuplicatePaymentUseCase.cs           — Interface: Duplicate payment
      DuplicatePaymentUseCase.cs            — Implementation
  
  ViewModels/
    PaymentViewModel.cs                     — CRUD logic, navigation, PaymentSaved event
    AccountingViewModel.cs                  — Updated with OpenPaymentCommand
  
  Views/
    PaymentWindow.xaml                      — Standalone Window UI
    PaymentWindow.xaml.cs                   — Code-behind (wires RequestClose event)
    AccountingView.xaml                     — Updated with "Phiếu Chi" button
  
  docs/
    phieu-chi.md                            — This documentation file

../HomeServiceCollectionExtensions.cs       — DI registration (updated)
```

## Backend API Endpoints

```
GET    /api/v1/accounting/payments           — Get all payments
GET    /api/v1/accounting/payments/{id}      — Get payment by ID
POST   /api/v1/accounting/payments           — Create new payment
PUT    /api/v1/accounting/payments/{id}      — Update payment
DELETE /api/v1/accounting/payments/{id}      — Delete payment
POST   /api/v1/accounting/payments/{id}/duplicate — Duplicate payment
```

## Auto-Population Features

When a supplier is selected from the **Đối tượng** dropdown:
- **Người nhận** is automatically populated with the supplier's name
- **Địa chỉ** is automatically populated with the supplier's address

This reduces manual data entry and ensures consistency with supplier master data.

## Key Differences from Phiếu Thu

| Aspect | Phiếu Thu (Receipt) | Phiếu Chi (Payment) |
|--------|---------------------|---------------------|
| **Đối tượng** | Customer (Khách hàng) | Supplier (Nhà cung cấp) |
| **Người** | Người nộp (Payer) | Người nhận (Payee) |
| **Nhân viên** | Người thu (Collector) | Người chi (Payment Employee) |
| **Lý do** | ThuKhac, ThuTienHang, ThuCongNo | ChiKhac, ChiMuaHang, ChiTraNo, ChiLuong |
| **Số chứng từ** | PT00067 | PC00001 |
| **TK Nợ default** | Cash111 (111) | Receivable131 (131) |
| **TK Có default** | Receivable131 (131) | Cash111 (111) |
| **Cash flow** | Inbound (Thu tiền) | Outbound (Chi tiền) |

## Known Limitations / Future Work

- `Số chứng từ` là free-text — không validate uniqueness trên WPF
- `Tham chiếu` search button chưa có lookup action
- `+` button cạnh `Đối tượng` và `Nhân viên chi` chưa có quick-add flow
- Duplicate feature có sẵn nhưng chưa có UI button (có thể thêm sau)

## Implementation Notes

This feature was auto-generated from `ReceiptWindow` using automated find/replace:
- `Receipt` → `Payment`
- `Customer` → `Supplier`
- `Payer` → `Payee`
- `Collector` → `PaymentEmployee`
- `ThuKhac/ThuTienHang/ThuCongNo` → `ChiKhac/ChiMuaHang/ChiTraNo/ChiLuong`

All backend entities, DTOs, repositories, use cases, and controllers follow Clean Architecture patterns identical to the Receipt feature.

## Testing Checklist

- [ ] Open PaymentWindow from AccountingView
- [ ] Select supplier → verify auto-population of Người nhận + Địa chỉ
- [ ] Add payment entries to DataGrid
- [ ] Save payment → verify API call succeeds
- [ ] Verify Quỹ Tiền Mặt auto-refreshes after save
- [ ] Navigate between payments using Trước/Sau buttons
- [ ] Delete payment → verify confirmation + API call
- [ ] Test validation (empty fields, invalid amounts)
