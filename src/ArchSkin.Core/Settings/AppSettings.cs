namespace ArchSkin.Core.Settings;

public sealed class AppSettings
{
    public string ThemeId { get; set; } = "adwaita";

    public string WallpaperPath { get; set; } = string.Empty;

    public bool DesktopIconsVisible { get; set; }

    public bool LoginOverlayEnabled { get; set; }

    public bool ShowBattery { get; set; } = true;

    public bool ShowNetwork { get; set; } = true;

    public bool ShowVolume { get; set; } = true;

    public string ClockFormat { get; set; } = "HH:mm";

    public bool ClockShowSeconds { get; set; }

    public bool ClockShowDate { get; set; }

    public int DockIconSize { get; set; } = 48;

    public bool DockAutoHide { get; set; }

    public string DockPosition { get; set; } = "Left";

    public bool DockShowRunningApps { get; set; } = true;

    public int WorkspaceCount { get; set; } = 2;

    public bool OverviewAnimations { get; set; } = true;

    public bool AutoUpdate { get; set; }

    public int UpdateCheckFrequencyHours { get; set; } = 24;

    public bool CrashReporting { get; set; }

    public string Language { get; set; } = "en-US";
}
