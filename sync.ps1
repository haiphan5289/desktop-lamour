robocopy Z:\ C:\projects\desktop-lamour\ /E /FFT /MT:8 /XD bin obj .git .vs .claude node_modules /XF *.user /NP /NFL /NDL /NJH
Write-Host "Synced! dotnet watch will auto-reload the app." -ForegroundColor Green
