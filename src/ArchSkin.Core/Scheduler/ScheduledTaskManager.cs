using System.Diagnostics;

namespace ArchSkin.Core.Scheduler;

public static class ScheduledTaskManager
{
    public const string TaskName = "ArchSkin.AutoStart";

    public static bool IsInstalled()
    {
        try
        {
            var result = RunSchtasks("/Query /TN " + TaskName, out var output);
            return result && !output.Contains("ERROR", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static void Install(string shellExePath)
    {
        if (!File.Exists(shellExePath))
        {
            throw new FileNotFoundException("Shell executable not found", shellExePath);
        }

        Uninstall();

        var arguments = $"/Create /TN {TaskName} /SC ONLOGON /DELAY 0005:00 " +
                        $"/TR \"\\\"{shellExePath}\\\" --autostart\" /F";

        var result = RunSchtasks(arguments, out var output);
        if (!result)
        {
            throw new InvalidOperationException($"Failed to install scheduled task: {output}");
        }
    }

    public static void Uninstall()
    {
        try
        {
            RunSchtasks("/Delete /TN " + TaskName + " /F", out _);
        }
        catch
        {
        }
    }

    private static bool RunSchtasks(string arguments, out string output)
    {
        output = string.Empty;
        var psi = new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return false;

        output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0;
    }
}
