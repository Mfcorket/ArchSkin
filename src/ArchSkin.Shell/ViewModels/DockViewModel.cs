using System.Collections.ObjectModel;
using ArchSkin.Core.AppLauncher;
using ArchSkin.Core.ProcessMonitor;
using ArchSkin.Core.Settings;
using ArchSkin.Shell.Mvvm;

namespace ArchSkin.Shell.ViewGroup;

public sealed class DockViewModel : ObservableObject, IDisposable
{
    private readonly AppSettings _settings;
    private readonly ProcessMonitor _processMonitor;
    private readonly AppLauncherService _launcher;

    public ObservableCollection<AppEntry> PinnedApps { get; } = new();

    public ObservableCollection<RunningApp> RunningApps { get; } = new();

    private int _iconSize;
    public int IconSize
    {
        get => _iconSize;
        set => SetProperty(ref _iconSize, value);
    }

    public DockViewModel(AppSettings settings, ProcessMonitor processMonitor, AppLauncherService launcher)
    {
        _settings = settings;
        _processMonitor = processMonitor;
        _launcher = launcher;

        IconSize = settings.DockIconSize;
        _processMonitor.RunningAppsChanged += OnRunningAppsChanged;

        foreach (var app in launcher.GetAllApps().Take(8))
        {
            PinnedApps.Add(app);
        }
    }

    public void Start() => _processMonitor.Start();

    private void OnRunningAppsChanged(object? sender, IReadOnlyList<RunningApp> apps)
    {
        RunningApps.Clear();
        foreach (var app in apps)
        {
            RunningApps.Add(app);
        }
    }

    public void LaunchApp(AppEntry app)
    {
        _launcher.Launch(app);
    }

    public void LaunchRunningApp(RunningApp app)
    {
        try
        {
            var process = System.Diagnostics.Process.GetProcessById(app.ProcessId);
            var handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                ArchSkin.Core.Windowing.WindowFunctions.SetForegroundWindow(handle);
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _processMonitor.RunningAppsChanged -= OnRunningAppsChanged;
    }
}
