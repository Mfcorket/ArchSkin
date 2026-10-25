namespace ArchSkin.Core.NotificationStore;

public sealed class NotificationStore
{
    private readonly object _lock = new();
    private readonly List<NotificationItem> _items = new();
    private int _maxItems;

    public NotificationStore(int maxItems = 50)
    {
        _maxItems = maxItems;
    }

    public event EventHandler<NotificationItem>? NotificationAdded;
    public event EventHandler<string>? NotificationRemoved;

    public NotificationItem Add(string appName, string title, string body, string? actionUrl = null, NotificationAction? action = null)
    {
        var item = new NotificationItem(
            Guid.NewGuid().ToString(),
            appName,
            title,
            body,
            DateTime.Now,
            actionUrl,
            action);

        lock (_lock)
        {
            _items.Insert(0, item);
            while (_items.Count > _maxItems)
            {
                _items.RemoveAt(_items.Count - 1);
            }
        }

        NotificationAdded?.Invoke(this, item);
        return item;
    }

    public bool Remove(string id)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(i => i.Id == id);
            if (index < 0) return false;
            _items.RemoveAt(index);
        }

        NotificationRemoved?.Invoke(this, id);
        return true;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }

    public IReadOnlyList<NotificationItem> GetAll()
    {
        lock (_lock)
        {
            return _items.ToList();
        }
    }
}

public sealed record NotificationAction(string Label, string ActionId);

public sealed record NotificationItem(
    string Id,
    string AppName,
    string Title,
    string Body,
    DateTime Timestamp,
    string? ActionUrl,
    NotificationAction? Action)
{
    public bool HasAction => Action != null;
}