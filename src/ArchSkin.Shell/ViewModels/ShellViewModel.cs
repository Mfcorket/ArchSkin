using ArchSkin.Core.AppLauncher;
using ArchSkin.Core.NotificationStore;
using ArchSkin.Core.ProcessMonitor;
using ArchSkin.Core.Settings;
using ArchSkin.Core.ShellService;
using ArchSkin.Core.SystemInfo;
using ArchSkin.Core.ThemeRegistry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using System.IO;
using System.Reflection;

namespace ArchSkin.Shell.ViewGroup;

public sealed class ShellViewModel
{
    public AppSettings Settings { get; }
    public SettingsService SettingsService { get; }
    public ThemeRegistryService ThemeRegistry { get; }
    public AppLauncherService Launcher { get; }
    public ProcessMonitor ProcessMonitor { get; }
    public NotificationStore Notifications { get; }
    public SystemInfoProvider SystemInfo { get; }
    public ShellService Shell { get; }

    public event EventHandler<string>? ThemeChanged;

    public ShellViewModel()
    {
        SettingsService = new SettingsService();
        Settings = SettingsService.Load();
        ThemeRegistry = new ThemeRegistryService();
        Launcher = new AppLauncherService(ThemeRegistry);
        ProcessMonitor = new ProcessMonitor();
        Notifications = new NotificationStore();
        SystemInfo = new SystemInfoProvider();
        Shell = new ShellService();
    }

    public void ApplyStartupSettings()
    {
        Shell.SetDesktopIconsVisible(Settings.DesktopIconsVisible);

        if (!string.IsNullOrEmpty(Settings.WallpaperPath) && File.Exists(Settings.WallpaperPath))
        {
            _ = Shell.SetWallpaperAsync(Settings.WallpaperPath);
        }

        ApplyTheme(Settings.ThemeId);
    }

    public void SaveSettings()
    {
        SettingsService.Save(Settings);
    }

    public bool ApplyTheme(string themeId)
    {
        var theme = ThemeRegistry.GetTheme(themeId);
        if (theme == null) return false;

        var resourceName = $"ArchSkin.Assets.{themeId}.{themeId}.xaml";
        var assembly = typeof(ThemeRegistryService).Assembly;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return false;

        using var reader = new StreamReader(stream);
        var xaml = reader.ReadToEnd();

        var newDict = XamlReader.Load(xaml) as ResourceDictionary;
        if (newDict == null) return false;

        var app = Microsoft.UI.Xaml.Application.Current;
        var resources = app.Resources.MergedDictionaries;

        var existingIndex = resources.IndexOf(resources.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains(".xaml")));
        if (existingIndex >= 0)
        {
            resources[existingIndex] = newDict;
        }
        else
        {
            resources.Add(newDict);
        }

        Settings.ThemeId = themeId;
        ThemeChanged?.Invoke(this, themeId);
        return true;
    }
}