using ArchSkin.Core.AppLauncher;
using Xunit;

namespace ArchSkin.Tests;

public class ShortcutFileParserTests
{
    [Fact]
    public void TryParse_ReturnsFalse_ForNonExistentFile()
    {
        var result = ShortcutFileParser.TryParse("nonexistent.lnk", out _);
        Assert.False(result);
    }
}
