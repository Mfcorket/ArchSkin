namespace ArchSkin.Core.TrayBridge;

public class TrayBridge
{
    public event EventHandler<TrayIconEventArgs>? TrayIconClicked;
    public event EventHandler<TrayIconEventArgs>? TrayIconDoubleClicked;
    public event EventHandler? TrayIconRightClicked;

    public void HandleTrayClick(string iconId)
    {
        TrayIconClicked?.Invoke(this, new TrayIconEventArgs(iconId, DateTime.Now));
    }

    public void HandleTrayDoubleClick(string iconId)
    {
        TrayIconDoubleClicked?.Invoke(this, new TrayIconEventArgs(iconId, DateTime.Now));
    }

    public void HandleTrayRightClick()
    {
        TrayIconRightClicked?.Invoke(this, EventArgs.Empty);
    }
}

public sealed record TrayIconEventArgs(string IconId, DateTime Timestamp);
