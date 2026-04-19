# Session Notes — UTM Windows 11 ARM Setup

## Trạng thái hiện tại
- ✅ ISO downloaded: `26100.4349.250607-1500.ge_release_svc_refresh_CLIENTCONSUMER_RET_A64FRE_en-us.iso`
- ✅ VM tạo xong trong UTM
- ✅ RAM: 8192 MiB, CPU: 4 Cores
- ✅ Shared Directory: `desktop-lamour`
- ✅ NVMe Drive resized: 61440 MB (60 GB)
- ✅ Windows 11 Pro được chọn (bỏ qua product key)
- ✅ Disk 0 Partition 3 (Primary) được chọn để cài
- ⏳ Windows đang cài ngầm — chờ VM tự restart

## Các lưu ý quan trọng từ session

### Boot từ ISO
- Khi thấy **"Press any key to boot from CD or DVD"** → nhấn **Space ngay lập tức** (trong 2 giây), nếu không VM sẽ boot vào UEFI Shell
- Nếu vào UEFI Shell: gõ `FS0:` → `cd EFI\BOOT` → `bootaa64.efi`

### Shared Directory
- **Không** tick "Share is read only" — cần quyền write để build (bin/, obj/)

### Resize Disk
- Vào **Drives → NVMe Drive → Resize** → nhập `61440` (6 chữ số) = 60 GB
- Size hiển thị "536 KB" là bình thường (dynamic allocation, tăng dần khi dùng)
- Size hiển thị "60 TB" trong installer là bình thường (quirk của QEMU)

### Popup "upgrade vs clean install"
- Chọn **Yes** để tiếp tục từ chỗ dở (không phải cài lại từ đầu)
- Chọn **No** nếu muốn clean install hoàn toàn

### Thời gian chờ
- Cài Windows trên QEMU ARM khá chậm: **20–30 phút** tổng
- Màn hình "Start boot option" (UTM logo) = đang cài ngầm, KHÔNG phải stuck
- Chỉ restart nếu sau 30 phút vẫn không có gì

## Bước tiếp theo (sau khi Windows boot lên)
1. Chọn Region, Keyboard (US)
2. Bypass Microsoft account: `Shift + F10` → `oobe\bypassnro` → restart → "I don't have internet"
3. Đặt username + password local
4. Vào Windows → cài **SPICE Guest Tools** từ CD-ROM drive
5. Restart VM
6. Map shared folder: `\\mac\share` → Z:
7. Cài .NET 8 SDK:
   ```powershell
   winget install Microsoft.DotNet.SDK.8
   ```
8. Build WPF:
   ```powershell
   Z:
   cd desktop-lamour
   dotnet run --project src\DesktopLamour\DesktopLamour.csproj
   ```
