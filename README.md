# 🚀 Starship Forward Viewport Windows Screensaver

A native Windows screensaver (`StarshipStarfield.scr`) modeling the forward viewport of an interstellar starship traversing deep space, with an authentic bottom-right tactical navigation telemetry HUD displaying velocity in **$\text{km/s}$** and interstellar rate in **$\text{ly/h}$**.

---

## 🌟 Visual & Tactical Features

* **Forward Viewport 3D Starfield**: Perspective projection simulating forward interstellar travel through deep space.
* **Spectral Stellar Classification**: Multi-spectral star distribution based on stellar spectral classes:
  * Class O/B (Deep space ice blue)
  * Class A (Brilliant pure white)
  * Class F (Warm ivory)
  * Class G (Solar gold)
  * Class K/M (Stellar amber)
* **Relativistic Warp Streaks**: Dynamic radial motion streaks calculated using relativistic perspective vectors.
* **Tactical Navigation Telemetry (Bottom-Right HUD)**:
  * Forward velocity in **$\text{km/s}$** (e.g. `2,997,925,000 km/s`)
  * Warp rate in **$\text{ly/h}$** (e.g. `1.155 ly/h`)
  * Status indicator: `SYS: ONLINE | WARP CRUISE ENGAGED`
  * Micro-sensor jitter simulating authentic live telemetry feed
  * Color themes: Tactical Cyan, LCARS Amber, Starlight White, Emerald Green, Crimson Alert
* **Interactive Helm Controls (Settings Dialog)**:
  * Speed / Warp slider with live telemetry preview
  * Presets: Sub-light Impulse (0.25c), Light Speed (1.0c), Warp 3, Warp 5, Warp 8, Deep Warp, Slipstream
  * Star density slider (400 to 4,500 particles)
  * Motion streak multiplier slider
  * HUD theme selector & multi-monitor support
  * One-click "Set Active Screensaver" button

---

## 🛠️ Installation & Usage

### Method 1: Automated Script (Recommended)
Double-click `Install-Screensaver.bat` or execute in PowerShell:
```powershell
.\Install-Screensaver.ps1
```
This automatically registers `StarshipStarfield.scr` in Windows Screen Saver settings.

### Method 2: Native Windows File Explorer
Right-click `StarshipStarfield.scr`:
* **Install**: Automatically sets it as your active screensaver and opens the Windows Screen Saver Settings dialog.
* **Configure**: Opens the Helm Controls / Settings dialog.
* **Test**: Immediately runs full-screen.

### Method 3: Command Line Switches
```cmd
StarshipStarfield.scr /s         :: Full-screen screensaver mode
StarshipStarfield.scr /c         :: Helm Controls / Settings dialog
StarshipStarfield.scr /p <HWND>  :: Diagnostic preview inside parent window handle
```

---

## 📂 Project Structure

* `StarshipStarfield.scr` - Compiled native Windows screensaver executable
* `StarfieldEngine.cs` - 3D relativistic particle engine with spectral shading
* `TelemetryOverlay.cs` - Tactical bottom-right HUD displaying km/s and ly/h
* `SettingsForm.cs` - Interactive Windows configuration dialog (`/c`)
* `ScreenSaverForm.cs` - Borderless full-screen & preview window (`/s`, `/p`)
* `ConfigManager.cs` - Registry persistence (`HKCU\Software\StarshipStarfieldScreensaver`)
* `Build.ps1` - Compilation script using native Windows `csc.exe` (no external SDK required)
* `Install-Screensaver.ps1` - Native OS installation and registration script
* `Uninstall-Screensaver.ps1` - Clean uninstallation script
* `Configure-Screensaver.bat` - Instant launcher for Helm Controls
* `Test-Screensaver.bat` - Instant launcher for full-screen preview
