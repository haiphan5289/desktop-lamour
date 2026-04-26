# Phiếu Thu — Desktop App Documentation

> Feature: Thu tiền khách hàng (Payment Receipt)
> Module: Accounting (WPF)
> Implemented: 2026-04-26

## User Flow

1. User vào màn **Kế Toán** (AccountingView)
2. Click nút **"📄 Phiếu Thu"** trong thanh action
3. Popup `PaymentReceiptWindow` mở (860px, CenterOwner)
4. User điền thông tin header + thêm dòng chứng từ
5. Click **"Thu tiền"** → POST lên BE → popup đóng → AccountingView tự refresh

## Screen Fields

### Header form

| Field | Binding | Notes |
|-------|---------|-------|
| Phương thức thanh toán | `SelectedPaymentMethod` | RadioButton: "Cash" / "BankTransfer" |
| Loại tiền | `Currency` | TextBox, default "VND" |
| Tỷ giá | `ExchangeRate` | TextBox decimal, default 1.00 |
| Khách hàng | `CustomerCode` | TextBox (nhập mã KH, vd KH00002) |
| Ngày thu tiền | `CollectionDate` | DatePicker |
| NV bán hàng | `EmployeeCode` | TextBox (nhập mã NV, vd NV003) |
| Số tiền | `TotalAmount` | TextBox decimal |

### Chứng từ công nợ — DataGrid

| Column | Binding | Type |
|--------|---------|------|
| Ngày chứng từ | `DocumentDate` | DatePicker |
| Số chứng từ | `DocumentNumber` | TextBox |
| Số hóa đơn | `InvoiceNumber` | TextBox |
| Diễn giải | `Description` | TextBox |
| Hạn thanh toán | `DueDate` | DatePicker (nullable) |
| Số phải thu | `AmountDue` | decimal TextBox |
| Số thanh toán | `AmountPaid` | decimal TextBox |

### Buttons

| Button | Command | Action |
|--------|---------|--------|
| + Thêm dòng | `AddLineCommand` | Thêm dòng trống vào DataGrid |
| Thu tiền | `SaveCommand` | POST → đóng popup (DialogResult = true) |
| Hủy bỏ | Closes window | DialogResult = null |

## Architecture

```
AccountingView.xaml          "📄 Phiếu Thu" button → OpenPaymentReceiptCommand
        ↓
AccountingViewModel           Func<PaymentReceiptWindow> factory → ShowDialog()
        ↓
PaymentReceiptWindow.xaml     Popup Window, DataContext = PaymentReceiptViewModel
        ↓
PaymentReceiptViewModel       SaveCommand → ICreatePaymentReceiptUseCase
        ↓
CreatePaymentReceiptUseCase   → IPaymentReceiptService.CreateAsync()
        ↓
PaymentReceiptService         HttpClient POST /api/v1/accounting/payment-receipts
```

## DI Registration (HomeServiceCollectionExtensions.cs)

```csharp
// Accounting — PaymentReceipt
services.AddTransient<PaymentReceiptWindow>();
services.AddTransient<PaymentReceiptViewModel>();
services.AddTransient<ICreatePaymentReceiptUseCase, CreatePaymentReceiptUseCase>();
services.AddTransient<Func<PaymentReceiptWindow>>(sp => () => sp.GetRequiredService<PaymentReceiptWindow>());
services.AddHttpClient<IPaymentReceiptService, PaymentReceiptService>(client =>
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
    Dtos/CreatePaymentReceiptRequestDto.cs
    Dtos/PaymentReceiptResponseDto.cs
    IPaymentReceiptService.cs
    PaymentReceiptService.cs
  Domain/
    Models/PaymentReceiptLineItem.cs
    UseCases/ICreatePaymentReceiptUseCase.cs
    UseCases/CreatePaymentReceiptUseCase.cs
  ViewModels/
    PaymentReceiptViewModel.cs       (new)
    AccountingViewModel.cs           (updated — OpenPaymentReceiptCommand)
  Views/
    PaymentReceiptWindow.xaml        (new)
    PaymentReceiptWindow.xaml.cs     (new)
    AccountingView.xaml              (updated — added Phiếu Thu button)

Shared/
  Converters/StringEqualityConverter.cs  (new — RadioButton ↔ string)
  AppConverters.xaml                     (updated — registered StringEqualityConverter)

HomeServiceCollectionExtensions.cs       (updated)
```

## Known Limitations / Future Work

- `CustomerCode` và `EmployeeCode` hiện là TextBox nhập tay — nên thêm ComboBox/lookup dropdown sau
- Chưa có "Lấy dữ liệu" (fetch outstanding invoices) — skip theo spec ban đầu
- Khi ExportInvoice được implement, "Lấy dữ liệu" sẽ gọi `GET /api/v1/accounting/outstanding-invoices?customer_id=`
