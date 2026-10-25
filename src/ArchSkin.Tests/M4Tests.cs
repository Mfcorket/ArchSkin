using ArchSkin.Core.Localization;
using ArchSkin.Core.Updates;
using Xunit;

namespace ArchSkin.Tests;

public class UpdateServiceTests
{
    [Theory]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("1.0.1", "1.0.0", 1)]
    [InlineData("1.0.0", "1.0.1", -1)]
    [InlineData("2.0.0", "1.9.9", 1)]
    [InlineData("v1.2.3", "1.2.3", 0)]
    [InlineData("1.2.3-beta", "1.2.3", 0)]
    [InlineData("1.2", "1.2.0", 0)]
    [InlineData("1.10.0", "1.9.0", 1)]
    public void CompareVersions_Works(string a, string b, int expectedSign)
    {
        var result = UpdateService.CompareVersions(a, b);
        Assert.Equal(expectedSign, Math.Sign(result));
    }

    [Fact]
    public void CurrentVersion_IsSet()
    {
        Assert.False(string.IsNullOrWhiteSpace(UpdateService.CurrentVersion));
    }
}

public class LocalizationServiceTests
{
    [Theory]
    [InlineData("en-US", AppLanguage.English)]
    [InlineData("zh-CN", AppLanguage.ChineseSimplified)]
    [InlineData("zh-Hans", AppLanguage.ChineseSimplified)]
    [InlineData("de-DE", AppLanguage.German)]
    [InlineData("fr-FR", AppLanguage.English)]
    [InlineData(null, AppLanguage.English)]
    public void Parse_DetectsLanguage(string? code, AppLanguage expected)
    {
        Assert.Equal(expected, LocalizationService.Parse(code));
    }

    [Fact]
    public void Get_ReturnsLocalizedString()
    {
        LocalizationService.Current = AppLanguage.English;
        Assert.Equal("Activities", LocalizationService.Get("panel.activities"));

        LocalizationService.Current = AppLanguage.ChineseSimplified;
        Assert.Equal("活动", LocalizationService.Get("panel.activities"));

        LocalizationService.Current = AppLanguage.German;
        Assert.Equal("Aktivitäten", LocalizationService.Get("panel.activities"));

        LocalizationService.Current = AppLanguage.English;
    }

    [Fact]
    public void Get_ReturnsKey_WhenMissing()
    {
        Assert.Equal("nonexistent.key", LocalizationService.Get("nonexistent.key"));
    }

    [Fact]
    public void AllLanguages_HaveSameKeys()
    {
        var enKeys = ArchSkin.Core.Localization.TestAccess.EnKeys();
        var zhKeys = ArchSkin.Core.Localization.TestAccess.ZhKeys();
        var deKeys = ArchSkin.Core.Localization.TestAccess.DeKeys();

        Assert.Equal(enKeys.OrderBy(k => k), zhKeys.OrderBy(k => k));
        Assert.Equal(enKeys.OrderBy(k => k), deKeys.OrderBy(k => k));
    }
}
