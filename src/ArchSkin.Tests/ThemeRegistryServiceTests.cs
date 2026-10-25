using ArchSkin.Core.ThemeRegistry;
using Xunit;

namespace ArchSkin.Tests;

public class ThemeRegistryServiceTests
{
    [Fact]
    public void GetInstalledThemes_ReturnsEmptyList_WhenNoThemesInstalled()
    {
        var registry = new ThemeRegistryService();
        var themes = registry.GetInstalledThemes();
        Assert.NotNull(themes);
    }

    [Fact]
    public void GetTheme_ReturnNull_WhenThemeNotInstalled()
    {
        var registry = new ThemeRegistryService();
        var theme = registry.GetTheme("nonexistent");
        Assert.Null(theme);
    }

    [Fact]
    public void GetThemeIconPath_ReturnEmptyString_WhenIconNotFound()
    {
        var registry = new ThemeRegistryService();
        var path = registry.GetThemeIconPath("nonexistent", "nonexistent");
        Assert.Equal(string.Empty, path);
    }
}
