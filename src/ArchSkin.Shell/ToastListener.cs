using ArchSkin.Core.NotificationStore;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ArchSkin.Shell;

public sealed class ToastListener
{
    private readonly NotificationStore _store;

    public ToastListener(NotificationStore store)
    {
        _store = store;
    }

    public void Start()
    {
        try
        {
            RefreshHistory();
        }
        catch
        {
        }
    }

    private void RefreshHistory()
    {
        var history = ToastNotificationManager.History.GetHistory();
        foreach (var toast in history)
        {
            var content = toast.Content;
            var textNodes = content.GetElementsByTagName("text");
            if (textNodes.Length >= 2)
            {
                var title = textNodes[0].InnerText ?? string.Empty;
                var body = textNodes[1].InnerText ?? string.Empty;
                var appName = toast.Tag.Length > 0 ? toast.Tag : "System";
                _store.Add(appName, title, body);
            }
        }
    }
}
