using System.Reflection;
using System.Text.Json;

namespace ArchSkin.Core.ThemeRegistry;

public sealed record ThemeManifest(
    string ThemeId,
    string SourcePackage,
    string SourceVersion,
    string SourceUrl,
    string License,
    string LicenseFile,
    string Attribution,
    ThemeAssets Assets);

public sealed record ThemeAssets(
    string[] Icons,
    string[] Cursors,
    string[] Fonts,
    string[] DesktopEntries);

public class ThemeRegistryService
{
    private static readonly string AssetsRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ArchSkin", "assets");

    public static string GetAssetsRoot() => AssetsRoot;

    public IReadOnlyList<ThemeManifest> GetInstalledThemes()
    {
        var themes = new List<ThemeManifest>();

        themes.AddRange(GetEmbeddedThemes());

        if (!Directory.Exists(AssetsRoot)) return themes;

        foreach (var dir in Directory.GetDirectories(AssetsRoot))
        {
            var manifestPath = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            try
            {
                var json = File.ReadAllText(manifestPath);
                var manifest = JsonSerializer.Deserialize<ThemeManifest>(json);
                if (manifest != null) themes.Add(manifest);
            }
            catch
            {
            }
        }

        return themes;
    }

    public ThemeManifest? GetTheme(string themeId)
    {
        var embedded = GetEmbeddedTheme(themeId);
        if (embedded != null) return embedded;

        var manifestPath = Path.Combine(AssetsRoot, themeId, "manifest.json");
        if (!File.Exists(manifestPath)) return null;

        try
        {
            var json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<ThemeManifest>(json);
        }
        catch
        {
            return null;
        }
    }

    public string GetThemeIconPath(string themeId, string iconName)
    {
        var themeDir = Path.Combine(AssetsRoot, themeId, "icons");
        var extensions = new[] { ".svg", ".png", ".ico" };

        foreach (var ext in extensions)
        {
            var path = Path.Combine(themeDir, iconName + ext);
            if (File.Exists(path)) return path;
        }

        foreach (var ext in extensions)
        {
            var path = Path.Combine(themeDir, "hicolor", "scalable", "apps", iconName + ext);
            if (File.Exists(path)) return path;
        }

        var embedded = GetEmbeddedIconPath(themeId, iconName);
        if (!string.IsNullOrEmpty(embedded)) return embedded;

        return string.Empty;
    }

    public Stream? GetEmbeddedThemeResource(string themeId, string resourcePath)
    {
        var assembly = typeof(ThemeRegistryService).Assembly;
        var resourceName = $"ArchSkin.Assets.{themeId}.{resourcePath}";
        return assembly.GetManifestResourceStream(resourceName);
    }

    private IReadOnlyList<ThemeManifest> GetEmbeddedThemes()
    {
        var themes = new List<ThemeManifest>();
        var assembly = typeof(ThemeRegistryService).Assembly;

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (resourceName.StartsWith("ArchSkin.Assets.") && resourceName.EndsWith("manifest.json"))
            {
                var parts = resourceName.Split('.');
                if (parts.Length >= 4)
                {
                    var themeId = parts[2];
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var json = reader.ReadToEnd();
                        var manifest = JsonSerializer.Deserialize<ThemeManifest>(json);
                        if (manifest != null) themes.Add(manifest);
                    }
                }
            }
        }

        return themes;
    }

    private ThemeManifest? GetEmbeddedTheme(string themeId)
    {
        var assembly = typeof(ThemeRegistryService).Assembly;
        var resourceName = $"ArchSkin.Assets.{themeId}.manifest.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<ThemeManifest>(json);
    }

    private string? GetEmbeddedIconPath(string themeId, string iconName)
    {
        var assembly = typeof(ThemeRegistryService).Assembly;
        var extensions = new[] { ".svg", ".png", ".ico" };

        foreach (var ext in extensions)
        {
            var resourceName = $"ArchSkin.Assets.{themeId}.icons.hicolor.scalable.apps.{iconName}{ext}";
            if (assembly.GetManifestResourceInfo(resourceName) != null)
                return resourceName;
        }

        return null;
    }
}