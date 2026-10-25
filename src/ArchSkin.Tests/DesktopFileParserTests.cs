using ArchSkin.Core.AppLauncher;
using Xunit;

namespace ArchSkin.Tests;

public class DesktopFileParserTests
{
    [Fact]
    public void Parse_ReturnsNull_ForNonExistentFile()
    {
        var result = DesktopFileParser.Parse("nonexistent.desktop", "adwaita");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ValidDesktopFile_ReturnsEntry()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, @"[Desktop Entry]
Name=TestApp
Comment=A test application
Exec=testapp %u
Icon=testapp
Type=Application
Categories=Utility;");

            var result = DesktopFileParser.Parse(tempFile, "adwaita");
            Assert.NotNull(result);
            Assert.Equal("TestApp", result!.Name);
            Assert.Equal("A test application", result.Comment);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Parse_NonApplicationType_ReturnsNull()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, @"[Desktop Entry]
Name=TestLink
Type=Link");

            var result = DesktopFileParser.Parse(tempFile, "adwaita");
            Assert.Null(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
