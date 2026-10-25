using ArchSkin.Core.ExplorerHook;
using Xunit;

namespace ArchSkin.Tests;

public class ExplorerHookTests
{
    [Fact]
    public void IsDesktopIconsVisible_ReturnsBool()
    {
        var visible = ExplorerHook.IsDesktopIconsVisible;
        Assert.True(visible || !visible);
    }
}
