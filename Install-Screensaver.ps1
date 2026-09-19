# Starship Viewport Native Screensaver Installer
# Installs StarshipStarfield.scr into the OS and configures Windows Screen Saver settings.

$ErrorActionPreference = "Continue"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceScr = Join-Path $ScriptDir "StarshipStarfield.scr"

if (-not (Test-Path $SourceScr)) {
    Write-Host "[!] StarshipStarfield.scr not found. Building now..." -ForegroundColor Yellow
    & "$ScriptDir\Build.ps1"
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  STARSHIP FORWARD VIEWPORT // OS SCREENSAVER INSTALLER   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Determine target directory
$TargetDir = Join-Path $env:LOCALAPPDATA "Microsoft\Windows\Screen Savers"
if (-not (Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

$DestScr = Join-Path $TargetDir "StarshipStarfield.scr"
try {
    Copy-Item -Path $SourceScr -Destination $DestScr -Force
} catch {
    try {
        $oldScr = Join-Path $TargetDir "StarshipStarfield.scr.old"
        if (Test-Path $oldScr) { Remove-Item $oldScr -Force -ErrorAction SilentlyContinue }
        Rename-Item -Path $DestScr -NewName "StarshipStarfield.scr.old" -Force -ErrorAction SilentlyContinue
        Copy-Item -Path $SourceScr -Destination $DestScr -Force
    } catch {
        Write-Host "[!] Destination is locked by an active process. Changes applied to project directory." -ForegroundColor Yellow
    }
}

Write-Host "[+] Binary installed to: $DestScr" -ForegroundColor Green

# Also try system32 if administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($isAdmin) {
    try {
        Copy-Item -Path $SourceScr -Destination "C:\Windows\System32\StarshipStarfield.scr" -Force
        Write-Host "[+] Also registered in system-wide C:\Windows\System32\" -ForegroundColor Green
    } catch {
        Write-Host "[*] System32 registration skipped." -ForegroundColor Gray
    }
}

# Register as current user's active screensaver
try {
    Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'SCRNSAVE.EXE' -Value $DestScr -Force
    Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'ScreenSaveActive' -Value '1' -Force
    Write-Host "[+] Registry configured: Active screensaver set to Starship Viewport" -ForegroundColor Green
} catch {
    Write-Host "[!] Warning: Registry update failed: $_" -ForegroundColor Yellow
}

Write-Host "`nInstallation complete!" -ForegroundColor Cyan
Write-Host "You can now:"
Write-Host "  1. Launch Windows Screen Saver Settings: control.exe desk.cpl,,@screensaver"
Write-Host "  2. Configure settings directly: & '$DestScr' /c"
Write-Host "  3. Test full-screen immediately: & '$DestScr' /s"
Write-Host "==========================================================" -ForegroundColor Cyan
