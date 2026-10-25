using ArchSkin.Core.ThemeRegistry;

namespace ArchSkin.Core.LicenseTracker;

public class LicenseTracker
{
    private readonly ThemeRegistryService _themeRegistry;

    public LicenseTracker(ThemeRegistryService themeRegistry)
    {
        _themeRegistry = themeRegistry;
    }

    public IReadOnlyList<ThemeLicenseInfo> GetAllLicenses()
    {
        var licenses = new List<ThemeLicenseInfo>();
        var themes = _themeRegistry.GetInstalledThemes();

        foreach (var theme in themes)
        {
            licenses.Add(new ThemeLicenseInfo(
                theme.ThemeId,
                theme.SourcePackage,
                theme.License,
                theme.LicenseFile,
                theme.Attribution));
        }

        return licenses;
    }

    public bool VerifyLicenseHeaders(string themeId)
    {
        var theme = _themeRegistry.GetTheme(themeId);
        if (theme == null) return false;

        var licensePath = Path.Combine(ThemeRegistryService.GetAssetsRoot(), themeId, theme.LicenseFile);
        if (!File.Exists(licensePath)) return false;

        var content = File.ReadAllText(licensePath);
        return content.Length > 0;
    }
}

public sealed record ThemeLicenseInfo(
    string ThemeId,
    string SourcePackage,
    string License,
    string LicenseFile,
    string Attribution);
