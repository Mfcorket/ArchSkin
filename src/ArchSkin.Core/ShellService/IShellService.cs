namespace ArchSkin.Core.ShellService;

public enum OverlayType
{
    Launcher,
    Overview,
    NotificationCenter,
    CalendarPopper
}

public interface IShellService
{
    void SetDesktopIconsVisible(bool visible);

    bool IsDesktopIconsVisible { get; }

    Task SetWallpaperAsync(string imagePath);

    void OpenLauncher();

    void OpenOverview();

    void ToggleOverlay(OverlayType type);

    void RegisterHotkey(string name, HotkeyManager.HotkeyCombo combo, Action handler);
}
