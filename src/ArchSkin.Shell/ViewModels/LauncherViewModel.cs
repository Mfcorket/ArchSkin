using System.Collections.ObjectModel;
using ArchSkin.Core.AppLauncher;
using ArchSkin.Shell.Mvvm;

namespace ArchSkin.Shell.ViewGroup;

public sealed class LauncherViewModel : ObservableObject
{
    private readonly AppLauncherService _launcher;

    public ObservableCollection<AppEntry> Apps { get; } = new();

    private string _searchQuery = "";

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyFilter();
            }
        }
    }

    public LauncherViewModel(AppLauncherService launcher)
    {
        _launcher = launcher;
        ApplyFilter();
    }

    public void ApplyFilter()
    {
        Apps.Clear();
        foreach (var app in _launcher.Search(SearchQuery))
        {
            Apps.Add(app);
        }
    }

    public bool Launch(AppEntry app)
    {
        return _launcher.Launch(app);
    }
}
