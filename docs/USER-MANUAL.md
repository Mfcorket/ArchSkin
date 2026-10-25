# ArchSkin User Manual

> Give your Windows an Arch-grade look.

## 1. What ArchSkin Is

ArchSkin is a **Windows-native desktop shell** that recreates the look and feel of an Arch Linux desktop (GNOME-style by default) using C# and WinUI 3.

It is **not** a Linux port, **not** WSL, and **not** an emulator. It only reads static assets (icons, themes, fonts, `.desktop` files) from Arch Linux packages as data — nothing from Linux is ever executed.

## 2. System Requirements

| Item | Requirement |
|---|---|
| OS | Windows 10 21H2+ or Windows 11 |
| Architecture | x64 (x86 and ARM64 builds available) |
| Runtime | .NET 8+ / Windows App SDK 1.6 runtime |
| Disk | ~150 MB |

## 3. Installation

### Option A — Installer (recommended)

1. Download `ArchSkin-Setup-<version>.exe`.
2. Run it. No administrator rights are required (per-user install).
3. On the last page, keep **Enable autostart** checked.
4. Log out and log back in — the ArchSkin shell starts automatically.

### Option B — From source

```powershell
.\scripts\bootstrap.ps1
.\scripts\build.ps1
.\scripts\install.ps1
```

## 4. Using the Shell

### Top Panel
| Element | Action |
|---|---|
| **Activities** (left) | Open the overview (workspaces + windows) |
| **Apps** (left) | Open the application launcher |
| **Clock** (center) | Open Quick Settings |
| **System tray** (right) | Battery, network, volume indicators |

### Quick Settings
Click the clock to open:
- **Brightness** slider (DDC/CI, WMI fallback)
- **Volume** slider (Windows Core Audio)
- **Wi-Fi** toggle
- **Network Settings** button
- **ArchSkin Settings** button

### Dock (left edge)
- Click a pinned app to launch it.
- Click a running app to bring its window to the front.
- Click the grid icon at the top to open the launcher.

### Launcher
Type to search installed applications (Start Menu shortcuts, registry entries, and `.desktop` files). Click a result to launch it.

### Overview (Activities)
- Left column: workspaces — click to switch.
- Right area: open windows — click to focus.

### Notifications
Click the clock area to toggle the notification center. Notifications with actions show an action button.

### Desktop
- Wallpaper is configurable in Settings.
- Optional widgets: clock, system monitor, task list.

## 5. Settings

Open **Quick Settings → ArchSkin Settings**, or run `ArchSkin.Settings.exe`.

| Section | What you can change |
|---|---|
| **Theme** | Switch between Adwaita (GNOME), Breeze (KDE Plasma), Xfce. Live preview. |
| **Panel** | Clock format, date/seconds, tray icons |
| **Dock** | Icon size, auto-hide, position, show running apps |
| **Overview** | Workspace count, animations |
| **Wallpaper** | Choose an image or reset to default |
| **Updates** | Auto-update, check frequency, crash reporting |
| **About** | Version, license, disclaimer |

## 6. Themes

Three themes ship built-in:

| Theme | Reference | Accent |
|---|---|---|
| `adwaita` | GNOME 46 | `#88c0d0` |
| `breeze` | KDE Plasma 6 | `#3daee9` |
| `xfce` | Xfce 4.18 | `#4a7cbf` |

### Installing more themes

```powershell
archskin install papirus-icon-theme
archskin convert papirus
archskin verify --theme papirus
```

## 7. Uninstalling

### Installer
Use **Settings → Apps → ArchSkin → Uninstall**, or `Uninstall ArchSkin` from the Start Menu.

### Script
```powershell
.\scripts\uninstall.ps1
```

Uninstall restores your original desktop-icon visibility setting and removes the autostart scheduled task.

## 8. Troubleshooting
| Symptom | Fix |
|---|---|
| Shell doesn't start on login | `Get-ScheduledTask -TaskName ArchSkin.AutoStart`; re-run `scripts\install.ps1` |
| Desktop icons still visible | Settings → check `ShowDesktopIcons` registry value is `0` |
| Theme doesn't apply | Re-apply in Settings → Theme; verify `manifest.json` exists |
| Panel not on top | Restart the shell from Task Manager |
| Brightness slider does nothing | Your monitor may not support DDC/CI; laptop panels fall back to WMI |
| Volume slider does nothing | No default audio output device is active |
| Build error `MrtCore.PriGen` | Build with `dotnet build src/ArchSkin.slnx -c Debug` (solution pins platforms) |

Logs: `%LOCALAPPDATA%\ArchSkin\logs\service.log`
Crash reports: `%LOCALAPPDATA%\ArchSkin\crashes\`

## 9. Legal

ArchSkin is licensed under MIT. Pulled assets keep their original licenses — see `ATTRIBUTION.md`.

> ArchSkin is not affiliated with, endorsed by, or sponsored by Arch Linux. Arch is a registered trademark of its respective owners.
