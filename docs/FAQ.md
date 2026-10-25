# ArchSkin FAQ

## Is this Linux on Windows?

No. ArchSkin is a **native C# / WinUI 3 application**. It recreates the *appearance* of an Arch Linux desktop. No Linux kernel, no WSL, no virtual machine, no emulation, and no Linux binaries are ever executed.

## Does it use WSL?

No. WSL is explicitly not used anywhere in ArchSkin.

## Will it break my Windows?

ArchSkin only changes:
- Desktop icon visibility (a single registry value, restored on uninstall)
- The wallpaper (optional)
- A per-user autostart scheduled task

Windows Explorer, the Start menu, and file browsing all keep working normally.

## Can I still use the Windows taskbar and Start menu?

Yes. ArchSkin draws its own panel and dock on top, but the Windows taskbar and Start menu remain functional. You can hide the taskbar separately if you prefer.

## How do I get back to normal Windows?

Run the uninstaller, or:
```powershell
.\scripts\uninstall.ps1
```
This restores your desktop icons and removes autostart.

## Where do themes come from?

Built-in themes (Adwaita, Breeze, Xfce) are bundled. Additional themes can be pulled from the Arch Linux package repository as **static files only** — icons, colors, fonts, and `.desktop` entries. No package scripts are ever executed.

## Are the themes safe to redistribute?

Each theme keeps its original license. See `ATTRIBUTION.md`. GPL/LGPL assets are redistributed unmodified, and ArchSkin runs fine without them.

## Why is the panel not showing on top of some apps?

Some full-screen apps (games, video players) request exclusive fullscreen. ArchSkin's panel is always-on-top but cannot overlay exclusive-fullscreen surfaces. Use windowed/borderless mode in those apps.

## The shell didn't start after login.

Check the scheduled task:
```powershell
Get-ScheduledTask -TaskName ArchSkin.AutoStart
```
If missing, re-run `scripts\install.ps1`. Check logs at `%LOCALAPPDATA%\ArchSkin\logs\service.log`.

## How do I report a crash?

Enable **Settings → Updates → Send anonymous crash reports**. Reports are written locally to `%LOCALAPPDATA%\ArchSkin\crashes\`. You can view or clear them from the same page.

## Does it support multiple monitors?

Basic support: the panel and dock are placed on the primary display. Multi-monitor per-display panels are planned for a future release.

## What languages are supported?

English, Simplified Chinese, and German. The language can be set in settings; more languages are planned.

## Is it free?

Yes — ArchSkin is MIT licensed and free to use, modify, and redistribute. Bundled themes retain their own licenses.
