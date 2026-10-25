using ArchSkin.Core.Settings;
using Xunit;

namespace ArchSkin.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void Load_ReturnsDefaultSettings_WhenNoFileExists()
    {
        var service = new SettingsService();
        var settings = service.Load();
        Assert.NotNull(settings);
        Assert.Equal("adwaita", settings.ThemeId);
        Assert.Equal(48, settings.DockIconSize);
        Assert.Equal(2, settings.WorkspaceCount);
        Assert.True(settings.ShowBattery);
    }

    [Fact]
    public void Save_And_Load_RoundTrip()
    {
        var service = new SettingsService();
        var original = service.Load();
        original.ThemeId = "papirus";
        original.DockIconSize = 64;
        original.ClockShowSeconds = true;
        service.Save(original);

        var loaded = service.Load();
        Assert.Equal("papirus", loaded.ThemeId);
        Assert.Equal(64, loaded.DockIconSize);
        Assert.True(loaded.ClockShowSeconds);

        // Cleanup
        service.Reset();
    }
}
