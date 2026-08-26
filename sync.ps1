$start = Get-Date

Write-Host ""
Write-Host "[1/2] Syncing src\..." -ForegroundColor Cyan
$r1 = robocopy Z:\src C:\projects\desktop-lamour\src /E /PURGE /FFT /MT:8 /NP /NDL /NJH /XF *.user /XD obj bin .git
$copied1  = ($r1 | Select-String "Files\s+:\s+(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -as [int]
$skipped1 = ($r1 | Select-String "Skipped\s+:\s+(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -as [int]
Write-Host "   Copied: $copied1  Skipped: $skipped1" -ForegroundColor Gray

Write-Host "[2/2] Syncing root files..." -ForegroundColor Cyan
$r2 = robocopy Z:\ C:\projects\desktop-lamour\ /LEV:1 /PURGE /FFT /MT:8 /NP /NDL /NJH /XF *.user
$copied2  = ($r2 | Select-String "Files\s+:\s+(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -as [int]
$skipped2 = ($r2 | Select-String "Skipped\s+:\s+(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -as [int]
Write-Host "   Copied: $copied2  Skipped: $skipped2" -ForegroundColor Gray

$elapsed = [math]::Round(((Get-Date) - $start).TotalSeconds, 1)
Write-Host ""
Write-Host "Synced in $($elapsed)s!" -ForegroundColor Green
