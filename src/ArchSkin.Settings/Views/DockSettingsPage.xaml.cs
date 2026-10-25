using ArchSkin.Core.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Settings.Views;

public sealed partial class DockSettingsPage : UserControl
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    public DockSettingsPage(SettingsService settingsService, AppSettings settings)
    {
        _settingsService = settingsService;
        _settings = settings;
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        IconSize.Value = _settings.DockIconSize;
        AutoHide.IsOn = _settings.DockAutoHide;
        ShowRunningApps.IsOn = _settings.DockShowRunningApps;

        foreach (ComboBoxItem item in DockPosition.Items)
        {
            if (item.Tag?.ToString() == _settings.DockPosition)
            {
                DockPosition.SelectedItem = item;
                break;
            }
        }
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        _settings.DockIconSize = (int)IconSize.Value;
        _settings.DockAutoHide = AutoHide.IsOn;
        _settings.DockShowRunningApps = ShowRunningApps.IsOn;

        if (DockPosition.SelectedItem is ComboBoxItem item)
        {
            _settings.DockPosition = item.Tag?.ToString() ?? "Left";
        }

        _settingsService.Save(_settings);
    }
}
