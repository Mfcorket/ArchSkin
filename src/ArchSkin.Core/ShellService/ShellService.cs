namespace ArchSkin.Core.ShellService;

public class ShellService : IShellService
{
    public event EventHandler? LauncherOpened;
    public event EventHandler? OverviewOpened;
    public event EventHandler<OverlayType>? OverlayToggled;

    public bool IsDesktopIconsVisible => ArchSkin.Core.ExplorerHook.ExplorerHook.IsDesktopIconsVisible;

    public void SetDesktopIconsVisible(bool visible)
    {
        ArchSkin.Core.ExplorerHook.ExplorerHook.SetDesktopIconsVisible(visible);
    }

    public Task SetWallpaperAsync(string imagePath)
    {
        ArchSkin.Core.ExplorerHook.WallpaperManager.SetWallpaper(imagePath);
        return Task.CompletedTask;
    }

    public void OpenLauncher() => LauncherOpened?.Invoke(this, EventArgs.Empty);

    public void OpenOverview() => OverviewOpened?.Invoke(this, EventArgs.Empty);

    public void ToggleOverlay(OverlayType type) => OverlayToggled?.Invoke(this, type);

    public void RegisterHotkey(string name, HotkeyManager.HotkeyCombo combo, Action handler)
    {
        HotkeyRequested?.Invoke(this, new HotkeyRequestEventArgs(name, combo, handler));
    }

    public event EventHandler<HotkeyRequestEventArgs>? HotkeyRequested;
}

public sealed record HotkeyRequestEventArgs(
    string Name, HotkeyManager.HotkeyCombo Combo, Action Handler);
