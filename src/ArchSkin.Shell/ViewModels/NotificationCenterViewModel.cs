using System.Collections.ObjectModel;
using ArchSkin.Core.NotificationStore;
using ArchSkin.Shell.Mvvm;

namespace ArchSkin.Shell.ViewGroup;

public sealed class NotificationCenterViewModel : ObservableObject
{
    private readonly NotificationStore _store;

    public ObservableCollection<NotificationItem> Notifications { get; } = new();

    public NotificationCenterViewModel(NotificationStore store)
    {
        _store = store;
        _store.NotificationAdded += OnNotificationAdded;
        _store.NotificationRemoved += OnNotificationRemoved;

        foreach (var item in store.GetAll())
        {
            Notifications.Add(item);
        }
    }

    private void OnNotificationAdded(object? sender, NotificationItem item)
    {
        Notifications.Insert(0, item);
    }

    private void OnNotificationRemoved(object? sender, string id)
    {
        var index = Notifications.ToList().FindIndex(n => n.Id == id);
        if (index >= 0)
        {
            Notifications.RemoveAt(index);
        }
    }

    public void Remove(string id)
    {
        _store.Remove(id);
    }

    public void Clear()
    {
        _store.Clear();
        Notifications.Clear();
    }
}
