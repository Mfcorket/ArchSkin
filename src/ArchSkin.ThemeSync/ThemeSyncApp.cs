using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using ArchSkin.Core.ThemeRegistry;
using ArchSkin.Core.LicenseTracker;
using ICSharpCode.SharpZipLib.Tar;
using ZstdSharp;

namespace ArchSkin.ThemeSync;

public class ThemeSyncApp
{
    private const string ArchIndexUrl = "https://archlinux.org/official/x86_64/INDEX.xz";
    private const string ArchPackageBase = "https://archlinux.org/official/x86_64/";

    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string ArchSkinRoot = Path.Combine(LocalAppData, "ArchSkin");
    private static readonly string StagingRoot = Path.Combine(ArchSkinRoot, "staging");
    private static readonly string AssetsRoot = Path.Combine(ArchSkinRoot, "assets");

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].ToLowerInvariant();

        try
        {
            return command switch
            {
                "sync" => await SyncAsync(args),
                "list" => ListThemes(),
                "install" => await InstallAsync(args),
                "remove" => RemoveTheme(args),
                "update" => await UpdateAsync(),
                "verify" => VerifyTheme(args),
                "convert" => await ConvertAsync(args),
                _ => PrintUsage()
            };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static int PrintUsage()
    {
        Console.WriteLine("ArchSync - ArchSkin Theme Synchronization Tool");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  archskin sync [--theme <name>] [--mirror <url>] [--offline]");
        Console.WriteLine("  archskin list");
        Console.WriteLine("  archskin install <theme>");
        Console.WriteLine("  archskin remove <theme>");
        Console.WriteLine("  archskin update");
        Console.WriteLine("  archskin verify [--theme <name>]");
        Console.WriteLine("  archskin convert <theme-id>");
        return 1;
    }

    private async Task<int> SyncAsync(string[] args)
    {
        var theme = GetArgumentValue(args, "--theme");
        var mirror = GetArgumentValue(args, "--mirror") ?? ArchIndexUrl;
        var offline = args.Contains("--offline");

        if (offline)
        {
            Console.WriteLine("Offline mode: using cached data only.");
            return 0;
        }

        Console.WriteLine($"Fetching theme index from {mirror}...");

        Directory.CreateDirectory(StagingRoot);
        Directory.CreateDirectory(AssetsRoot);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "ArchSkin-ThemeSync/1.0");

        var indexBytes = await http.GetByteArrayAsync(mirror);
        var indexText = await DecompressXzAsync(indexBytes);

        var packages = ParseIndex(indexText);
        Console.WriteLine($"Found {packages.Count} packages in index.");

        if (!string.IsNullOrEmpty(theme))
        {
            var matching = packages.Where(p =>
                p.PackageName.Contains(theme, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var pkg in matching)
            {
                Console.WriteLine($"  {pkg.PackageName} ({pkg.Version}) - {pkg.Description}");
            }

            if (matching.Count == 0)
            {
                Console.WriteLine($"No packages matching '{theme}' found.");
                return 1;
            }
        }
        else
        {
            var themePackages = packages.Where(p =>
                p.PackageName.Contains("icon-theme") ||
                p.PackageName.Contains("themes-extra") ||
                p.PackageName.Contains("cursor")).ToList();

            Console.WriteLine($"Found {themePackages.Count} theme-related packages:");
            foreach (var pkg in themePackages.Take(20))
            {
                Console.WriteLine($"  {pkg.PackageName} ({pkg.Version})");
            }
        }

        return 0;
    }

    private int ListThemes()
    {
        var registry = new ThemeRegistryService();
        var themes = registry.GetInstalledThemes();

        if (themes.Count == 0)
        {
            Console.WriteLine("No themes installed.");
            Console.WriteLine("Use 'archskin install <package>' to install a theme.");
            return 0;
        }

        Console.WriteLine($"Installed themes ({themes.Count}):");
        foreach (var theme in themes)
        {
            Console.WriteLine($"  {theme.ThemeId}");
            Console.WriteLine($"    Source: {theme.SourcePackage} {theme.SourceVersion}");
            Console.WriteLine($"    License: {theme.License}");
            Console.WriteLine($"    URL: {theme.SourceUrl}");
            Console.WriteLine();
        }

        return 0;
    }

    private async Task<int> InstallAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: archskin install <package-name>");
            return 1;
        }

        var packageName = args[1];
        Console.WriteLine($"Installing {packageName}...");

        Directory.CreateDirectory(StagingRoot);
        Directory.CreateDirectory(AssetsRoot);

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "ArchSkin-ThemeSync/1.0");

        var indexBytes = await http.GetByteArrayAsync(ArchIndexUrl);
        var indexText = await DecompressXzAsync(indexBytes);
        var packages = ParseIndex(indexText);

        var pkg = packages.FirstOrDefault(p => p.PackageName == packageName);
        if (pkg == null)
        {
            Console.WriteLine($"Package '{packageName}' not found in index.");
            return 1;
        }

        Console.WriteLine($"Found {pkg.PackageName} ({pkg.Version})");
        Console.WriteLine($"  {pkg.Description}");
        Console.WriteLine($"  License: {pkg.License}");

        var pkgFileName = $"{pkg.PackageName}-{pkg.Version}-{pkg.Epoch}-x86_64.pkg.tar.zst";
        var pkgUrl = ArchPackageBase + pkgFileName;
        var stagingDir = Path.Combine(StagingRoot, pkg.PackageName);

        Console.WriteLine($"Downloading {pkgUrl}...");
        var pkgBytes = await http.GetByteArrayAsync(pkgUrl);

        Directory.CreateDirectory(stagingDir);
        var pkgPath = Path.Combine(stagingDir, pkgFileName);
        await File.WriteAllBytesAsync(pkgPath, pkgBytes);

        Console.WriteLine("Extracting...");
        ExtractZstTar(pkgPath, stagingDir);

        var themeId = pkg.PackageName.Replace("-theme", "").Replace("-icon", "");
        var targetDir = Path.Combine(AssetsRoot, themeId);
        Directory.CreateDirectory(targetDir);

        CopyAssets(stagingDir, targetDir);

        var manifest = new ThemeManifest(
            themeId,
            pkg.PackageName,
            pkg.Version,
            $"https://archlinux.org/packages/any/x86_64/{pkg.PackageName}/",
            pkg.License,
            "LICENSE",
            $"https://archlinux.org/packages/any/x86_64/{pkg.PackageName}/",
            new ThemeAssets(
                Directory.Exists(Path.Combine(targetDir, "icons")) ? new[] { "hicolor" } : Array.Empty<string>(),
                Directory.Exists(Path.Combine(targetDir, "cursors")) ? new[] { "*" } : Array.Empty<string>(),
                Directory.Exists(Path.Combine(targetDir, "fonts")) ? new[] { "*" } : Array.Empty<string>(),
                Directory.Exists(Path.Combine(targetDir, "applications")) ? new[] { "*" } : Array.Empty<string>()));

        var manifestPath = Path.Combine(targetDir, "manifest.json");
        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"Theme installed: {themeId}");
        Console.WriteLine($"  Location: {targetDir}");
        Console.WriteLine($"  Manifest: {manifestPath}");

        return 0;
    }

    private int RemoveTheme(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: archskin remove <theme-id>");
            return 1;
        }

        var themeId = args[1];
        var targetDir = Path.Combine(AssetsRoot, themeId);

        if (!Directory.Exists(targetDir))
        {
            Console.WriteLine($"Theme '{themeId}' not found.");
            return 1;
        }

        Directory.Delete(targetDir, recursive: true);
        Console.WriteLine($"Theme '{themeId}' removed.");
        return 0;
    }

    private async Task<int> UpdateAsync()
    {
        Console.WriteLine("Checking for theme updates...");
        var registry = new ThemeRegistryService();
        var themes = registry.GetInstalledThemes();

        if (themes.Count == 0)
        {
            Console.WriteLine("No themes installed.");
            return 0;
        }

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "ArchSkin-ThemeSync/1.0");

        var indexBytes = await http.GetByteArrayAsync(ArchIndexUrl);
        var indexText = await DecompressXzAsync(indexBytes);
        var packages = ParseIndex(indexText);

        foreach (var theme in themes)
        {
            var pkg = packages.FirstOrDefault(p => p.PackageName == theme.SourcePackage);
            if (pkg == null) continue;

            if (pkg.Version != theme.SourceVersion)
            {
                Console.WriteLine($"Update available: {theme.ThemeId} ({theme.SourceVersion} -> {pkg.Version})");
            }
            else
            {
                Console.WriteLine($"Up to date: {theme.ThemeId} ({theme.SourceVersion})");
            }
        }

        return 0;
    }

    private int VerifyTheme(string[] args)
    {
        var theme = GetArgumentValue(args, "--theme");

        if (!string.IsNullOrEmpty(theme))
        {
            return VerifySingleTheme(theme) ? 0 : 1;
        }

        var registry = new ThemeRegistryService();
        var themes = registry.GetInstalledThemes();
        var allValid = true;

        foreach (var t in themes)
        {
            if (!VerifySingleTheme(t.ThemeId))
            {
                allValid = false;
            }
        }

        return allValid ? 0 : 1;
    }

    private bool VerifySingleTheme(string themeId)
    {
        var themeDir = Path.Combine(AssetsRoot, themeId);
        var manifestPath = Path.Combine(themeDir, "manifest.json");

        if (!File.Exists(manifestPath))
        {
            Console.WriteLine($"FAIL: {themeId} - manifest.json not found");
            return false;
        }

        var licensePath = Path.Combine(themeDir, "LICENSE");
        if (!File.Exists(licensePath))
        {
            Console.WriteLine($"FAIL: {themeId} - LICENSE not found");
            return false;
        }

        Console.WriteLine($"OK: {themeId}");
        return true;
    }

    private async Task<int> ConvertAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: archskin convert <theme-id>");
            return 1;
        }

        var themeId = args[1];
        var themeDir = Path.Combine(AssetsRoot, themeId);

        if (!Directory.Exists(themeDir))
        {
            Console.WriteLine($"Theme '{themeId}' not found.");
            return 1;
        }

        Console.WriteLine($"Converting theme '{themeId}' to WinUI 3 resources...");

        var xamlPath = Path.Combine(themeDir, "shell.xaml");
        var xaml = GenerateWinUiXaml(themeId);
        await File.WriteAllTextAsync(xamlPath, xaml);

        Console.WriteLine($"Generated: {xamlPath}");
        return 0;
    }

    private static string GenerateWinUiXaml(string themeId)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ResourceDictionary
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">

    <!-- ArchSkin Theme: {themeId} -->
    <!-- Auto-generated by archskin convert -->

    <Color x:Key=""PanelBackground"">#2e3440</Color>
    <Color x:Key=""PanelForeground"">#eceff4</Color>
    <Color x:Key=""DesktopBackground"">#3b4252</Color>
    <Color x:Key=""AccentColor"">#88c0d0</Color>

    <SolidColorBrush x:Key=""PanelBackgroundBrush"" Color=""{{StaticResource PanelBackground}}""/>
    <SolidColorBrush x:Key=""PanelForegroundBrush"" Color=""{{StaticResource PanelForeground}}""/>
    <SolidColorBrush x:Key=""DesktopBackgroundBrush"" Color=""{{StaticResource DesktopBackground}}""/>
    <SolidColorBrush x:Key=""AccentBrush"" Color=""{{StaticResource AccentColor}}""/>

</ResourceDictionary>";
    }

    private static List<ArchPackage> ParseIndex(string indexText)
    {
        var packages = new List<ArchPackage>();
        var blocks = indexText.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 3) continue;

            var pkg = new ArchPackage();
            foreach (var line in lines)
            {
                if (line.StartsWith("%NAME%")) pkg.PackageName = line[6..];
                else if (line.StartsWith("%VERSION%")) pkg.Version = line[9..];
                else if (line.StartsWith("%EPOCH%")) pkg.Epoch = line[7..];
                else if (line.StartsWith("%DESC%")) pkg.Description = line[6..];
                else if (line.StartsWith("%LICENSE%")) pkg.License = line[9..];
            }

            if (!string.IsNullOrEmpty(pkg.PackageName))
            {
                packages.Add(pkg);
            }
        }

        return packages;
    }

    private static async Task<string> DecompressXzAsync(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var output = new MemoryStream();

        var decoder = new Decompressor();
        using (var xzStream = new DecompressionStream(input, decoder))
        {
            await xzStream.CopyToAsync(output);
        }

        return System.Text.Encoding.UTF8.GetString(output.ToArray());
    }

    private static void ExtractZstTar(string zstTarPath, string outputDir)
    {
        using var fileStream = File.OpenRead(zstTarPath);
        var zstdDecompressor = new Decompressor();
        using var zstdStream = new DecompressionStream(fileStream, zstdDecompressor);
        using var tarInput = new TarInputStream(zstdStream, System.Text.Encoding.UTF8);

        TarEntry? entry;
        while ((entry = tarInput.GetNextEntry()) != null)
        {
            if (entry.IsDirectory) continue;

            var entryPath = Path.Combine(outputDir, entry.Name);
            var entryDir = Path.GetDirectoryName(entryPath);
            if (!string.IsNullOrEmpty(entryDir))
            {
                Directory.CreateDirectory(entryDir);
            }

            using var entryStream = File.Create(entryPath);
            var buffer = new byte[81920];
            int read;
            while ((read = tarInput.Read(buffer, 0, buffer.Length)) > 0)
            {
                entryStream.Write(buffer, 0, read);
            }
        }
    }

    private static void CopyAssets(string sourceDir, string targetDir)
    {
        var assetDirs = new[]
        {
            Path.Combine("usr", "share", "icons"),
            Path.Combine("usr", "share", "themes"),
            Path.Combine("usr", "share", "cursors"),
            Path.Combine("usr", "share", "fonts"),
            Path.Combine("usr", "share", "applications"),
            Path.Combine("usr", "share", "gnome-shell", "theme"),
        };

        foreach (var assetDir in assetDirs)
        {
            var srcPath = Path.Combine(sourceDir, assetDir);
            if (!Directory.Exists(srcPath)) continue;

            var dirName = Path.GetFileName(assetDir);
            var destPath = Path.Combine(targetDir, dirName);

            CopyDirectory(srcPath, destPath);
        }

        var licenseFiles = new[] { "LICENSE", "LICENSE.txt", "COPYING", "COPYING.txt" };
        foreach (var licenseFile in licenseFiles)
        {
            var srcPath = Path.Combine(sourceDir, "usr", "share", "doc", Path.GetFileName(sourceDir), licenseFile);
            if (File.Exists(srcPath))
            {
                File.Copy(srcPath, Path.Combine(targetDir, "LICENSE"), overwrite: true);
                break;
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }

    private static string? GetArgumentValue(string[] args, string key)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }
}

public sealed class ArchPackage
{
    public string PackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public string Epoch { get; set; } = "0";
    public string Description { get; set; } = "";
    public string License { get; set; } = "";
}
