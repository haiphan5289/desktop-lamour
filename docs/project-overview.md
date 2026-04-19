# Desktop Lamour — Phần Mềm Quản Lý Mỹ Phẩm

## Giới thiệu

**Desktop Lamour** là ứng dụng desktop WPF (.NET 8) dành cho các cửa hàng / doanh nghiệp kinh doanh mỹ phẩm. Phần mềm hỗ trợ quản lý toàn diện: nhân viên, kho hàng, nhập xuất hoá đơn, giúp chủ cửa hàng kiểm soát hoạt động kinh doanh dễ dàng.

---

## Mục tiêu

- Quản lý danh sách nhân viên và phân quyền.
- Quản lý kho hàng mỹ phẩm (nhập, xuất, tồn kho).
- Lập và theo dõi hoá đơn nhập hàng từ nhà cung cấp.
- Lập và theo dõi hoá đơn xuất hàng / bán lẻ cho khách.
- Thống kê doanh thu, tồn kho theo kỳ.

---

## Công nghệ

| Thành phần | Chi tiết |
|---|---|
| Nền tảng | .NET 8 / WPF (Windows) |
| Kiến trúc | MVVM + Clean Architecture |
| DI | `Microsoft.Extensions.DependencyInjection` |
| MVVM Toolkit | `CommunityToolkit.Mvvm` 8.3.2 |
| HTTP | `Microsoft.Extensions.Http` |
| Logging | `Microsoft.Extensions.Logging` |
| Target platform | x64 / ARM64 |

---

## Kiến trúc dự án

```
src/DesktopLamour/
├── Features/                  # Các tính năng theo module
│   ├── Authentication/        # Đăng nhập / đăng ký
│   │   ├── Domain/            # Models, UseCases (interface)
│   │   ├── Data/              # Services, Repositories, DTOs
│   │   └── Views/ ViewModels/ # UI + ViewModel
│   ├── Employees/             # Quản lý nhân viên
│   ├── Inventory/             # Quản lý kho
│   ├── ImportInvoices/        # Hoá đơn nhập hàng
│   └── ExportInvoices/        # Hoá đơn xuất hàng / bán lẻ
├── Core/                      # Shared infrastructure
│   ├── Navigation/
│   ├── Storage/
│   ├── UseCases/
│   └── ViewModels/
├── Shared/                    # Controls, styles dùng chung
├── Themes/                    # AppTypography, AppStyles, màu sắc
└── MainWindow/                # Shell window
```

Mỗi feature tuân theo pattern 3 lớp:

```
Domain (Models + UseCase interfaces)
  └── Data (Service gọi API, Repository)
        └── Presentation (ViewModel + View/XAML)
```

---

## Các module chính

### 1. Xác thực (Authentication)
- Đăng ký tài khoản bằng số điện thoại.
- Kiểm tra số điện thoại đã tồn tại.
- Đăng nhập và lưu phiên làm việc.

### 2. Quản lý nhân viên (Employees)
- Thêm, sửa, xoá hồ sơ nhân viên.
- Phân quyền: Admin / Thu ngân / Kho.
- Lịch sử hoạt động của từng nhân viên.

### 3. Quản lý kho (Inventory)
- Danh mục sản phẩm mỹ phẩm (tên, mã, thương hiệu, đơn vị, giá).
- Theo dõi số lượng tồn kho theo thời gian thực.
- Cảnh báo hàng sắp hết.

### 4. Hoá đơn nhập hàng (Import Invoices)
- Tạo phiếu nhập từ nhà cung cấp.
- Danh sách sản phẩm nhập, số lượng, đơn giá, tổng tiền.
- Cập nhật tồn kho tự động sau khi xác nhận nhập.

### 5. Hoá đơn xuất hàng / bán lẻ (Export Invoices)
- Tạo hoá đơn bán hàng cho khách.
- Hỗ trợ chiết khấu, thuế.
- In hoá đơn / xuất PDF.
- Trừ tồn kho tự động sau khi xác nhận xuất.

---

## Luồng dữ liệu

```
View (XAML)
  ↕ binding
ViewModel (CommunityToolkit.Mvvm)
  ↕ UseCase interface
UseCase (Domain)
  ↕ Repository interface
Repository (Data)
  ↕ HTTP / local storage
Service / API
```

---

## Cài đặt & chạy

### Yêu cầu
- Windows 10/11 (x64 hoặc ARM64)
- .NET 8 SDK
- Visual Studio 2022+ hoặc Rider

### Chạy local

```bash
cd src/DesktopLamour
dotnet run
```

### Build release

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

---

## Tài liệu liên quan

- [Setup Windows VM với UTM](setup-windows-vm-utm.md)
