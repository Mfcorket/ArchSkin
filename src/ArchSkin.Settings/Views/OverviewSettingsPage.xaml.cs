using ArchSkin.Core.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Settings.Views;

public sealed partial class OverviewSettingsPage : UserControl
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    public OverviewSettingsPage(SettingsService settingsService, AppSettings settings)
    {
        _settingsService = settingsService;
        _settings = settings;
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        WorkspaceCount.Value = _settings.WorkspaceCount;
        Animations.IsOn = _settings.OverviewAnimations;
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        _settings.WorkspaceCount = (int)WorkspaceCount.Value;
        _settings.OverviewAnimations = Animations.IsOn;
        _settingsService.Save(_settings);
    }
}
