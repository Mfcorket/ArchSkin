using ArchSkin.Core.Settings;
using ArchSkin.Settings.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace ArchSkin.Settings;

public sealed partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private AppSettings Settings => _settingsService.Load();

    public MainWindow()
    {
        _settingsService = new SettingsService();
        this.InitializeComponent();
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        NavigateTo(item.Tag?.ToString() ?? "Theme");
    }

    private void NavigateTo(string tag)
    {
        string title, description;
        UIElement content;
        IntPtr hwnd = WindowNative.GetWindowHandle(this);

        switch (tag)
        {
            case "Panel":
                title = "Panel";
                description = "Configure the top panel behavior";
                content = new PanelSettingsPage(_settingsService, Settings);
                break;
            case "Dock":
                title = "Dock";
                description = "Customize the application dock";
                content = new DockSettingsPage(_settingsService, Settings);
                break;
            case "Overview":
                title = "Overview";
                description = "Configure workspace and animation preferences";
                content = new OverviewSettingsPage(_settingsService, Settings);
                break;
            case "Wallpaper":
                title = "Wallpaper";
                description = "Choose your desktop wallpaper";
                content = new WallpaperSettingsPage(_settingsService, Settings, WindowNative.GetWindowHandle(this));
                break;
            case "Updates":
                title = "Updates";
                description = "Manage update preferences";
                content = new UpdatesSettingsPage(_settingsService, Settings);
                break;
            case "About":
                title = "About";
                description = "ArchSkin information and credits";
                content = new AboutSettingsPage();
                break;
            default:
                title = "Theme";
                description = "Customize the look and feel of ArchSkin";
                content = new ThemeSettingsPage(_settingsService, Settings);
                break;
        }

        PageTitle.Text = title;
        PageDescription.Text = description;
        PageContent.Content = content;
    }
}
