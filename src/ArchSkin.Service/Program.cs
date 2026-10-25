using ArchSkin.Core.ExplorerHook;
using ArchSkin.Core.RegistryStore;
using ArchSkin.Core.Scheduler;

namespace ArchSkin.Service;

public static class Program
{
    private const string ShellExeName = "ArchSkin.Shell.exe";
    private const string SettingsExeName = "ArchSkin.Settings.exe";

    private static readonly string AppDataRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArchSkin");
    private static readonly string LogPath = Path.Combine(AppDataRoot, "logs", "service.log");

    private static StreamWriter? _logWriter;

    public static async Task<int> Main(string[] args)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            _logWriter = new StreamWriter(LogPath, append: true) { AutoFlush = true };

            var command = args.Length > 0 ? args[0].ToLowerInvariant() : "run";

            return command switch
            {
                "install" => Install(),
                "uninstall" => Uninstall(),
                "run" => await RunAsync(),
                "health" => HealthCheck(),
                _ => ShowUsage()
            };
        }
        catch (Exception ex)
        {
            Log($"Fatal error: {ex}");
            return 1;
        }
        finally
        {
            _logWriter?.Dispose();
        }
    }

    private static int Install()
    {
        try
        {
            Log("Installing ArchSkin service...");

            var shellExePath = GetShellExePath();
            if (string.IsNullOrEmpty(shellExePath))
            {
                Log("ERROR: ArchSkin.Shell.exe not found. Install ArchSkin first.");
                return 1;
            }

            ScheduledTaskManager.Install(shellExePath);
            Log("Scheduled task installed successfully.");

            var originalIconsVisible = ExplorerHook.IsDesktopIconsVisible;
            RegistryStore.SetValue("OriginalDesktopIconsVisible", originalIconsVisible);
            ExplorerHook.HideDesktopIcons();
            Log("Desktop icons hidden.");

            Log("Service installation complete.");
            return 0;
        }
        catch (Exception ex)
        {
            Log($"Install failed: {ex}");
            return 1;
        }
    }

    private static int Uninstall()
    {
        try
        {
            Log("Uninstalling ArchSkin service...");

            ScheduledTaskManager.Uninstall();
            Log("Scheduled task removed.");

            var originalIconsVisible = RegistryStore.GetValue<bool>("OriginalDesktopIconsVisible", true);
            ExplorerHook.SetDesktopIconsVisible(originalIconsVisible);
            Log($"Desktop icons restored to: {originalIconsVisible}");

            Log("Service uninstall complete.");
            return 0;
        }
        catch (Exception ex)
        {
            Log($"Uninstall failed: {ex}");
            return 1;
        }
    }

    private static async Task<int> RunAsync()
    {
        Log("ArchSkin service starting...");

        var settingsExePath = GetSettingsExePath();
        var shellExePath = GetShellExePath();

        if (string.IsNullOrEmpty(shellExePath))
        {
            Log("ERROR: ArchSkin.Shell.exe not found.");
            return 1;
        }

        ExplorerHook.HideDesktopIcons();

        var shellProcess = StartShell(shellExePath);

        if (shellProcess == null)
        {
            Log("ERROR: Failed to start ArchSkin.Shell.");
            return 1;
        }

        Log($"ArchSkin.Shell started (PID: {shellProcess.Id})");

        var healthCheckTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await healthCheckTimer.WaitForNextTickAsync())
        {
            try
            {
                if (shellProcess == null || shellProcess.HasExited)
                {
                    Log("ArchSkin.Shell has exited. Restarting...");
                    shellProcess = StartShell(shellExePath);
                    if (shellProcess == null)
                    {
                        Log("ERROR: Failed to restart ArchSkin.Shell.");
                        break;
                    }
                    Log($"ArchSkin.Shell restarted (PID: {shellProcess.Id})");
                }
            }
            catch (Exception ex)
            {
                Log($"Health check error: {ex.Message}");
            }
        }

        return 0;
    }

    private static int HealthCheck()
    {
        try
        {
            var shellProcess = GetShellProcess();
            if (shellProcess == null || shellProcess.HasExited)
            {
                Log("Health check: Shell is NOT running.");
                return 1;
            }

            Log($"Health check: Shell is running (PID: {shellProcess.Id}).");
            return 0;
        }
        catch (Exception ex)
        {
            Log($"Health check error: {ex}");
            return 1;
        }
    }

    private static int ShowUsage()
    {
        Console.WriteLine("ArchSkin Service - Background service for ArchSkin shell.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  archskin-service install     Install service (scheduled task)");
        Console.WriteLine("  archskin-service uninstall   Uninstall service and restore settings");
        Console.WriteLine("  archskin-service run         Run the background service");
        Console.WriteLine("  archskin-service health      Check shell health status");
        return 1;
    }

    private static System.Diagnostics.Process? StartShell(string exePath)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "--autostart",
                UseShellExecute = true,
                CreateNoWindow = false
            };

            return System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            Log($"Failed to start shell: {ex.Message}");
            return null;
        }
    }

    private static System.Diagnostics.Process? GetShellProcess()
    {
        return System.Diagnostics.Process.GetProcessesByName("ArchSkin.Shell").FirstOrDefault();
    }

    private static string? GetShellExePath()
    {
        var serviceDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(serviceDir, ShellExeName);
        if (File.Exists(candidate)) return candidate;

        var parentDir = Path.GetDirectoryName(serviceDir);
        if (parentDir != null)
        {
            candidate = Path.Combine(parentDir, "ArchSkin.Shell", ShellExeName);
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(parentDir, ShellExeName);
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static string? GetSettingsExePath()
    {
        var serviceDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(serviceDir, SettingsExeName);
        if (File.Exists(candidate)) return candidate;

        var parentDir = Path.GetDirectoryName(serviceDir);
        if (parentDir != null)
        {
            candidate = Path.Combine(parentDir, "ArchSkin.Settings", SettingsExeName);
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(parentDir, SettingsExeName);
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine(line);
        try { _logWriter?.WriteLine(line); } catch { }
    }
}
