# Build WPF App on macOS via UTM (Windows 11 ARM)

## Trạng thái hiện tại
- ✅ UTM đã cài tại `/Applications/UTM.app`
- ✅ Windows 11 ARM ISO đã tải
- ✅ VM đã tạo và đang hoạt động

## Yêu cầu
- Mac Apple Silicon (M1/M2/M3/M4)
- UTM đã cài: `/Applications/UTM.app`
- Dung lượng trống: ~60GB
- RAM: 16GB Mac trở lên (cấp 8GB cho VM)

---

## Bước 1: Tải Windows 11 ARM ISO

Mở Safari, vào đúng link này:

**https://www.microsoft.com/en-us/software-download/windows11arm64**

- Chọn **"Windows 11 (multi-edition ISO for ARM64 devices)"**
- Chọn ngôn ngữ → **Download 64-bit**
- File ~6GB — chờ tải xong, lưu vào `~/Downloads/`

> Nếu link trên báo lỗi, thử: **https://www.microsoft.com/software-download/windows11**
> → Scroll xuống phần **"Download Windows 11 Disk Image (ISO) for ARM64 devices"**

---

## Bước 2: Tạo VM trong UTM

Mở UTM:
```bash
open /Applications/UTM.app
```

1. Click **"+"** (Create a New Virtual Machine)
2. Chọn **Virtualize**
3. Chọn **Windows**
4. Click **Browse** → chọn file ISO vừa tải
5. Cấu hình:
   - **Memory**: `8192 MB` (8GB)
   - **CPU Cores**: `4`
   - **Storage**: `60 GB`
7. **Shared Directory**: chọn thư mục project Mac
   - Path: `/Users/hai.phan/Desktop/haiphan/desktop-lamour`
8. Click **Save**

---

## Bước 3: Khởi động và cài Windows 11

Click **Play ▶** để bật VM, rồi làm theo:

1. Chọn ngôn ngữ → **Next** → **Install Now**
2. Bỏ qua product key: **"I don't have a product key"**
3. Chọn **Windows 11 Home** hoặc **Pro**
4. Chọn **Custom Install** → chọn ổ đĩa → **Next**
5. Chờ cài (~15–20 phút, VM tự restart vài lần)
6. Setup tài khoản — **bỏ qua Microsoft account**:
   - Khi bị bắt đăng nhập Microsoft → nhấn `Shift + F10` → gõ:
     ```
     oobe\bypassnro
     ```
   - VM tự restart → chọn **"I don't have internet"** → **"Continue with limited setup"**
   - Đặt tên user và password → **Next**

---

## Bước 4: Cài SPICE Guest Tools (để shared folder hoạt động)

Trong Windows VM, mở **File Explorer** → tìm ổ CD-ROM có tên **SPICE Guest Tools**:

1. Double-click để chạy installer
2. **Next** → **Install** → **Finish**
3. **Restart VM**

Sau khi restart, mở **File Explorer** → **Network** → `\\mac\share` → thấy file Mac.

Map thành ổ đĩa cho tiện:
```
Right-click \\mac\share → Map network drive → chọn Z:
```

---

## Bước 5: Cài .NET 8 SDK trong VM

Mở **PowerShell** (Run as Administrator):

```powershell
# Cài winget nếu chưa có (Windows 11 thường có sẵn)
winget install Microsoft.DotNet.SDK.8

# Kiểm tra
dotnet --version
# Expected: 8.x.x
```

---

## Bước 6: Copy sang C:\ và Run Project WPF

> ⚠️ **KHÔNG** chạy `dotnet run` từ `Z:\` trực tiếp.
> MSBuild không glob `**/*.xaml` qua network drive → lỗi BG1002/BG1003.

Mở **PowerShell Terminal 2** (sync), **Terminal 1** (run):

**Terminal 2 — Sync từ Mac sang VM:**
```powershell
xcopy Z:\ C:\projects\desktop-lamour\ /E /I /Y
```

**Terminal 1 — Chạy từ local:**
```powershell
cd C:\projects\desktop-lamour
dotnet run --project src\DesktopLamour\DesktopLamour.csproj
```

Lần đầu tiên (chưa có `C:\projects\`):
```powershell
mkdir C:\projects
xcopy Z:\ C:\projects\desktop-lamour\ /E /I /Y
cd C:\projects\desktop-lamour
dotnet run --project src\DesktopLamour\DesktopLamour.csproj
```

---

## Bước 7: Fix lỗi ARM64 (nếu có)

Nếu gặp lỗi build liên quan đến platform, sửa file
`src/DesktopLamour/DesktopLamour.csproj`:

```xml
<!-- Trước -->
<Platforms>x64</Platforms>

<!-- Sau -->
<Platforms>x64;ARM64</Platforms>
```

Sau đó build lại:
```powershell
dotnet run --project src\DesktopLamour\DesktopLamour.csproj -r win-arm64
```

---

## Workflow hàng ngày

```
Mac (VS Code)                    Windows VM — Terminal 2      Windows VM — Terminal 1
─────────────────                ───────────────────────      ───────────────────────
Mở UTM → Start VM
Edit .xaml / .cs                 xcopy Z:\ C:\projects\       cd C:\projects\desktop-lamour
Save file               ──────→  desktop-lamour\ /E /I /Y  →  dotnet run --project src\DesktopLamour\...
Sửa tiếp                         xcopy lại                    Ctrl+C → dotnet run lại
```

> **Terminal 1**: chạy WPF app (`dotnet run`)
> **Terminal 2**: sync code từ Mac (`xcopy Z:\`)

---

## Troubleshooting

| Lỗi | Cách fix |
|-----|----------|
| VM quá chậm | Tăng RAM lên 8GB, CPU lên 4 cores trong UTM Settings |
| Shared folder không thấy | Cài lại SPICE Guest Tools, restart VM |
| `dotnet` không tìm thấy | Đóng PowerShell, mở lại sau khi cài SDK |
| WPF không hiện cửa sổ | Chạy lại với `-r win-arm64` |
| Màn hình VM bị nhỏ | Cài SPICE Guest Tools → resolution tự scale |
| BG1002: `**/*.xaml` cannot be found | Đang chạy từ `Z:\` (network drive) → copy sang `C:\projects\` rồi chạy lại |
| BG1003: project file property not valid | Cùng nguyên nhân BG1002 → copy sang `C:\projects\` |

---

## Dừng VM đúng cách

Trong Windows VM:
```
Start → Power → Shut down
```

Hoặc trong UTM: click nút **Stop ■** → **Save State** (để resume nhanh lần sau).
