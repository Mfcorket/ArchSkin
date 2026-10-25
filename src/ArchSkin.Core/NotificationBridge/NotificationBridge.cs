namespace ArchSkin.Core.NotificationBridge;

public class NotificationBridge
{
    public event EventHandler<NotificationEventArgs>? NotificationReceived;
    public event EventHandler<NotificationEventArgs>? NotificationDismissed;

    public void Subscribe()
    {
    }

    public void Unsubscribe()
    {
    }

    public Task ShowNotificationAsync(string appName, string title, string body, string? actionUrl = null)
    {
        var args = new NotificationEventArgs(
            Guid.NewGuid().ToString(), appName, title, body, DateTime.Now, actionUrl);

        NotificationReceived?.Invoke(this, args);
        return Task.CompletedTask;
    }

    /// <summary>Raises <see cref="NotificationDismissed"/> for the given notification.</summary>
    public void Dismiss(string id, string appName, string title, string body)
    {
        NotificationDismissed?.Invoke(this, new NotificationEventArgs(
            id, appName, title, body, DateTime.Now, null));
    }
}

public sealed record NotificationEventArgs(
    string Id,
    string AppName,
    string Title,
    string Body,
    DateTime Timestamp,
    string? ActionUrl);
