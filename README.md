# ArchSkin

> Give your Windows an Arch-grade look.

ArchSkin is a Windows-native desktop shell that visually and behaviorally replicates an Arch Linux desktop experience (GNOME-style by default) using C# + WinUI 3.

## What ArchSkin IS

A Windows-native desktop shell that pulls static assets (icons, themes, CSS, fonts, cursor packs, `.desktop` files) from the Arch Linux package repository as files only (never executed).

## What ArchSkin is NOT

- NOT a Linux DE port. No GNOME Shell, no Plasma, no KWin, no Mutter runs anywhere.
- NOT WSL-dependent. Zero Linux runtime, zero Hyper-V VM, zero WSL2 kernel.
- NOT Wine/QEMU. No emulation of any kind.
- NOT running Linux binaries. The only "Linux" content is parsed data (icons, CSS, fonts, `.desktop` entries) — never executed.
- NOT a terminal emulation. Windows Terminal remains the terminal.

## Features

| Area | Highlights |
|---|---|
| **Panel** | Clock, system tray, Activities, Apps menu, Quick Settings |
| **Quick Settings** | Real brightness (DDC/CI + WMI), volume (Core Audio), network |
| **Dock** | Pinned apps, running apps, app launcher |
| **Launcher** | Search + grid over Start Menu, registry apps, `.desktop` files |
| **Overview** | Multi-workspace switcher and window cards |
| **Notifications** | WinRT toast history with action buttons |
| **Themes** | Adwaita (GNOME), Breeze (KDE Plasma), Xfce — live switching |
| **Desktop** | Wallpaper engine, widget layer (clock, system monitor, tasks) |
| **Displays** | DPI-aware placement on the primary monitor |
| **Login** | Arch/GDM-style overlay with password verification |
| **Settings** | Theme, panel, dock, overview, wallpaper, updates, about |
| **Updates** | Update checker with semantic versioning and throttling |
| **Diagnostics** | Opt-in local crash reporting |
| **Localization** | English, Simplified Chinese, German |

## Code Quality

- `TreatWarningsAsErrors=true` repository-wide — build is **0 warnings, 0 errors**
- 42 xUnit tests covering themes, launcher parsing, settings, updates, localization, system controls, and monitor enumeration

## Project Status

| Milestone | Status |
|---|---|
| M0 — Proof of Concept | ✅ Complete |
| M1 — MVP Shell | ✅ Complete |
| M2 — Full Shell | ✅ Complete |
| M3 — Multi-Theme | ✅ Complete |
| M4 — General Availability | ✅ Complete |

## Build

```powershell
dotnet build src/ArchSkin.slnx -c Debug
```

## Test

```powershell
dotnet test src/ArchSkin.Tests/
```

## Install / Uninstall

```powershell
.\scripts\build.ps1
.\scripts\install.ps1
# ...
.\scripts\uninstall.ps1
```

## Documentation

- [User Manual](docs/USER-MANUAL.md)
- [FAQ](docs/FAQ.md)
- [AI Development Reference](docs/ARCHSKIN-AI.md)
- [Changelog](CHANGELOG.md)

## Project Structure

```
src/
├── ArchSkin.Core/        # Shared library (themes, launcher, hooks, updates, localization)
├── ArchSkin.Shell/       # WinUI 3 shell (panel, dock, overview, launcher, desktop)
├── ArchSkin.Settings/    # WinUI 3 settings app
├── ArchSkin.Login/       # Login overlay
├── ArchSkin.Service/     # Background service (autostart, health check)
├── ArchSkin.ThemeSync/   # CLI theme sync tool
├── ArchSkin.Assets/      # Embedded themes (adwaita, breeze, xfce)
└── ArchSkin.Tests/       # xUnit tests
```

## License

MIT (code). See [ATTRIBUTION.md](ATTRIBUTION.md) for pulled asset licenses.

## Disclaimer

ArchSkin is not affiliated with, endorsed by, or sponsored by Arch Linux. Arch is a registered trademark of its respective owners.
