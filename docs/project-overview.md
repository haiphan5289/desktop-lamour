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

## Mô hình hoạt động

Desktop Lamour hoạt động theo mô hình **Client–Server**:

- **Backend (BE)** là nguồn dữ liệu duy nhất — xử lý business logic, lưu trữ, xác thực.
- **App (WPF)** là client — hiển thị dữ liệu do BE trả về, người dùng thao tác trực tiếp trên App.
- Mọi thao tác (đăng nhập, tạo hoá đơn, cập nhật kho...) đều gửi request lên BE và render response về UI.

```
BE (API Server)
  ↕ HTTP/JSON
App (Desktop WPF) ← User thao tác
```

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

## Workflow phát triển trên Windows VM (UTM)

Desktop Lamour chạy trong Windows VM (UTM) trên Mac Apple Silicon. Code được chỉnh sửa trên Mac, sau đó sync sang VM để build và chạy.

> **Lý do không chạy trực tiếp từ Z:\ (shared folder):**
> MSBuild không thể glob `**/*.xaml` qua network drive → lỗi BG1002/BG1003.
> Phải copy toàn bộ project sang ổ C:\ local trước khi chạy.

### Script: `sync.ps1` — Đồng bộ code từ Mac → VM

```powershell
robocopy Z:\ C:\projects\desktop-lamour\ /MIR /IS /XD bin obj .git .vs .claude node_modules /XF *.user /NFL /NDL /NJH
Write-Host "Synced! dotnet watch will auto-reload the app." -ForegroundColor Green
```

| Flag | Ý nghĩa |
|---|---|
| `/MIR` | Mirror — xoá file trên đích nếu đã bị xoá trên nguồn |
| `/IS` | Include Same — copy lại kể cả file không đổi (đảm bảo mới nhất) |
| `/XD bin obj .git ...` | Bỏ qua các thư mục build artifact và metadata |
| `/XF *.user` | Bỏ qua file cấu hình cá nhân của VS |
| `/NFL /NDL /NJH` | Tắt log chi tiết — chỉ hiện thông báo lỗi |

**Khi nào chạy:** Mỗi khi thêm file mới trên Mac (`.xaml`, `.cs`, thư mục mới). Với file đã có, `dotnet watch` tự reload qua shared folder.

### Script: `start-watch.ps1` — Khởi động hot-reload

```powershell
cd C:\projects\desktop-lamour
dotnet watch run --project src\DesktopLamour\DesktopLamour.csproj
```

Chạy `dotnet watch` từ thư mục local C:\. Khi file `.cs` hoặc `.xaml` thay đổi (qua sync), app tự rebuild và reload mà không cần restart thủ công.

### Workflow hàng ngày — 2 terminal song song

`start-watch.ps1` chạy liên tục (blocking), nên cần **2 terminal riêng** trong VM:

```
Terminal 1 (giữ mở suốt)       Terminal 2 (dùng khi cần sync)
──────────────────────────      ──────────────────────────────
.\start-watch.ps1               .\sync.ps1
  │                               │
  │  dotnet watch đang chờ...     │  robocopy Z:\ → C:\  (vài giây)
  │                               │  "Synced!" ✓
  └── tự reload khi file đổi ←───┘  dotnet watch bắt file mới
```

**Khi nào dùng Terminal 2:**

| Tình huống | Cần sync? |
|---|---|
| Sửa file `.cs` / `.xaml` đã có, Cmd+S trên Mac | Không — dotnet watch tự nhận qua Z:\ |
| Thêm file mới (`.cs`, `.xaml`, thư mục) | **Có** — chạy `.\sync.ps1` |
| Xoá file trên Mac | **Có** — `/MIR` sẽ xoá tương ứng trên C:\ |

**Lưu ý:** `sync.ps1` phải chạy lại khi có file mới. Với file đã tồn tại, chỉ cần save trên Mac là đủ.

---

## Tài liệu liên quan

- [Setup Windows VM với UTM](setup-windows-vm-utm.md)
- [UTM Run Workflow](utm-run-workflow.md)
