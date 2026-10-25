# ArchSkin - AI Development Reference

> Version 1.0 · Renamed from "Linux DE for Windows (LDE)"
> Scope: Native C# + WinUI 3 rewrite of Arch-inspired DE look on Windows 10/11
> **This is NOT a port. This is NOT WSL. This is NOT running Linux binaries.**

---

## 0. Project Identity (READ FIRST)

### 0.1 What ArchSkin IS
A Windows-native desktop shell that **visually and behaviorally replicates** an Arch Linux desktop experience (primarily GNOME-style) using C# + WinUI 3. It pulls **static assets** (icons, themes, CSS, fonts, cursor packs, `.desktop` files) from the Arch Linux package repository as **files only** (never executed).

### 0.2 What ArchSkin is NOT
- ❌ NOT a Linux DE port. No GNOME Shell, no Plasma, no KWin, no Mutter runs anywhere.
- ❌ NOT WSL-dependent. Zero Linux runtime, zero Hyper-V VM, zero WSL2 kernel.
- ❌ NOT Wine/QEMU. No emulation of any kind.
- ❌ NOT running Linux binaries. The only "Linux" content is parsed data (icons, CSS, fonts, `.desktop` entries) — never executed.
- ❌ NOT a terminal emulation. Windows Terminal remains the terminal.

### 0.3 Hard Constraints (do not violate)
1. **Platform**: Windows 10 21H2+ and Windows 11 (x86_64 only).
2. **Stack**: C# (.NET 8 LTS) + WinUI 3 + Windows App SDK.
3. **Dev platform**: Windows 11.
4. **No Linux runtime**: no WSL, no VM, no container, no chroot.
5. **Keep Explorer browsing**: The Windows Explorer's file browsing must remain functional. Only the *desktop* (icons area) and *taskbar* UI are replaced.
6. **Asset licensing**: All pulled assets must retain their original license headers and be bundled with an `ATTRIBUTION.md`.

### 0.4 Marketing Positioning
- User-facing name: **ArchSkin**
- Tagline: *"Give your Windows an Arch-grade look."*
- Never claim "Linux DE on Windows" or "port Linux DE" — claim "native rewrite inspired by Arch".

---

## 1. Reference DE & Aesthetic

### 1.1 Primary Reference
**GNOME 46 on Arch Linux** — the most iconic "Linux desktop" look. Specifically:

| Element | Reference | Notes |
|---|---|---|
| Top panel | GNOME Shell top bar | Clock in center, menus left/right |
| Activities | GNOME Shell "Activities" overview | Workspace + window cards |
| Dock | Ubuntu/Pop!_OS dash | Left-side dock (GNOME doesn't have one natively, but it's an Arch-common extension) |
| Launcher | GNOME "Activities" → app grid | Full-screen grid with search |
| Notifications | GNOME Shell notification center | Top-right dropdown |
| Clock/calendar | GNOME Shell date/time popper | Calendar in top-center dropdown |
| Settings | GNOME Control Center | Windows-style settings but with GNOME layout |
| Login | GDM | User avatar + password field |
| Desktop | No icons (GNOME philosophy) | Replace Explorer desktop |
| File browser | Nautilus | **NOT replaced** — Explorer stays |

### 1.2 Alternate Themes (Phase 2+)
- **KDE Plasma** (look): Plasma-style taskbar, KWin-style overview
- **Xfce** (look): Xfce panel + Whiskermenu launcher
- Each theme is a set of XAML resource dictionaries + asset folder.

### 1.3 Asset Source
Arch package repository:
- INDEX: `https://archlinux.org/official/x86_64/INDEX.xz`
- Package mirror: `https://archlinux.org/official/x86_64/<pkgname>-<ver>-<epoch>-x86_64.pkg.tar.zst`
- Primary theme packages:
  - `gnome-themes-extra`
  - `papirus-icon-theme`
  - `adwaita-icon-theme`
  - `material-icon-theme`
  - `numix-themes-arch` (community repo)

---

## 2. Architecture Overview

```
┌────────────────────────────────────────────────────────────┐
│                     ArchSkin System                         │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────┐    ┌─────────────────────────┐   │
│  │  ArchSkin.Shell     │    │  ArchSkin.Login         │   │
│  │  (WinUI 3 App)      │    │  (LogonUI Overlay)      │   │
│  │  ─────────────      │    │  ─────────────          │   │
│  │  • Panel (top bar)  │    │  • User list            │   │
│  │  • Dock             │    │  • Password input       │   │
│  │  • Launcher         │    │  • Wallpaper + branding │   │
│  │  • Overview         │    │  • Credential pass-thru │   │
│  │  • Notifications    │    └─────────────────────────┘   │
│  │  • Clock/calendar   │                                   │
│  │  • System tray      │    ┌─────────────────────────┐   │
│  └──────────┬──────────┘    │  ArchSkin.Desktop       │   │
│             │               │  (Desktop Replacement)  │   │
│             ▼               │  ─────────────          │   │
│  ┌─────────────────────┐    │  • Wallpaper engine     │   │
│  │  ArchSkin.Core      │    │  • Icon layer           │   │
│  │  (Shared libs)      │    │  • Widget layer         │   │
│  │  ─────────────      │    └─────────────────────────┘   │
│  │  • Theme registry   │                                   │
│  │  • Asset loader     │    ┌─────────────────────────┐   │
│  │  • Tray bridge      │    │  ArchSkin.Service       │   │
│  │  • Notify bridge    │    │  (Background Service)   │   │
│  │  • Explorer hooks   │    │  ─────────────          │   │
│  └─────────────────────┘    │  • Hide Explorer desktop│   │
│                              │  • Manage autostart     │   │
│  ┌─────────────────────┐    │  • Update checker       │   │
│  │  ArchSkin.ThemeSync │    └─────────────────────────┘   │
│  │  (CLI Tool)         │                                   │
│  │  ─────────────      │    ┌─────────────────────────┐   │
│  │  • Fetch INDEX.xz   │    │  ArchSkin.Settings      │   │
│  │  • Download pkg     │    │  (Settings UI)          │   │
│  │  • Extract assets   │    │  ─────────────          │   │
│  │  • Convert CSS      │    │  • Theme picker         │   │
│  │  • Install fonts    │    │  • Wallpaper picker     │   │
│  └─────────────────────┘    │  • Behavior toggles     │   │
│                              └─────────────────────────┘   │
│  ┌─────────────────────┐                                   │
│  │  ArchSkin.Assets    │  (Bundled default themes)        │
│  │  (NuGet/embedded)   │                                   │
│  └─────────────────────┘                                   │
│                                                             │
└────────────────────────────────────────────────────────────┘
                          │
                          ▼
        ┌───────────────────────────────────┐
        │       Windows 10/11 Shell         │
        │  ─────────────────────────        │
        │  • Taskbar (hidden by ArchSkin)   │
        │  • Explorer (desktop hidden)      │
        │  • Start menu (still functional)  │
        │  • LogonUI (overlayed by Login)   │
        │  • WinRT Toast API                │
        │  • Shell hooks (SHChangeNotify)   │
        └───────────────────────────────────┘
```

### 2.1 Component Responsibilities

| Component | Type | Lifecycle | Persists |
|---|---|---|---|
| `ArchSkin.Shell` | WinUI 3 desktop app | Runs after user login | Per-session |
| `ArchSkin.Login` | WinUI 3 overlay app | Runs at boot, before user login | One-shot |
| `ArchSkin.Desktop` | WinUI 3 background | Runs after login | Per-session |
| `ArchSkin.Service` | Background app (scheduled task) | Runs at boot | Always |
| `ArchSkin.ThemeSync` | CLI tool | On-demand | State in registry |
| `ArchSkin.Settings` | WinUI 3 app | On-demand | Per-session |
| `ArchSkin.Core` | Shared .NET library | Library | N/A |
| `ArchSkin.Assets` | Embedded resource pack | Library | N/A |

---

## 3. Technology Stack

### 3.1 Mandatory

| Layer | Choice | Version |
|---|---|---|
| Language | C# | 12 (`.NET 8 LTS`) |
| UI framework | WinUI 3 | 1.6+ |
| OS SDK | Windows App SDK | 1.6+ |
| Runtime | Windows 10 21H2+ / Win 11 | x86_64 |
| Package mgmt | NuGet | — |
| Build | `dotnet build` | .NET 8 SDK |
| Installer | Inno Setup | 6.x |

### 3.2 Key NuGet Packages
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.x" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.26100.x" />
<PackageReference Include="CommunityToolkit.WinUI.UI" Version="8.x" />
<PackageReference Include="CommunityToolkit.WinUI.Controls" Version="8.x" />
<PackageReference Include="CommunityToolkit.WinUI.Animations" Version="8.x" />
<PackageReference Include="System.Text.Json" Version="8.x" />
<PackageReference Include="SharpZipLib" Version="1.x" />   <!-- .tar handling -->
<PackageReference Include="ZstdSharp.Port" Version="8.x" /> <!-- .zst handling -->
<PackageReference Include="Sandbox" Version="1.x" />        <!-- sandbox .desktop parse -->
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.x" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.x" />
```

### 3.3 Key Windows APIs
- **WinUI 3**: `Microsoft.UI.Xaml.*`
- **WinRT Toast**: `Windows.UI.Notifications.*`
- **Shell hooks**: P/Invoke `user32.dll`, `shell32.dll`, `explorerframe.dll`
- **Desktop hiding**: Registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced` → `ShowDesktopIcons=0`, then `SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, ...)`
- **App model**: `Windows.ApplicationModel.*`
- **Scheduled tasks**: `Microsoft.Win32.TaskScheduler` NuGet
- **Background task**: WinUI background activation

### 3.4 Forbidden
- ❌ `System.Diagnostics.Process.Start("wsl ...")` — never call WSL
- ❌ `libuv`, `libcurl`, native libs requiring Linux
- ❌ Any dependency on `\\wsl$` UNC path

---

## 4. Component Specifications

### 4.1 ArchSkin.Shell (Main UI)

**Purpose**: The always-visible desktop chrome. Replaces Windows taskbar and start menu visually.

**Entry point**: `ArchSkin.Shell.App.xaml.cs` → `MainWindow.xaml`

**Windows**:
1. `Panel` (top bar): Always-on-top, no-border, spans full screen width.
2. `Dock` (left side): Floating window with running apps.
3. `Overview` (Activities): Full-screen overlay, toggled by Win-Tab or click "Activities".
4. `NotificationCenter`: Top-right flyout.
5. `CalendarPopper`: Center dropdown.

**XAML Pages**:
```
ArchSkin.Shell/
├── App.xaml
├── MainWindow.xaml              # Hosts Panel + Dock
├── Views/
│   ├── PanelView.xaml
│   ├── DockView.xaml
│   ├── OverviewView.xaml
│   ├── LauncherView.xaml
│   ├── NotificationCenterView.xaml
│   ├── CalendarPopperView.xaml
│   └── DesktopView.xaml
├── Controls/
│   ├── ClockBadge.xaml
│   ├── RunningAppTile.xaml
│   ├── NotificationCard.xaml
│   └── WorkspacePreview.xaml
├── ViewModels/
│   ├── ShellViewModel.cs
│   ├── DockViewModel.cs
│   └── OverviewViewModel.cs
└── Models/
    ├── RunningApp.cs
    └── NotificationItem.cs
```

**Behavior contracts**:
- Must launch within 500ms of user login (cold start).
- Must not consume > 100MB RAM steady-state.
- Must handle Win+L (lock), Win+D (show desktop), Win+Tab (overview).
- Must not block Start menu (Win key) — only visually hide taskbar.

### 4.2 ArchSkin.Login

**Purpose**: Replace Windows LogonUI with an Arch/GDM-style login screen.

**Mechanism**: Overlay window positioned over `LogonUI.exe`. Detect `LogonUI.exe` start via WMI process events; show overlay within 100ms.

**Constraints**:
- Cannot truly disable LogonUI (requires admin + signing).
- Must pass through: Windows Hello, BitLocker PIN, domain auth, smart card.
- If any of these are active, hide ArchSkin.Login and let LogonUI show.

**Flow**:
```
Boot → Windows pre-login → LogonUI.exe starts
   ↓ (WMI detection)
ArchSkin.Login launches, shows overlay on top
   ↓
User clicks username → password field appears
   ↓
User types password → overlay hides → LogonUI receives input
```

**Fallback**: If WMI/overlay fails, just don't show. Windows logs in normally.

### 4.3 ArchSkin.Desktop

**Purpose**: Hide Explorer desktop icons, show custom wallpaper and optional widget layer.

**Mechanism**:
1. Read registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowDesktopIcons`.
2. If `1`, set to `0`.
3. Call `SHChangeNotify` to refresh.
4. Restore original value on ArchSkin uninstall.

**Wallpaper**: Use `SystemParametersInfo(SPI_SETDESKWALLPAPER, ..., SPUPDATE|SPIF_UPDATEHICON, path)`.

**Widget layer** (optional): Floating widgets (clock, system monitor, todo). Phase 2+.

### 4.4 ArchSkin.ThemeSync

**Purpose**: CLI tool that fetches themes/icons from Arch repo and prepares them for the shell.

**Commands**:
```
archskin sync [--theme <name>] [--mirror <url>] [--offline]
archskin list                          # list available themes
archskin install <theme>               # install a theme
archskin remove <theme>                # remove a theme
archskin update                        # check for updates
archskin verify [--theme <name>]       # verify license headers intact
```

**Implementation pipeline**:
1. Download `INDEX.xz` from mirror.
2. Parse INDEX (XZ + CSV format).
3. For each requested theme package, download `.pkg.tar.zst`.
4. Extract `.pkg.tar.zst` (zstd → tar).
5. Extract asset subdirectories:
   - `/usr/share/icons/<theme>/` → icon packs
   - `/usr/share/themes/<theme>/` → GTK/Qt themes
   - `/usr/share/cursors/<theme>/` → cursor packs
   - `/usr/share/fonts/<theme>/` → fonts
   - `/usr/share/applications/*.desktop` → app entries
   - `/usr/share/gnome-shell/theme/` → GNOME Shell CSS
6. Save raw assets to `%LOCALAPPDATA%\ArchSkin\assets\<theme>\`.
7. **Do NOT run any scripts or executables** from the package.
8. Run `archskin convert` to generate WinUI 3 resource dictionaries.
9. Write manifest with license info.

**Security**:
- All `.tar` contents are read-only; no `chmod +x`.
- No `PKGBUILD` execution.
- No `.sh` script execution.
- All extracted files are treated as data.

**Manifest format** (`manifest.json`):
```json
{
  "themeId": "papirus-dark",
  "sourcePackage": "papirus-icon-theme",
  "sourceVersion": "2024.06.15",
  "sourceUrl": "https://archlinux.org/packages/extra/x86_64/papirus-icon-theme/",
  "license": "MIT",
  "licenseFile": "LICENSE",
  "attribution": "https://github.com/PapirusDE/Papirus-Icon",
  "assets": {
    "icons": ["hicolor", "actions", "apps", "devices", "emblems"],
    "cursors": ["*"]
  }
}
```

### 4.5 ArchSkin.Service

**Purpose**: Background service for lifecycle management.

**Responsibilities**:
- Hide Explorer desktop at boot (before user login).
- Autostart ArchSkin.Shell after user login.
- Health check: restart Shell if it crashes.
- Update checker (periodic, not aggressive).
- Registry rollback on uninstall.

**Implementation**:
- Option A: Windows Service (`sc create`, requires admin).
- Option B: Scheduled Task triggered at `SystemStart` and `Logon`. **Recommended** — no admin required.
- Option C: Registry `Run` key (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).

**Decision**: Use **Scheduled Task** (per-user, no admin). Task name: `ArchSkin.AutoStart`.

### 4.6 ArchSkin.Settings

**Purpose**: Configuration UI for the user.

**Sections**:
1. **Theme** — pick from installed themes, preview, apply.
2. **Wallpaper** — system wallpaper or custom.
3. **Panel** — clock position, battery %, system tray icons.
4. **Dock** — icon size, auto-hide, position.
5. **Overview** — workspace count, animations.
6. **Login** — enable/disable overlay, wallpaper, branding.
7. **Desktop** — icons on/off, widgets.
8. **Updates** — auto-update, theme update frequency.
9. **About** — version, credits, license info.

### 4.7 ArchSkin.Core

**Purpose**: Shared library, no UI.

**Namespaces**:
- `ArchSkin.Core.ThemeRegistry` — discovers installed themes.
- `ArchSkin.Core.AssetLoader` — loads icons, cursors, fonts by ID.
- `ArchSkin.Core.TrayBridge` — WinRT Toast ↔ shell bridge.
- `ArchSkin.Core.NotificationBridge` — Windows notifications to shell notification center.
- `ArchSkin.Core.ExplorerHook` — desktop icon hiding, wallpaper setting.
- `ArchSkin.Core.ProcessMonitor` — detect running apps for dock.
- `ArchSkin.Core.HotkeyManager` — global hotkeys (Win+Tab, Win+Space, etc.).
- `ArchSkin.Core.RegistryStore` — typed registry access.
- `ArchSkin.Core.LicenseTracker` — tracks attribution for each theme.

### 4.8 ArchSkin.Assets

**Purpose**: Default bundled assets so the app works offline on first install.

**Contents** (per theme):
```
assets/
├── adwaita/
│   ├── icons/
│   ├── cursor/
│   ├── fonts/
│   ├── panel.css        # source CSS (for reference)
│   └── shell.xaml       # converted WinUI 3 dictionary
├── papirus/
└── ...
```

Bundled via `<EmbeddedResource>` in `ArchSkin.Core.csproj`.

---

## 5. Public APIs

### 5.1 ThemeSync API

```csharp
namespace ArchSkin.Core.ThemeSync;

public interface IThemeSyncClient
{
    // Fetch available themes from mirror
    Task<IReadOnlyList<ThemePackageInfo>> ListAvailableAsync(
        Uri mirror = default, CancellationToken ct = default);

    // Download and install a theme
    Task<InstallResult> InstallAsync(
        string packageName, Uri mirror = default, CancellationToken ct = default);

    // Remove installed theme (assets + manifest + converted xaml)
    Task RemoveAsync(string themeId);

    // Convert installed assets to WinUI 3 resource dictionary
    Task<ConvertResult> ConvertToWinUiAsync(string themeId);

    // Verify license headers intact (read-only check)
    Task<VerifyResult> VerifyAsync(string themeId);
}

public sealed record ThemePackageInfo(
    string PackageName, string Version, string Description, string License);

public sealed record InstallResult(
    bool Success, string ThemeId, string Message, string[]? Warnings);
```

### 5.2 Shell Service API

```csharp
namespace ArchSkin.Core;

public interface IShellService
{
    // Hide/show Explorer desktop icons
    void SetDesktopIconsVisible(bool visible);
    bool IsDesktopIconsVisible { get; }

    // Set wallpaper
    Task SetWallpaperAsync(string imagePath);

    // Open launcher (Win+Space or Start button)
    void OpenLauncher();

    // Open overview (Win+Tab)
    void OpenOverview();

    // Toggle a specific overlay
    void ToggleOverlay(OverlayType type);

    // Register global hotkey
    void RegisterHotkey(string name, HotkeyCombo combo, Action handler);
}

public enum OverlayType { Launcher, Overview, NotificationCenter, CalendarPopper }
```

### 5.3 Notification Bridge API

```csharp
namespace ArchSkin.Core;

public interface INotificationBridge
{
    // Subscribe to Windows Toast notifications
    void Subscribe();
    void Unsubscribe();

    // Show a notification from shell (to WinRT Toast)
    Task ShowNotificationAsync(string appName, string title, string body, string? actionUrl = null);

    // Notification events
    event EventHandler<NotificationEventArgs> NotificationReceived;
    event EventHandler<NotificationEventArgs> NotificationDismissed;
}

public sealed record NotificationEventArgs(
    string Id, string AppName, string Title, string Body, DateTime Timestamp, string? ActionUrl);
```

### 5.4 Hotkey Manager API

```csharp
namespace ArchSkin.Core;

public interface IHotkeyManager
{
    void Register(string name, HotkeyCombo combo, Action handler);
    void Unregister(string name);
    bool IsRegistered(string name);
}

public sealed record HotkeyCombo(bool Ctrl, bool Alt, bool Shift, bool Win, Key Key);
```

---

## 6. Data Flow

### 6.1 Theme Installation Flow

```
User runs `archskin install papirus-icon-theme`
   ↓
CLI downloads INDEX.xz from https://archlinux.org/official/x86_64/
   ↓
CLI parses INDEX, finds papirus-icon-theme-<ver>-<epoch>-x86_64.pkg.tar.zst
   ↓
CLI downloads the package (read-only)
   ↓
CLI extracts to %LOCALAPPDATA%\ArchSkin\staging\papirus\
   ↓ (NO execution of any script)
CLI runs `archskin convert papirus`
   ↓
Convert reads /usr/share/icons/papirus/, generates WinUI 3 XAML dictionary
   ↓
Convert writes to %LOCALAPPDATA%\ArchSkin\assets\papirus\
   ↓
Convert writes manifest.json with license info
   ↓
Shell reloads theme registry → user sees new theme in Settings
```

### 6.2 Shell Boot Flow

```
User logs in
   ↓
Scheduled Task triggers ArchSkin.Service
   ↓
Service checks registry: is desktop hidden? yes.
Service launches ArchSkin.Shell (WinUI 3 app)
   ↓
Shell starts in 500ms
   ↓
Shell loads: PanelView, DockView, DesktopView
   ↓
Shell reads %LOCALAPPDATA%\ArchSkin\manifest.json → finds installed themes
   ↓
Shell applies default theme (or last-used)
   ↓
Shell registers global hotkeys
   ↓
Shell subscribes to Toast notifications
   ↓
Shell starts ProcessMonitor for dock
   ↓
User can now use their "Linux" desktop
```

### 6.3 Login Flow

```
Boot → Windows pre-login → LogonUI.exe starts
   ↓
ArchSkin.Service (already running) detects via WMI
   ↓
Service checks: is Login overlay enabled in settings?
   ↓ (yes)
Service checks: is user using Hello/BitLocker/domain?
   ↓ (no — local password)
Service launches ArchSkin.Login (WinUI 3 app)
   ↓
Login app shows overlay on top of LogonUI
   ↓
User clicks username → Login shows password field
   ↓
User types password → Login forwards to LogonUI (via UI Automation or focus pass)
   ↓
LogonUI authenticates → user logs in
   ↓
Login app exits, Shell launches
```

---

## 7. Project Structure

```
A:\Now\ArchSkin\
├── docs/
│   ├── ARCHSKIN-AI.md            # This file (AI reference)
│   ├── ArchSkin_开发文档.docx     # Human-readable dev guide
│   ├── ARCHITECTURE.md           # Detailed architecture (Phase 2)
│   └── LEGAL.md                  # Licensing guide
│
├── src/
│   ├── ArchSkin.sln
│   ├── ArchSkin.Shell/           # WinUI 3 main app
│   │   ├── ArchSkin.Shell.csproj
│   │   ├── Package.appxmanifest
│   │   ├── App.xaml(.cs)
│   │   ├── MainWindow.xaml(.cs)
│   │   ├── Views/
│   │   ├── Controls/
│   │   ├── ViewModels/
│   │   ├── Models/
│   │   └── Assets/               # App-specific images
│   ├── ArchSkin.Login/           # WinUI 3 login overlay
│   ├── ArchSkin.Service/         # Background service (WinUI background)
│   ├── ArchSkin.ThemeSync/       # CLI tool (console .NET)
│   ├── ArchSkin.Settings/        # WinUI 3 settings app
│   ├── ArchSkin.Core/            # Shared class library
│   │   ├── ThemeSync/
│   │   ├── TrayBridge/
│   │   ├── NotificationBridge/
│   │   ├── ExplorerHook/
│   │   ├── ProcessMonitor/
│   │   ├── HotkeyManager/
│   │   ├── RegistryStore/
│   │   └── LicenseTracker/
│   ├── ArchSkin.Assets/          # Embedded default themes
│   └── ArchSkin.Tests/           # xUnit tests
│
├── installer/
│   └── setup.iss                 # Inno Setup script
│
├── scripts/
│   ├── bootstrap.ps1             # Dev setup
│   ├── sync-themes.ps1           # One-shot theme sync
│   ├── build.ps1                 # CI build
│   └── test.ps1                  # Run tests
│
├── .github/
│   └── workflows/
│       ├── build.yml
│       └── release.yml
│
├── .gitignore
├── .editorconfig
├── README.md
├── LICENSE                        # ArchSkin code: MIT
├── ATTRIBUTION.md                 # Pulled asset attributions
└── CHANGELOG.md
```

### 7.1 Naming Conventions
- **Solution**: `ArchSkin.sln`
- **Projects**: `ArchSkin.<Component>`
- **Namespaces**: `ArchSkin.<Component>.<SubComponent>`
- **XAML**: PascalCase, `.xaml` + `.xaml.cs`
- **ViewModels**: `*ViewModel` (MVVM, no code-behind for logic)
- **Models**: `*` (POCOs)
- **Tests**: `*Test.cs` in `ArchSkin.Tests`

---

## 8. Build & Test

### 8.1 Dev Setup (first time)
```powershell
# Prereqs
- Visual Studio 2022 (Community)
- .NET 8 SDK
- Windows App SDK workload
- Node.js (for theme tooling if needed)
- Git

# Bootstrap
.\scripts\bootstrap.ps1

# Open solution
code ArchSkin.sln
```

### 8.2 Build Commands
```powershell
# Debug build
dotnet build src/ArchSkin.sln -c Debug

# Release build
dotnet build src/ArchSkin.sln -c Release

# Pack NuGet (Assets library)
dotnet pack src/ArchSkin.Assets/ -c Release

# Publish Shell
dotnet publish src/ArchSkin.Shell/ -c Release -r win-x64 --self-contained

# Build installer
"isetup installer\setup.iss"
```

### 8.3 Test Commands
```powershell
# Run all tests
dotnet test src/ArchSkin.Tests/

# Run with coverage
dotnet test src/ArchSkin.Tests/ --collect:"XPlat Code Coverage"

# Lint (if Roslyn analyzers configured)
dotnet build src/ArchSkin.sln -warnaserror
```

### 8.4 CI/CD
GitHub Actions:
- On PR: build + test
- On tag: publish + installer + upload release

---

## 9. Milestones

### M0: PoC (2-4 weeks)
**Goal**: Prove the basic architecture works.

Tasks:
- [ ] Solution scaffold (`ArchSkin.sln`, empty projects)
- [ ] `ArchSkin.Shell` boots and shows a blank panel
- [ ] `ArchSkin.ThemeSync` fetches `papirus-icon-theme` from Arch, extracts to local cache
- [ ] Convert 1 icon set to WinUI resource
- [ ] `ArchSkin.Core` ExplorerHook hides desktop icons
- [ ] Verify: boot ArchSkin, see blank panel + Arch icon on a button

Exit criteria: A working demo where user sees an Arch icon rendered via WinUI, with desktop icons hidden.

### M1: MVP Shell (2-3 months, after M0)
**Goal**: Functional desktop shell.

Tasks:
- [ ] Panel with clock, system tray, app menu
- [ ] Dock showing running apps (ProcessMonitor)
- [ ] Basic launcher (search + grid)
- [ ] Notification center (read-only display of WinRT Toast)
- [ ] Settings app (theme picker, panel toggles)
- [ ] Scheduled task autostart
- [ ] Installer (Inno Setup)
- [ ] Uninstaller (rollback registry, remove scheduled task)

Exit criteria: User installs, logs out, logs in, sees ArchSkin shell with functional panel + dock + launcher. Can launch apps from launcher.

### M2: Full Shell (2-3 months, after M1)
**Goal**: Full GNOME-like experience.

Tasks:
- [ ] Overview (Activities) with workspace switcher
- [ ] Multi-workspace support
- [ ] Notification action buttons
- [ ] Calendar popper
- [ ] Quick settings (brightness, volume, network)
- [ ] Wallpaper engine
- [ ] Desktop widget layer
- [ ] Login overlay (with fallback)

Exit criteria: User can switch workspaces, receive and dismiss notifications, change wallpaper, log in via overlay.

### M3: Multi-Theme (1-2 months, after M2)
**Goal**: KDE, Xfce themes.

Tasks:
- [ ] KDE Plasma theme (XAML + assets)
- [ ] Xfce theme (XAML + assets)
- [ ] Theme switcher in Settings
- [ ] Theme preview in Settings

Exit criteria: User can switch between GNOME/KDE/Xfce looks in real-time.

### M4: GA (2-3 months, after M3)
**Goal**: Public release.

Tasks:
- [ ] Update checker + auto-update
- [ ] Crash reporting (opt-in)
- [ ] Documentation (user manual, FAQ)
- [ ] Localization (English, Chinese, German)
- [ ] Beta program
- [ ] Store release (Microsoft Store + direct download)

Exit criteria: 1.0 release, ~1000 beta users, no critical bugs.

**Total: ~10-14 months solo, or 6-8 months with 2 devs.**

---

## 10. Risks & Mitigations

| Risk | Severity | Mitigation |
|---|---|---|
| Windows 11 update breaks Explorer hook | High | Test on 23H2, 24H2, Insider; maintain registry fallbacks |
| LogonUI overlay blocked by MS | High | Fallback: don't show; user uses native login |
| EDR/AV false positive | Medium | Code sign cert, submit to MS Defender, document behavior |
| GPL asset redistribution | High | ATTRIBUTION.md, keep license headers, no modification |
| Performance on low-end | Medium | Lazy-load assets, limit animation frames |
| User confusion ("is this Linux?") | Low | Clear marketing, README states "native rewrite" |
| Theme sync breaks if Arch changes URL | Medium | Mirror fallback, offline mode with cached assets |
| WinUI 3 runtime missing on Win10 | Medium | Bundle runtime or check before install |
| Hotkey conflicts | Medium | Settings UI for hotkey reassignment |
| Single dev burnout | Medium | Modular design allows outside contributors |

---

## 11. Legal & Licensing

### 11.1 ArchSkin Code
- License: **MIT** (permissive, allows commercial use).
- File: `LICENSE` at repo root.
- All new code: MIT.

### 11.2 Pulled Assets
Each theme package has its own license. Common ones:
- `papirus-icon-theme` → MIT
- `adwaita-icon-theme` → LGPL 2.1+
- `gnome-themes-extra` → LGPL 2.1+
- `breeze-icons` → GPL-2.0+
- `numix-themes-arch` → CC-BY-SA-3.0

**Rules**:
1. Preserve license headers in all extracted files.
2. Do NOT modify GPL/LGPL code (only redistribute).
3. Keep `ATTRIBUTION.md` at repo root with all licenses + URLs.
4. Bundle `LICENSE.txt` from each source package alongside assets.
5. For GPL assets: ensure ArchSkin can run **without** those assets (so ArchSkin itself isn't linked to GPL code).

### 11.3 Trademarks
- "Arch" and the Arch logo are trademarks of Arch Linux project. ArchSkin must include disclaimer:
  > "ArchSkin is not affiliated with, endorsed by, or sponsored by Arch Linux. Arch is a registered trademark of its respective owners."
- Same for GNOME, KDE, Ubuntu trademarks.

### 11.4 End-User Agreement
- Standard MIT-style EULA.
- No warranty, no liability (standard).

---

## 12. Glossary

| Term | Meaning |
|---|---|
| **DE** | Desktop Environment (e.g., GNOME, KDE, Xfce) |
| **WM** | Window Manager (e.g., Mutter, KWin) |
| **Panel** | The top or bottom bar (GNOME top bar, KDE taskbar) |
| **Dock** | Floating app launcher (Ubuntu dash, Pop!_OS dock) |
| **Shell** | The desktop UI layer (GNOME Shell, Plasma Shell) |
| **Overview** | Workspace switcher + window cards (GNOME Activities) |
| **Compositor** | Window manager that handles transparency, shadows (Mutter, KWin) |
| **Wallpaper engine** | Background image manager |
| **System tray** | Right side of taskbar with indicators (battery, network, etc.) |
| **Toast** | Windows notification API (WinRT) |
| **WSL** | Windows Subsystem for Linux — **NOT USED** in ArchSkin |
| **WSLg** | WSL Graphics — **NOT USED** in ArchSkin |
| **PKG** | Arch package file (`.pkg.tar.zst`) |
| **PKGBUILD** | Arch package build script — **NEVER EXECUTED** by ThemeSync |
| **XAML** | WinUI 3 UI markup (like XML) |
| **MVVM** | Model-View-ViewModel pattern |
| **Scheduled Task** | Windows Task Scheduler entry |
| **DWM** | Desktop Window Manager (Windows' built-in compositor) |

---

## 13. Quick-Start for AI Agents

If you're picking up ArchSkin development as an AI agent, do these first:

1. **Read this document top to bottom**. Do not skip section 0 (Identity).
2. **Verify you understand the NOT list** in 0.2. Never propose WSL or running Linux binaries.
3. **Clone / open** `A:\Now\ArchSkin\`.
4. **If `src/` doesn't exist**, run M0:
   - `dotnet new sln -n ArchSkin`
   - Create projects per section 7.
5. **For any code change**, run `dotnet build src/ArchSkin.sln` first.
6. **For any theme sync change**, test with `archskin sync --theme papirus --offline` (cached).
7. **For any UI change**, test on Windows 11 24H2 (latest stable).
8. **Never commit**:
   - Secrets / API keys
   - Modified GPL asset files (only add, never edit)
   - `bin/`, `obj/`, `build/` directories
9. **For new features**, update section 9 (Milestones) with a checkmark.
10. **Before releasing**, run `archskin verify` to ensure all licenses intact.

---

## 14. Appendix: Common Tasks

### A.1 Add a new theme
1. Fetch assets: `archskin install <package-name>`
2. Convert: `archskin convert <theme-id>`
3. Create XAML dictionary in `ArchSkin.Assets/<theme>/shell.xaml`
4. Update `ArchSkin.Core.ThemeRegistry` to discover it
5. Add entry to `ATTRIBUTION.md`
6. Update `docs/ArchSkin_开发文档.docx` if user-facing

### A.2 Add a new panel button
1. Create XAML control in `ArchSkin.Shell/Controls/`
2. Add ViewModel in `ArchSkin.Shell/ViewModels/`
3. Wire to `PanelView.xaml`
4. Test in Overview and standalone
5. Add to `ArchSkin.Settings` for user config

### A.3 Add a new notification action
1. Define action in `ArchSkin.Core.NotificationBridge`
2. Add XAML button in `NotificationCard.xaml`
3. Wire to handler in `NotificationCenterViewModel`
4. Test with a sample toast

### A.4 Diagnose Explorer hook failure
1. Check registry: `Get-ItemProperty HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`
2. Verify `ShowDesktopIcons` is set to `0`
3. Run: `(New-Object -ComObject Shell.Application).Explore` to refresh
4. Check scheduled task: `Get-ScheduledTask -TaskName ArchSkin.AutoStart`
5. Check service logs: `%LOCALAPPDATA%\ArchSkin\logs\service.log`

---

*End of AI Reference. Keep this file in sync with the codebase as it evolves.*
