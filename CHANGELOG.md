# Changelog

All notable changes to ArchSkin are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added
- **Core Audio volume control** (`AudioVolumeController`) via `IAudioEndpointVolume` COM interop — real master volume and mute, no extra packages
- **DDC/CI brightness control** (`BrightnessController`) with WMI `WmiMonitorBrightness` fallback for laptop panels
- **Multi-monitor support** (`MonitorManager`) — panel, dock, desktop, and overlays now position against the primary monitor's real bounds and DPI scale
- Quick Settings sliders now initialize from and drive real system state
- "Network Settings" button opens `ms-settings:network`
- Close buttons for the notification center and calendar popper
- 10 new tests for system controls and monitor enumeration (42 total)

### Changed
- `SystemControl` is now an aggregate facade over `AudioVolumeController` and `BrightnessController`
- `WindowHelper` exposes DPI-aware `GetPrimaryBounds()` / `GetBoundsForWindow()`
- Repository-wide strict compilation: `TreatWarningsAsErrors=true` via `Directory.Build.props`; build is now **0 warnings, 0 errors**

### Fixed
- Unused `NotificationDismissed` event now has a `Dismiss()` raiser
- `RegistryStore.SetValue` no longer passes null to `RegistryKey.SetValue`
- Service health-check no longer dereferences a possibly-null process handle

## [1.0.0] - 2026-10-25

### Added

**M0 — Proof of Concept**
- Solution scaffold: `ArchSkin.Core`, `ArchSkin.Shell`, `ArchSkin.ThemeSync`, `ArchSkin.Assets`, `ArchSkin.Tests`
- WinUI 3 shell window with top panel and desktop
- Arch package index fetching (`INDEX.xz`) and `.pkg.tar.zst` extraction
- Explorer desktop-icon hiding via `SHChangeNotify`
- Adwaita theme with Arch icon

**M1 — MVP Shell**
- Panel: clock, system tray (battery/network/volume), Activities and Apps buttons
- Dock: pinned apps and running apps (ProcessMonitor)
- Launcher: search + application grid (Start Menu, registry, `.desktop`)
- Notification center with WinRT toast history
- Settings app: Theme, Panel, Dock, Overview, Wallpaper, Updates, About
- Scheduled-task autostart, background service with health check and crash recovery
- Inno Setup installer and PowerShell install/uninstall scripts

**M2 — Full Shell**
- Quick Settings: brightness, volume, network, battery, performance
- Overview with multi-workspace switching and window focus
- Notification action buttons
- Login overlay with password verification (`LogonUser`)
- Wallpaper engine
- Desktop widget layer: clock, system monitor, task list

**M3 — Multi-Theme**
- Breeze (KDE Plasma) theme: colors, icons, XAML resource dictionary
- Xfce theme: colors, icons, XAML resource dictionary
- Embedded-theme discovery in `ThemeRegistryService`
- Theme picker with live preview in Settings
- Runtime theme switching via `ShellViewModel.ApplyTheme`

**M4 — General Availability**
- Update checker (`UpdateService`) with semantic version comparison and check-frequency throttling
- Opt-in local crash reporting (`CrashReporter`)
- Localization infrastructure: English, Simplified Chinese, German
- User manual and FAQ
- Release feed (`release.json`), changelog, and CI build workflow

### Known Limitations
- Panel and dock are placed on the primary display only (per-display panels planned)
- Brightness control requires DDC/CI support or a WMI-capable panel
- Network adapter toggling is a no-op (requires elevation)
- Login overlay is a best-effort overlay; Windows Hello, BitLocker PIN, domain auth, and smart card fall back to native LogonUI

[Unreleased]: https://github.com/archskin/archskin/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/archskin/archskin/releases/tag/v1.0.0
