using System.Collections.ObjectModel;
using ArchSkin.Core.ProcessMonitor;
using ArchSkin.Core.Settings;
using ArchSkin.Shell.Mvvm;

namespace ArchSkin.Shell.ViewGroup;

public sealed class OverviewViewModel : ObservableObject, IDisposable
{
    private readonly AppSettings _settings;
    private readonly ProcessMonitor _processMonitor;

    public ObservableCollection<WorkspaceInfo> Workspaces { get; } = new();

    public ObservableCollection<RunningApp> Windows { get; } = new();

    private int _activeWorkspace;
    public int ActiveWorkspace
    {
        get => _activeWorkspace;
        set
        {
            if (SetProperty(ref _activeWorkspace, value))
            {
                WorkspaceChanged?.Invoke(this, _activeWorkspace);
            }
        }
    }

    private bool _animations = true;
    public bool Animations
    {
        get => _animations;
        set => SetProperty(ref _animations, value);
    }

    public event EventHandler<int>? WorkspaceChanged;

    public OverviewViewModel(AppSettings settings, ProcessMonitor processMonitor)
    {
        _settings = settings;
        _processMonitor = processMonitor;
        Animations = settings.OverviewAnimations;

        for (var i = 0; i < settings.WorkspaceCount; i++)
        {
            Workspaces.Add(new WorkspaceInfo(i + 1, $"Workspace {i + 1}"));
        }

        _processMonitor.RunningAppsChanged += OnRunningAppsChanged;
    }

    public void Refresh()
    {
        _processMonitor.RefreshRunningApps();
    }

    private void OnRunningAppsChanged(object? sender, IReadOnlyList<RunningApp> apps)
    {
        Windows.Clear();
        foreach (var app in apps)
        {
            Windows.Add(app);
        }
    }

    public void FocusWindow(RunningApp app)
    {
        try
        {
            var process = System.Diagnostics.Process.GetProcessById(app.ProcessId);
            ArchSkin.Core.Windowing.WindowFunctions.BringToFront(process.MainWindowHandle);
        }
        catch
        {
        }
    }

    public void SwitchWorkspace(int workspaceIndex)
    {
        if (workspaceIndex >= 0 && workspaceIndex < Workspaces.Count)
        {
            ActiveWorkspace = workspaceIndex;
        }
    }

    public void Dispose()
    {
        _processMonitor.RunningAppsChanged -= OnRunningAppsChanged;
    }
}

public sealed record WorkspaceInfo(int Index, string Name);