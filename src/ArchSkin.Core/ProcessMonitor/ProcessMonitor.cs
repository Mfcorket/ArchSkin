using System.Diagnostics;

namespace ArchSkin.Core.ProcessMonitor;

public class ProcessMonitor : IDisposable
{
    private readonly List<RunningApp> _runningApps = new();
    private readonly System.Timers.Timer _timer;
    private readonly object _lock = new();

    public event EventHandler<IReadOnlyList<RunningApp>>? RunningAppsChanged;

    public ProcessMonitor()
    {
        _timer = new System.Timers.Timer(2000);
        _timer.Elapsed += Timer_Elapsed;
        _timer.AutoReset = true;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        RefreshRunningApps();
    }

    public void RefreshRunningApps()
    {
        var current = new List<RunningApp>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (process.MainWindowHandle == IntPtr.Zero) continue;
                if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;

                current.Add(new RunningApp(
                    process.Id,
                    process.ProcessName,
                    process.MainWindowTitle,
                    process.MainWindowHandle));
            }
            catch
            {
            }
        }

        lock (_lock)
        {
            _runningApps.Clear();
            _runningApps.AddRange(current);
        }

        RunningAppsChanged?.Invoke(this, GetRunningApps());
    }

    public IReadOnlyList<RunningApp> GetRunningApps()
    {
        lock (_lock)
        {
            return _runningApps.ToList();
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}

public sealed record RunningApp(
    int ProcessId,
    string ProcessName,
    string WindowTitle,
    IntPtr WindowHandle);
