using ArchSkin.Shell.ViewGroup;
using ArchSkin.Shell.Views;
using Microsoft.UI.Xaml;

namespace ArchSkin.Shell;

public sealed class WindowManager : IDisposable
{
    private const int PanelHeight = 32;
    private const int DockWidth = 64;
    private const int NotificationWidth = 360;
    private const int CalendarWidth = 340;

    private readonly ShellViewModel _vm;
    private readonly ArchSkin.Core.SystemControl.SystemControl _systemControl = new();
    private Window? _desktop;
    private Window? _panel;
    private Window? _dock;
    private Window? _overview;
    private Window? _launcher;
    private Window? _notification;
    private Window? _calendar;

    public WindowManager(ShellViewModel vm)
    {
        _vm = vm;
    }

    public void Start()
    {
        _vm.ApplyStartupSettings();

        CreateDesktop();
        CreatePanel();
        CreateDock();
        CreateOverview();
        CreateLauncher();
        CreateNotification();
        CreateCalendar();
    }

    private void CreateDesktop()
    {
        var view = new DesktopView();
        _desktop = new Window { Content = view };
        _desktop.Title = "ArchSkin Desktop";

        var (mx, my, mw, mh) = WindowHelper.GetPrimaryBounds();
        _desktop.Move(mx, my);
        _desktop.Resize(mw, mh);
        _desktop.MakeNonResizable();
        _desktop.SetTopmost(false);
        _desktop.Activate();

        if (!string.IsNullOrEmpty(_vm.Settings.WallpaperPath))
        {
            view.SetWallpaper(_vm.Settings.WallpaperPath);
        }

        view.ToggleWidgets(true);
    }

    private void CreatePanel()
    {
        var vm = new PanelViewModel(_vm.Settings, _vm.SystemInfo);
        var view = new PanelView(vm);
        view.ActivitiesClicked += OnActivitiesClicked;
        view.AppsClicked += OnAppsClicked;
        view.ClockClicked += OnClockClicked;
        view.SettingsRequested += OnSettingsRequested;
        view.BrightnessChanged += OnBrightnessChanged;
        view.VolumeChanged += OnVolumeChanged;
        view.NetworkToggled += OnNetworkToggled;

        _panel = new Window { Content = view };
        _panel.Title = "ArchSkin Panel";

        var (mx, my, mw, _) = WindowHelper.GetPrimaryBounds();
        _panel.Move(mx, my);
        _panel.Resize(mw, PanelHeight);
        _panel.MakeNonResizable();
        _panel.SetTopmost(true);
        _panel.Activate();

        view.InitializeQuickSettings(
            _systemControl.GetBrightness(),
            _systemControl.GetVolume(),
            _vm.SystemInfo.IsNetworkAvailable());
    }

    private void CreateDock()
    {
        var vm = new DockViewModel(_vm.Settings, _vm.ProcessMonitor, _vm.Launcher);
        var view = new DockView(vm);
        view.AppsClicked += OnAppsClicked;
        vm.Start();

        _dock = new Window { Content = view };
        _dock.Title = "ArchSkin Dock";

        var (mx, my, _, mh) = WindowHelper.GetPrimaryBounds();
        _dock.Move(mx, my + PanelHeight);
        _dock.Resize(DockWidth, mh - PanelHeight);
        _dock.MakeNonResizable();
        _dock.SetTopmost(true);
        _dock.Activate();
    }

    private void CreateOverview()
    {
        var vm = new OverviewViewModel(_vm.Settings, _vm.ProcessMonitor);
        var view = new OverviewView(vm);
        view.CloseRequested += OnCloseOverview;

        _overview = new Window { Content = view };
        _overview.Title = "ArchSkin Overview";

        var (mx, my, mw, mh) = WindowHelper.GetPrimaryBounds();
        _overview.Move(mx, my);
        _overview.Resize(mw, mh);
        _overview.MakeNonResizable();
        _overview.SetTopmost(true);
    }

    private void CreateLauncher()
    {
        var vm = new LauncherViewModel(_vm.Launcher);
        var view = new LauncherView(vm);
        view.CloseRequested += OnCloseLauncher;

        _launcher = new Window { Content = view };
        _launcher.Title = "ArchSkin Launcher";

        var (mx, my, mw, mh) = WindowHelper.GetPrimaryBounds();
        _launcher.Move(mx, my);
        _launcher.Resize(mw, mh);
        _launcher.MakeNonResizable();
        _launcher.SetTopmost(true);
    }

    private void CreateNotification()
    {
        var vm = new NotificationCenterViewModel(_vm.Notifications);
        var view = new NotificationCenterView(vm);
        view.CloseRequested += OnCloseNotification;

        _notification = new Window { Content = view };
        _notification.Title = "ArchSkin Notifications";

        var (mx, my, mw, _) = WindowHelper.GetPrimaryBounds();
        _notification.Move(mx + mw - NotificationWidth - 20, my + PanelHeight + 8);
        _notification.Resize(NotificationWidth, 500);
        _notification.MakeNonResizable();
        _notification.SetTopmost(true);
    }

    private void CreateCalendar()
    {
        var vm = new CalendarViewModel();
        var view = new CalendarPopperView(vm);
        view.CloseRequested += OnCloseCalendar;

        _calendar = new Window { Content = view };
        _calendar.Title = "ArchSkin Calendar";

        var (mx, my, mw, _) = WindowHelper.GetPrimaryBounds();
        _calendar.Move(mx + mw / 2 - CalendarWidth / 2, my + PanelHeight + 8);
        _calendar.Resize(CalendarWidth, 420);
        _calendar.MakeNonResizable();
        _calendar.SetTopmost(true);
    }

    private void OnActivitiesClicked(object? sender, EventArgs e)
    {
        ToggleOverview();
    }

    private void OnAppsClicked(object? sender, EventArgs e)
    {
        ToggleLauncher();
    }

    private void OnClockClicked(object? sender, EventArgs e)
    {
        ToggleNotification();
    }

    private void OnSettingsRequested(object? sender, EventArgs e)
    {
        OpenSettings();
    }

    private void OnBrightnessChanged(object? sender, int percent)
    {
        _systemControl.SetBrightness(percent);
    }

    private void OnVolumeChanged(object? sender, int percent)
    {
        _systemControl.SetVolume(percent);
    }

    private void OnNetworkToggled(object? sender, bool enabled)
    {
        _systemControl.SetNetworkEnabled(enabled);
    }

    public void OpenSettings()
    {
        try
        {
            var exe = Path.Combine(AppContext.BaseDirectory, "ArchSkin.Settings.exe");
            if (File.Exists(exe))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exe,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
        }
    }

    private bool _overviewVisible;
    private bool _launcherVisible;
    private bool _notificationVisible;

    private void OnCloseOverview(object? sender, EventArgs e)
    {
        HideOverview();
    }

    private void OnCloseLauncher(object? sender, EventArgs e)
    {
        HideLauncher();
    }

    private void OnCloseNotification(object? sender, EventArgs e)
    {
        HideNotification();
    }

    private void OnCloseCalendar(object? sender, EventArgs e)
    {
        _calendar?.AppWindow.Hide();
    }

    public void ToggleOverview()
    {
        if (_overviewVisible) HideOverview(); else ShowOverview();
    }

    public void ToggleLauncher()
    {
        if (_launcherVisible) HideLauncher(); else ShowLauncher();
    }

    public void ToggleNotification()
    {
        if (_notificationVisible) HideNotification(); else ShowNotification();
    }

    public void ShowOverview()
    {
        if (_overview == null) return;
        _overview.AppWindow.Show();
        _overviewVisible = true;
    }

    public void HideOverview()
    {
        if (_overview == null) return;
        _overview.AppWindow.Hide();
        _overviewVisible = false;
    }

    public void ShowLauncher()
    {
        if (_launcher == null) return;
        _launcher.AppWindow.Show();
        _launcherVisible = true;
    }

    public void HideLauncher()
    {
        if (_launcher == null) return;
        _launcher.AppWindow.Hide();
        _launcherVisible = false;
    }

    public void ShowNotification()
    {
        if (_notification == null) return;
        _notification.AppWindow.Show();
        _notificationVisible = true;
    }

    public void HideNotification()
    {
        if (_notification == null) return;
        _notification.AppWindow.Hide();
        _notificationVisible = false;
    }

    public void Dispose()
    {
        _desktop?.Close();
        _panel?.Close();
        _dock?.Close();
        _overview?.Close();
        _launcher?.Close();
        _notification?.Close();
        _calendar?.Close();
        _vm.ProcessMonitor.Dispose();
        _systemControl.Dispose();
    }
}
