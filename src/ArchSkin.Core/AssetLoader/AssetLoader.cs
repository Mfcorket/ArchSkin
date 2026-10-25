using ArchSkin.Core.ThemeRegistry;

namespace ArchSkin.Core.AssetLoader;

public class AssetLoader
{
    private readonly ThemeRegistryService _themeRegistry;

    public AssetLoader(ThemeRegistryService themeRegistry)
    {
        _themeRegistry = themeRegistry;
    }

    public string LoadIcon(string themeId, string iconName)
    {
        return _themeRegistry.GetThemeIconPath(themeId, iconName);
    }

    public IReadOnlyList<string> LoadThemeIcons(string themeId)
    {
        var icons = new List<string>();
        var themeDir = Path.Combine(ThemeRegistryService.GetAssetsRoot(), themeId, "icons");
        if (!Directory.Exists(themeDir)) return icons;

        foreach (var file in Directory.GetFiles(themeDir, "*.*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".svg" or ".png" or ".ico")
            {
                icons.Add(file);
            }
        }

        return icons;
    }

    public string? LoadFontPath(string themeId, string fontName)
    {
        var fontsDir = Path.Combine(ThemeRegistryService.GetAssetsRoot(), themeId, "fonts");
        if (!Directory.Exists(fontsDir)) return null;

        var extensions = new[] { ".ttf", ".otf", ".woff", ".woff2" };
        foreach (var ext in extensions)
        {
            var path = Path.Combine(fontsDir, fontName + ext);
            if (File.Exists(path)) return path;
        }

        return null;
    }
}
