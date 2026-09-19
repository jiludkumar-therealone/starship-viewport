# Starship Viewport Native Screensaver Uninstaller

$ErrorActionPreference = "Continue"

Write-Host "Reverting screensaver configuration..." -ForegroundColor Cyan

try {
    Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'SCRNSAVE.EXE' -Value "" -Force
    Write-Host "[+] Cleared active screensaver in Registry." -ForegroundColor Green
} catch {
    Write-Host "[!] Could not update registry: $_" -ForegroundColor Yellow
}

$localPath = Join-Path $env:LOCALAPPDATA "Microsoft\Windows\Screen Savers\StarshipStarfield.scr"
if (Test-Path $localPath) {
    Remove-Item -Path $localPath -Force
    Write-Host "[+] Removed $localPath" -ForegroundColor Green
}

if (Test-Path "C:\Windows\System32\StarshipStarfield.scr") {
    try {
        Remove-Item -Path "C:\Windows\System32\StarshipStarfield.scr" -Force
        Write-Host "[+] Removed C:\Windows\System32\StarshipStarfield.scr" -ForegroundColor Green
    } catch {}
}

Write-Host "Uninstallation completed." -ForegroundColor Cyan
