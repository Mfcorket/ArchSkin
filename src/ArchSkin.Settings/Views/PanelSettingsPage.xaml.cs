using ArchSkin.Core.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Settings.Views;

public sealed partial class PanelSettingsPage : UserControl
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    public PanelSettingsPage(SettingsService settingsService, AppSettings settings)
    {
        _settingsService = settingsService;
        _settings = settings;
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        ClockShowDate.IsOn = _settings.ClockShowDate;
        ClockShowSeconds.IsOn = _settings.ClockShowSeconds;
        ShowBattery.IsOn = _settings.ShowBattery;
        ShowNetwork.IsOn = _settings.ShowNetwork;
        ShowVolume.IsOn = _settings.ShowVolume;

        foreach (ComboBoxItem item in ClockFormat.Items)
        {
            if (item.Tag?.ToString() == _settings.ClockFormat)
            {
                ClockFormat.SelectedItem = item;
                break;
            }
        }
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        _settings.ClockShowDate = ClockShowDate.IsOn;
        _settings.ClockShowSeconds = ClockShowSeconds.IsOn;
        _settings.ShowBattery = ShowBattery.IsOn;
        _settings.ShowNetwork = ShowNetwork.IsOn;
        _settings.ShowVolume = ShowVolume.IsOn;

        if (ClockFormat.SelectedItem is ComboBoxItem item)
        {
            _settings.ClockFormat = item.Tag?.ToString() ?? "HH:mm";
        }

        _settingsService.Save(_settings);
    }
}
