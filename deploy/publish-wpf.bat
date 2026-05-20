@echo off
REM Publish WPF desktop app as self-contained Windows x64
REM Run from desktop-lamour project root on a Windows machine
REM Then run installer.iss with Inno Setup to create the installer .exe

SET OUTPUT=.\publish\wpf

echo [1/3] Restoring packages...
dotnet restore src\DesktopLamour

echo [2/3] Publishing self-contained win-x64...
dotnet publish src\DesktopLamour ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=false ^
  -o %OUTPUT%

echo [3/3] Done. Output: %OUTPUT%
echo.
echo NEXT STEP: Open installer.iss with Inno Setup and click Compile
echo Download Inno Setup: https://jrsoftware.org/isdl.php
pause
