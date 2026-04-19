---
name: setup-windows-vm-utm
description: Guide to setting up a Windows 11 ARM VM on macOS (Apple Silicon) using UTM to build and run WPF applications. Use when the user needs to create or troubleshoot a UTM Windows VM for WPF development, configure shared folders, install .NET SDK, or run dotnet build/run from the shared Mac project directory.
model: sonnet
effort: medium
---

# Setup Windows 11 ARM VM via UTM for WPF Development

## Overview

This skill walks through creating a Windows 11 ARM VM on Apple Silicon Mac using UTM, sharing the Mac project folder into the VM, and building/running the WPF app from within Windows.

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

> Fallback URL: **https://www.microsoft.com/software-download/windows11**
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
5. Bypass Microsoft account requirement:
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

After restart, verify shared folder access:
```
File Explorer → Network → \\mac\share
```

Map as a drive letter for convenience:
```
Right-click \\mac\share → Map network drive → assign Z:
```

---

## Step 5: Install .NET 8 SDK

Open **PowerShell** as Administrator in the VM:

```powershell
winget install Microsoft.DotNet.SDK.8

# Verify
dotnet --version
# Expected: 8.x.x
```

---

## Step 6: Build and Run the WPF Project

Open **PowerShell** in the VM:

```powershell
# Navigate to shared project folder
Z:
cd desktop-lamour

# Restore NuGet packages
dotnet restore desktop-lamour.sln

# Build
dotnet build desktop-lamour.sln -c Debug

# Run WPF app
dotnet run --project src\DesktopLamour\DesktopLamour.csproj
```

---

## Step 7: Fix ARM64 Build Errors (if needed)

If the build fails with a platform error, update `src/DesktopLamour/DesktopLamour.csproj`:

```xml
<!-- Before -->
<Platforms>x64</Platforms>

<!-- After -->
<Platforms>x64;ARM64</Platforms>
```

Then run with explicit runtime:
```powershell
dotnet run --project src\DesktopLamour\DesktopLamour.csproj -r win-arm64
```

---

## Daily Workflow

```
Mac (VS Code / Claude Code)        Windows VM (UTM)
──────────────────────────         ────────────────────────
Edit .xaml / .cs files    ──────→  Z:\desktop-lamour (shared)
Save                      ──────→  dotnet run → WPF window appears
Edit again                         Ctrl+C → dotnet run again
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| VM runs slowly | Set RAM to 8192 MB and CPU to 4 cores in UTM Settings |
| Shared folder not visible | Reinstall SPICE Guest Tools and restart VM |
| `dotnet` command not found | Close PowerShell and reopen after SDK installation |
| WPF window doesn't appear | Run with `-r win-arm64` flag |
| VM display resolution small | Install SPICE Guest Tools — resolution auto-scales |

---

## Shutdown VM Correctly

From inside Windows:
```
Start → Power → Shut down
```

Or from UTM: click **Stop ■** → **Save State** to resume quickly next time.

---

## Checklist

- [ ] UTM installed at `/Applications/UTM.app`
- [ ] Windows 11 ARM ISO downloaded
- [ ] VM created with 8 GB RAM, 4 CPU cores, 60 GB disk
- [ ] Shared directory set to Mac project folder
- [ ] Windows 11 installed (local account, no Microsoft account)
- [ ] SPICE Guest Tools installed and VM restarted
- [ ] Shared folder visible at `\\mac\share` (mapped to Z:)
- [ ] .NET 8 SDK installed and `dotnet --version` returns `8.x.x`
- [ ] `dotnet restore` and `dotnet build` succeed
- [ ] `dotnet run` opens WPF window
