using ArchSkin.Core.SystemControl;
using ArchSkin.Core.Windowing;
using Xunit;

namespace ArchSkin.Tests;

public class SystemControlTests
{
    [Fact]
    public void SystemControl_CanBeConstructedAndDisposed()
    {
        using var control = new SystemControl();
        Assert.NotNull(control.Audio);
        Assert.NotNull(control.Brightness);
    }

    [Fact]
    public void Volume_GetReturnsSaneRange()
    {
        using var audio = new AudioVolumeController();
        var volume = audio.GetVolume();
        Assert.InRange(volume, 0, 100);
    }

    [Fact]
    public void Volume_SetClampsOutOfRange()
    {
        using var audio = new AudioVolumeController();
        // Must not throw for out-of-range input.
        _ = audio.SetVolume(-50);
        _ = audio.SetVolume(500);
        Assert.InRange(audio.GetVolume(), 0, 100);
    }

    [Fact]
    public void Brightness_GetReturnsSaneRange()
    {
        using var brightness = new BrightnessController();
        Assert.InRange(brightness.GetBrightness(), 0, 100);
    }

    [Fact]
    public void Brightness_SetClampsOutOfRange()
    {
        using var brightness = new BrightnessController();
        _ = brightness.SetBrightness(-10);
        _ = brightness.SetBrightness(999);
        Assert.InRange(brightness.GetBrightness(), 0, 100);
    }

    [Fact]
    public void NetworkToggle_IsNoOpButSucceeds()
    {
        using var control = new SystemControl();
        Assert.True(control.SetNetworkEnabled(true));
        Assert.True(control.SetNetworkEnabled(false));
    }
}

public class MonitorManagerTests
{
    [Fact]
    public void GetMonitors_ReturnsAtLeastOneMonitor()
    {
        var monitors = MonitorManager.GetMonitors();
        Assert.NotEmpty(monitors);
    }

    [Fact]
    public void GetPrimaryMonitor_ReturnsMonitor()
    {
        var primary = MonitorManager.GetPrimaryMonitor();
        Assert.NotNull(primary);
        Assert.True(primary!.Width > 0);
        Assert.True(primary.Height > 0);
    }

    [Fact]
    public void MonitorInfo_ComputesRightAndBottom()
    {
        var monitor = new MonitorInfo(IntPtr.Zero, 100, 200, 1920, 1080, true);
        Assert.Equal(2020, monitor.Right);
        Assert.Equal(1280, monitor.Bottom);
    }

    [Fact]
    public void ExactlyOnePrimaryMonitor()
    {
        var primaries = MonitorManager.GetMonitors().Count(m => m.IsPrimary);
        Assert.Equal(1, primaries);
    }
}
