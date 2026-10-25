namespace ArchSkin.Core.AppLauncher;

public enum AppEntrySource
{
    DesktopFile,
    StartMenu,
    Registry
}

public sealed record AppEntry(
    string id,
    string name,
    string? comment,
    string? iconName,
    string? iconPath,
    string? exec,
    string? executablePath,
    string? workingDirectory,
    AppEntrySource source,
    string category)
{
    public string Id => id;
    public string Name => name;
    public string? Comment => comment;
    public string? IconName => iconName;
    public string? IconPath => iconPath;
    public string? Exec => exec;
    public string? ExecutablePath => executablePath;
    public string? WorkingDirectory => workingDirectory;
    public AppEntrySource Source => source;
    public string Category => category;
    public string SearchText => $"{Name} {Comment} {Category}".ToLowerInvariant();
}
