using Microsoft.Win32;

namespace ArchSkin.Core.AppLauncher;

public class StartMenuProvider
{
    private static readonly string[] StartMenuRoots =
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs")
    };

    public IReadOnlyList<AppEntry> GetStartMenuApps()
    {
        var entries = new List<AppEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in StartMenuRoots)
        {
            if (!Directory.Exists(root)) continue;

            foreach (var file in Directory.GetFiles(root, "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    var entry = ResolveShortcut(file);
                    if (entry == null) continue;
                    if (!seen.Add(entry.Name)) continue;
                    entries.Add(entry);
                }
                catch
                {
                }
            }
        }

        return entries;
    }

    public IReadOnlyList<AppEntry> GetInstalledApps()
    {
        var entries = new List<AppEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roots = new[]
        {
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
            Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
        };

        foreach (var root in roots)
        {
            if (root == null) continue;

            foreach (var subKeyName in root.GetSubKeyNames())
            {
                try
                {
                    using var subKey = root.OpenSubKey(subKeyName);
                    if (subKey == null) continue;

                    var displayName = subKey.GetValue("DisplayName") as string;
                    if (string.IsNullOrWhiteSpace(displayName)) continue;
                    if (!seen.Add(displayName)) continue;

                    var displayIcon = subKey.GetValue("DisplayIcon") as string;
                    var installLocation = subKey.GetValue("InstallLocation") as string;
                    var uninstallString = subKey.GetValue("UninstallString") as string;

                    entries.Add(new AppEntry(
                        id: subKeyName,
                        name: displayName,
                        comment: null,
                        iconName: null,
                        iconPath: ResolveIcon(displayIcon),
                        exec: uninstallString,
                        executablePath: ResolveExecutable(installLocation, displayIcon),
                        workingDirectory: installLocation,
                        source: AppEntrySource.Registry,
                        category: "Installed"));
                }
                catch
                {
                }
            }
        }

        return entries;
    }

    private static AppEntry? ResolveShortcut(string lnkPath)
    {
        if (!ShortcutFileParser.TryParse(lnkPath, out var info)) return null;

        var name = Path.GetFileNameWithoutExtension(lnkPath);

        return new AppEntry(
            id: lnkPath,
            name: name,
            comment: null,
            iconName: null,
            iconPath: info.IconPath,
            exec: info.TargetPath,
            executablePath: File.Exists(info.TargetPath) ? info.TargetPath : null,
            workingDirectory: null,
            source: AppEntrySource.StartMenu,
            category: "Start Menu");
    }

    private static string? ResolveIcon(string? iconLocation)
    {
        if (string.IsNullOrWhiteSpace(iconLocation)) return null;

        var comma = iconLocation.IndexOf(',');
        var path = comma >= 0 ? iconLocation[..comma] : iconLocation;
        path = path.Trim().Trim('"');

        if (File.Exists(path)) return path;
        if (File.Exists(path + ".exe")) return path + ".exe";
        return null;
    }

    private static string? ResolveExecutable(string? installLocation, string? displayIcon)
    {
        if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(displayIcon))
        {
            var comma = displayIcon.IndexOf(',');
            var path = comma >= 0 ? displayIcon[..comma] : displayIcon;
            path = path.Trim().Trim('"');
            if (File.Exists(path)) return path;
        }

        return null;
    }
}
