using System.Diagnostics;

namespace ArchSkin.Core.AppLauncher;

public class AppLauncherService
{
    private readonly ThemeRegistry.ThemeRegistryService _themeRegistry;
    private readonly StartMenuProvider _startMenuProvider;

    private IReadOnlyList<AppEntry>? _cachedApps;

    public AppLauncherService(ThemeRegistry.ThemeRegistryService themeRegistry)
    {
        _themeRegistry = themeRegistry;
        _startMenuProvider = new StartMenuProvider();
    }

    public IReadOnlyList<AppEntry> GetAllApps(bool refresh = false)
    {
        if (_cachedApps != null && !refresh) return _cachedApps;

        var apps = new List<AppEntry>();

        foreach (var theme in _themeRegistry.GetInstalledThemes())
        {
            var desktopDir = Path.Combine(
                ThemeRegistry.ThemeRegistryService.GetAssetsRoot(), theme.ThemeId, "applications");
            apps.AddRange(DesktopFileParser.ParseDirectory(desktopDir, theme.ThemeId));
        }

        try
        {
            apps.AddRange(_startMenuProvider.GetStartMenuApps());
        }
        catch
        {
        }

        try
        {
            apps.AddRange(_startMenuProvider.GetInstalledApps());
        }
        catch
        {
        }

        var distinct = apps
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(a => a.Name)
            .ToList();

        _cachedApps = distinct;
        return distinct;
    }

    public IReadOnlyList<AppEntry> Search(string query)
    {
        var apps = GetAllApps();
        if (string.IsNullOrWhiteSpace(query)) return apps;

        var q = query.Trim().ToLowerInvariant();
        return apps.Where(a => a.SearchText.Contains(q)).ToList();
    }

    public bool Launch(AppEntry app)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true
            };

            if (!string.IsNullOrEmpty(app.ExecutablePath) && File.Exists(app.ExecutablePath))
            {
                psi.FileName = app.ExecutablePath;
            }
            else if (!string.IsNullOrEmpty(app.Exec))
            {
                var exe = ExtractExecutable(app.Exec);
                psi.FileName = exe;
            }
            else
            {
                return false;
            }

            if (!string.IsNullOrEmpty(app.WorkingDirectory) && Directory.Exists(app.WorkingDirectory))
            {
                psi.WorkingDirectory = app.WorkingDirectory;
            }

            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractExecutable(string exec)
    {
        var trimmed = exec.Trim();
        if (trimmed.StartsWith('"'))
        {
            var end = trimmed.IndexOf('"', 1);
            if (end > 0) return trimmed[1..end];
        }

        var space = trimmed.IndexOf(' ');
        return space > 0 ? trimmed[..space] : trimmed;
    }
}
