using System.Text;

namespace ArchSkin.Core.Diagnostics;

public sealed record CrashReport(
    string Id,
    DateTimeOffset Timestamp,
    string Source,
    string ExceptionType,
    string Message,
    string? StackTrace,
    string AppVersion,
    string OsVersion);

public sealed class CrashReporter
{
    private static readonly string CrashDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ArchSkin", "crashes");

    private readonly bool _enabled;

    public CrashReporter(bool enabled)
    {
        _enabled = enabled;
    }

    public static string GetCrashDirectory() => CrashDir;

    public CrashReport? Capture(Exception exception, string source)
    {
        if (!_enabled || exception == null) return null;

        var report = new CrashReport(
            Id: Guid.NewGuid().ToString("N")[..12],
            Timestamp: DateTimeOffset.UtcNow,
            Source: source,
            ExceptionType: exception.GetType().FullName ?? exception.GetType().Name,
            Message: exception.Message,
            StackTrace: exception.StackTrace,
            AppVersion: Updates.UpdateService.CurrentVersion,
            OsVersion: Environment.OSVersion.VersionString);

        TryWrite(report);
        return report;
    }

    private static void TryWrite(CrashReport report)
    {
        try
        {
            Directory.CreateDirectory(CrashDir);
            var fileName = $"{report.Timestamp:yyyyMMdd-HHmmss}-{report.Id}.log";
            var path = Path.Combine(CrashDir, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("ArchSkin Crash Report");
            sb.AppendLine("=====================");
            sb.AppendLine($"Id:         {report.Id}");
            sb.AppendLine($"Timestamp:  {report.Timestamp:O}");
            sb.AppendLine($"Source:     {report.Source}");
            sb.AppendLine($"Version:    {report.AppVersion}");
            sb.AppendLine($"OS:         {report.OsVersion}");
            sb.AppendLine();
            sb.AppendLine($"Exception:  {report.ExceptionType}");
            sb.AppendLine($"Message:    {report.Message}");
            sb.AppendLine();
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(report.StackTrace ?? "(none)");

            File.WriteAllText(path, sb.ToString());
        }
        catch
        {
        }
    }

    public IReadOnlyList<string> GetPendingReports()
    {
        try
        {
            if (!Directory.Exists(CrashDir)) return Array.Empty<string>();
            return Directory.GetFiles(CrashDir, "*.log");
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public void ClearReports()
    {
        try
        {
            if (!Directory.Exists(CrashDir)) return;
            foreach (var file in Directory.GetFiles(CrashDir, "*.log"))
            {
                File.Delete(file);
            }
        }
        catch
        {
        }
    }
}
