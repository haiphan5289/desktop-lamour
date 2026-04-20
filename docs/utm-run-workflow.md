# UTM Run Workflow — Chạy WPF trên Windows VM

## ⚠️ Lưu ý quan trọng

**KHÔNG chạy `dotnet run` từ `Z:\` (shared folder).**
MSBuild không thể glob `**/*.xaml` qua network drive → lỗi BG1002/BG1003.

**LUÔN copy project sang ổ C:\ local trước, rồi chạy từ đó.**

---

## Workflow hàng ngày

### Bước 1 — Sync từ Mac sang VM

```powershell
xcopy Z:\ C:\projects\desktop-lamour\ /E /I /Y
```

- `/E` — copy tất cả thư mục kể cả rỗng
- `/I` — tạo thư mục đích nếu chưa có
- `/Y` — overwrite không hỏi

### Bước 2 — Chạy từ local

```powershell
cd C:\projects\desktop-lamour
dotnet run --project src\DesktopLamour\DesktopLamour.csproj
```

---

## Lần đầu tiên (chưa có thư mục C:\projects\)

```powershell
mkdir C:\projects
xcopy Z:\ C:\projects\desktop-lamour\ /E /I /Y
cd C:\projects\desktop-lamour
dotnet run --project src\DesktopLamour\DesktopLamour.csproj
```

---

## Troubleshooting

| Lỗi | Nguyên nhân | Fix |
|---|---|---|
| BG1002: `**/*.xaml` cannot be found | Đang chạy từ Z:\ (network drive) | Copy sang C:\ rồi chạy lại |
| BG1003: project file property not valid | Cùng nguyên nhân trên | Copy sang C:\ rồi chạy lại |
| `cd desktop-lamour` not found | Z:\ đã là root của project rồi | Không cần cd thêm, dùng `xcopy Z:\ ...` trực tiếp |
| WPF không hiện cửa sổ | Platform mismatch | Thêm `-r win-arm64` vào lệnh run |
