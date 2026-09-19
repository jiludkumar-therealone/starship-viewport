# Build script for Starship Viewport Screensaver
param (
    [switch]$Install
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

Write-Host "Compiling StarshipStarfield.scr with $csc..." -ForegroundColor Cyan

$sources = @(
    "Program.cs",
    "ConfigManager.cs",
    "StarfieldEngine.cs",
    "TelemetryOverlay.cs",
    "SettingsForm.cs",
    "ScreenSaverForm.cs"
)

$cmdArgs = @(
    "/target:winexe",
    "/optimize+",
    "/platform:anycpu",
    "/win32manifest:app.manifest",
    "/win32icon:starship.ico",
    "/out:StarshipStarfield.scr"
) + $sources + @(
    "/r:System.dll,System.Drawing.dll,System.Windows.Forms.dll"
)

& $csc $cmdArgs

if ($LASTEXITCODE -eq 0) {
    Copy-Item "StarshipStarfield.scr" "StarshipStarfield.exe" -Force
    Write-Host "[OK] Successfully built StarshipStarfield.scr and StarshipStarfield.exe" -ForegroundColor Green
    
    if ($Install) {
        & "$ScriptDir\Install-Screensaver.ps1"
    }
} else {
    Write-Error "Build failed with exit code $LASTEXITCODE"
}
