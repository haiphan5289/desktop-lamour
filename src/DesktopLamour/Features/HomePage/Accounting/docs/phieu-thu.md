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

┌─ Toolbar ─────────────────────────────────────────────┐
│ Trước | Sau | Thêm | Ghi số | Xóa | Hoàn | Đóng      │
└───────────────────────────────────────────────────────┘

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

Sau khi save thành công, `ReceiptViewModel` fires `ReceiptSaved` event.
`AccountingViewModel` subscribes khi mở window → tự gọi `LoadAsync()` → Quỹ Tiền Mặt reload.

```csharp
// AccountingViewModel.OpenReceipt()
window.ViewModel.ReceiptSaved += () => _ = LoadAsync(CancellationToken.None);
```

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

## Known Limitations / Future Work

- `Số chứng từ` là free-text — không validate uniqueness trên WPF
- `Tham chiếu` search button chưa có lookup action
- `+` button cạnh `Đối tượng` và `Nhân viên thu` chưa có quick-add flow
