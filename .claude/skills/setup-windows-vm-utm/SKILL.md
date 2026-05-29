---
name: setup-windows-vm-utm
description: Guide to setting up a Windows 11 ARM VM on macOS (Apple Silicon) using UTM to build and run WPF desktop-lamour. Use when the user needs to create or troubleshoot a UTM Windows VM, configure shared folders, install .NET SDK, or run dotnet build/run for the WPF project.
model: sonnet
effort: medium
---

# Setup Windows 11 ARM VM via UTM for WPF Development

## Requirements

- Mac Apple Silicon (M1/M2/M3/M4)
- UTM installed at `/Applications/UTM.app`
- ~60 GB free disk space
- 16 GB Mac RAM or more (VM gets 8 GB)

---

## Step 1: Download Windows 11 ARM ISO

Direct Microsoft download page: **https://www.microsoft.com/en-us/software-download/windows11arm64**

- Select **"Windows 11 (multi-edition ISO for ARM64 devices)"**
- Choose language → **Download 64-bit**
- File is ~6 GB — save to `~/Downloads/`

> Fallback: **https://www.microsoft.com/software-download/windows11**
> Scroll to **"Download Windows 11 Disk Image (ISO) for ARM64 devices"**

---

## Step 2: Create the VM in UTM

```bash
open /Applications/UTM.app
```

1. Click **"+"** → **Virtualize** → **Windows**
2. Click **Browse** → select the downloaded ISO
3. Set resources:
   - **Memory**: `8192 MB`
   - **CPU Cores**: `4`
   - **Storage**: `60 GB`
4. **Shared Directory** → select the Mac project path:
   - `/Users/hai.phan/Desktop/haiphan/desktop-lamour`
5. Click **Save**

---

## Step 3: Install Windows 11

Click **Play ▶** in UTM, then follow the installer:

1. Language → **Next** → **Install Now**
2. Skip product key: **"I don't have a product key"**
3. Choose **Windows 11 Home** or **Pro**
4. **Custom Install** → select disk → **Next** (wait ~15–20 min)
5. Bypass Microsoft account:
   - When prompted to sign in → press `Shift + F10` → run:
     ```
     oobe\bypassnro
     ```
   - VM restarts → choose **"I don't have internet"** → **"Continue with limited setup"**
   - Set local username and password

---

## Step 4: Install SPICE Guest Tools (Shared Folder Support)

Inside Windows VM:

1. Open **File Explorer** → find the **SPICE Guest Tools** CD-ROM drive
2. Double-click the installer → **Next** → **Install** → **Finish**
3. **Restart VM**

After restart, verify shared folder:
```
File Explorer → Network → \\mac\share
```

Map as drive Z: for convenience:
```
Right-click \\mac\share → Map network drive → assign Z:
```

---

## Step 5: Install .NET 8 SDK

Open **PowerShell** as Administrator:

```powershell
winget install Microsoft.DotNet.SDK.8

# Verify
dotnet --version
# Expected: 8.x.x
```

---

## Step 6: Copy Project to C:\ and Run

> ⚠️ **KHÔNG** chạy `dotnet run` từ `Z:\` trực tiếp.
> MSBuild không glob `**/*.xaml` qua network drive → lỗi BG1002/BG1003.

**Lần đầu tiên:**
```powershell
mkdir C:\projects
xcopy Z:\ C:\projects\desktop-lamour\ /E /I /Y
cd C:\projects\desktop-lamour
dotnet run --project src\DesktopLamour\DesktopLamour.csproj
```

---

## Step 7: Fix ARM64 Build Errors (if needed)

If build fails with platform error, update `src/DesktopLamour/DesktopLamour.csproj`:

```xml
<!-- Before -->
<Platforms>x64</Platforms>

<!-- After -->
<Platforms>x64;ARM64</Platforms>
```

Then run:
```powershell
dotnet run --project src\DesktopLamour\DesktopLamour.csproj -r win-arm64
```

---

## Daily Workflow (2 Terminals)

```
Mac (VS Code)               UTM — Terminal 2 (sync)       UTM — Terminal 1 (run)
─────────────               ──────────────────────        ──────────────────────
Edit .xaml / .cs  ──────→   .\sync.ps1                →   cd C:\projects\desktop-lamour
Save                        (robocopy Z:\ → C:\)            dotnet run --project src\...
Edit again                  .\sync.ps1 lại                  Ctrl+C → dotnet run lại
```

| Terminal | Lệnh |
|---|---|
| **Terminal 1** | `cd C:\projects\desktop-lamour` → `dotnet run --project src\DesktopLamour\DesktopLamour.csproj` |
| **Terminal 2** | `cd C:\projects\desktop-lamour` → `.\sync.ps1` |

---

## BE API (MacBook)

WPF client gọi BE tại `http://192.168.64.1:5282` (MacBook IP từ UTM).

Trên MacBook, chạy BE trước:
```bash
dotnet run --project src/Lamour.Api
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| VM runs slowly | Set RAM to 8192 MB, CPU to 4 cores in UTM Settings |
| Shared folder not visible | Reinstall SPICE Guest Tools and restart VM |
| `dotnet` command not found | Close PowerShell and reopen after SDK installation |
| WPF window doesn't appear | Run with `-r win-arm64` flag |
| VM display resolution small | Install SPICE Guest Tools — resolution auto-scales |
| BG1002: `**/*.xaml` cannot be found | Running from `Z:\` → copy to `C:\projects\` first |
| BG1003: project file property not valid | Same as BG1002 → copy to `C:\projects\` first |

---

## Shutdown VM

```
Start → Power → Shut down
```

Or in UTM: **Stop ■** → **Save State** to resume quickly next time.

---

## Checklist (Lần đầu Setup)

- [ ] UTM installed at `/Applications/UTM.app`
- [ ] Windows 11 ARM ISO downloaded
- [ ] VM created: 8 GB RAM, 4 CPU cores, 60 GB disk, shared dir set
- [ ] Windows 11 installed (local account)
- [ ] SPICE Guest Tools installed, VM restarted
- [ ] Shared folder visible at `\\mac\share`, mapped to `Z:`
- [ ] .NET 8 SDK installed — `dotnet --version` returns `8.x.x`
- [ ] `xcopy Z:\ C:\projects\desktop-lamour\ /E /I /Y` completed
- [ ] `dotnet run` from `C:\projects\desktop-lamour` opens WPF window
- [ ] BE running on MacBook at `http://0.0.0.0:5282`
