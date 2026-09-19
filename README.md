# 🚀 Starship Forward Viewport Screensaver

[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D4?logo=windows)](https://github.com/jiludkumar-therealone/starship-viewport)
[![Language](https://img.shields.io/badge/language-C%23-239120?logo=csharp)](https://github.com/jiludkumar-therealone/starship-viewport)
[![Framework](https://img.shields.io/badge/dependency-None%20(Native%20Win32)-brightgreen)](https://github.com/jiludkumar-therealone/starship-viewport)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A native Windows screensaver (`.scr`) that transforms your display into the forward viewing screen of an interstellar starship accelerating through deep space. Features true 3D perspective projection, relativistic warp streaks, and an authentic military-grade navigation telemetry HUD in the lower-right sector displaying velocity in **km/s** and interstellar rate in **lightyears/hour**.

---

## 🌌 Viewport Preview

![Starship Forward Viewport Preview](test_viewport_preview.png)

```text
┌─ NAV TELEMETRY // FWD SENSORS ────────────────┐
│ VELOCITY    3,037,087,927 km/s                │
│ RATE        1.156 ly/h                        │
│ SYS: ONLINE | WARP CRUISE ENGAGED             │
└───────────────────────────────────────────────┘
```

---

## ⚡ Key Features

* **3D Perspective Starfield**: Deep space particles radiating forward with realistic depth-attenuated luminance.
* **Hubble/JWST-Grade Procedural Nebulae (NEW in v1.3)**: Volumetric multi-lobed fractal cosmic clouds with dark absorption dust lanes (Bok globules) and astronomical emission spectrums (Hydrogen-Alpha rose, Oxygen-III cyan, Sulfur-II amber, and deep ultraviolet) with 3D parallax drift.
* **Relativistic Star Strikes (NEW in v1.3)**: Radial warp streaks emanating outward from the forward flight tunnel apex with dynamic ionizing tails and blazing stellar cores.
* **Interstellar Cosmic Dust (NEW in v1.3)**: Microscopic cosmic dust motes and micro-debris flying past the viewport with transverse drift.
* **Spectral Stellar Classes**: Multi-spectral star generation across stellar classifications (Class O/B ice blue, Class A pure white, Class F warm ivory, Class G solar gold, Class K amber).
* **Tactical Navigation HUD**:
  * Real-time velocity readout in **km/s** (e.g. `3,036,931,268 km/s`).
  * Relativistic transit rate in **ly/h** (e.g. `1.156 ly/h`).
  * Micro-sensor telemetry jitter simulating active navigation sensors.
* **Helm Controls (Configuration Modal)**:
  * Speed slider with synchronized live calculation of km/s and ly/h.
  * Presets: Sub-light Impulse ($0.3c$), Light Speed ($1.0c$), Warp 3, Warp 5, Warp 8, Deep Warp ($10,000c$), and Slipstream ($50,000c$).
  * Fully persistent preset selection without reverting to defaults.
  * Particle density (400 to 4,500 stars).
  * 5 HUD Themes: Tactical Cyan, LCARS Amber, Starlight White, Emerald Green, Crimson Alert.
  * Multi-monitor span support.
* **Zero Runtime Dependencies**: Compiles to a lightweight ~35 KB binary using built-in Windows Framework components. Runs immediately on any stock Windows 10/11 machine without needing .NET runtimes or external DLLs installed.

---

## 🎛️ Helm Controls Interface

![Screensaver Helm Controls](screensaver_controls_ui.png)

---

## 📥 Installation

### Method 1: 1-Click Install (Pre-compiled Binary)
1. Download or locate `StarshipStarfield.scr`.
2. Right-click `StarshipStarfield.scr` and click **Install**.
3. Windows Screen Saver Settings will open with the screensaver active and previewing in the mini monitor.

### Method 2: Automated PowerShell Script
Run `Install-Screensaver.ps1` or double-click `Install-Screensaver.bat`:
```powershell
.\Install-Screensaver.ps1
```
This registers `StarshipStarfield.scr` into your active user profile under `HKCU\Control Panel\Desktop\SCRNSAVE.EXE`.

### Command Line Switches
```cmd
StarshipStarfield.scr /s         :: Full-screen screensaver mode
StarshipStarfield.scr /c         :: Open Helm Controls / Settings dialog
StarshipStarfield.scr /p <HWND>  :: Render in child preview window
```

---

## 🔨 Building from Source

No Visual Studio or external .NET SDK installation is required—uses the native Windows C# compiler (`csc.exe`):

```powershell
git clone https://github.com/jiludkumar-therealone/starship-viewport.git
cd starship-viewport
.\Build.ps1 -Install
```

---

## 📜 License
MIT License. Free for personal and tactical use across all star systems.
