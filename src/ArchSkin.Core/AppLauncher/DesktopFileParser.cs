namespace ArchSkin.Core.AppLauncher;

public static class DesktopFileParser
{
    public static AppEntry? Parse(string path, string themeId)
    {
        if (!File.Exists(path)) return null;

        string? name = null;
        string? comment = null;
        string? exec = null;
        string? icon = null;
        string? workingDir = null;
        string? type = null;
        var categories = new List<string>();

        try
        {
            var inEntry = false;
            foreach (var rawLine in File.ReadLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#') continue;

                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    inEntry = line.Equals("[Desktop Entry]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inEntry) continue;

                var eq = line.IndexOf('=');
                if (eq <= 0) continue;

                var key = line[..eq].Trim();
                var value = line[(eq + 1)..].Trim();

                switch (key)
                {
                    case "Name": name = value; break;
                    case "Comment": comment = value; break;
                    case "Exec": exec = value; break;
                    case "Icon": icon = value; break;
                    case "Path": workingDir = value; break;
                    case "Type": type = value; break;
                    case "Categories":
                        categories.AddRange(value.Split(';', StringSplitOptions.RemoveEmptyEntries));
                        break;
                }
            }
        }
        catch
        {
            return null;
        }

        if (type != null && !type.Equals("Application", StringComparison.OrdinalIgnoreCase)) return null;
        if (string.IsNullOrWhiteSpace(name)) return null;

        var iconPath = string.Empty;
        if (!string.IsNullOrEmpty(icon))
        {
            var registry = new ThemeRegistry.ThemeRegistryService();
            var resolved = registry.GetThemeIconPath(themeId, icon);
            if (resolved.Length > 0) iconPath = resolved;
        }

        var executablePath = ResolveExecutablePath(exec);

        return new AppEntry(
            id: Path.GetFileNameWithoutExtension(path),
            name: name,
            comment: comment,
            iconName: icon,
            iconPath: iconPath,
            exec: exec,
            executablePath: executablePath,
            workingDirectory: workingDir,
            source: AppEntrySource.DesktopFile,
            category: categories.Count > 0 ? categories[0] : "Other");
    }

    public static IReadOnlyList<AppEntry> ParseDirectory(string directory, string themeId)
    {
        var entries = new List<AppEntry>();
        if (!Directory.Exists(directory)) return entries;

        foreach (var file in Directory.GetFiles(directory, "*.desktop", SearchOption.AllDirectories))
        {
            var entry = Parse(file, themeId);
            if (entry != null) entries.Add(entry);
        }

        return entries;
    }

    private static string? ResolveExecutablePath(string? exec)
    {
        if (string.IsNullOrWhiteSpace(exec)) return null;

        var parts = Tokenize(exec);
        if (parts.Count == 0) return null;

        var candidate = parts[0];
        if (candidate.StartsWith('/') || candidate.Contains('\\'))
        {
            return File.Exists(candidate) ? candidate : null;
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;
            var full = Path.Combine(dir, candidate);
            if (File.Exists(full)) return full;
            if (File.Exists(full + ".exe")) return full + ".exe";
        }

        return candidate;
    }

    private static List<string> Tokenize(string command)
    {
        var tokens = new List<string>();
        var sb = new System.Text.StringBuilder();
        var inQuote = false;
        var quoteChar = '\0';

        foreach (var c in command)
        {
            if (inQuote)
            {
                if (c == quoteChar) inQuote = false;
                else sb.Append(c);
                continue;
            }

            if (c == '"' || c == '\'')
            {
                inQuote = true;
                quoteChar = c;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
                continue;
            }

            sb.Append(c);
        }

        if (sb.Length > 0) tokens.Add(sb.ToString());
        return tokens;
    }
}
