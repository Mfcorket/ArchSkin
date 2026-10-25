using ArchSkin.Core.Diagnostics;
using ArchSkin.Core.Localization;
using ArchSkin.Core.Settings;
using ArchSkin.Core.Updates;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Settings.Views;

public sealed partial class UpdatesSettingsPage : UserControl
{
    private static readonly Uri ReleaseFeed =
        new("https://raw.githubusercontent.com/archskin/archskin/main/release.json");

    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly UpdateService _updateService = new();

    public UpdatesSettingsPage(SettingsService settingsService, AppSettings settings)
    {
        _settingsService = settingsService;
        _settings = settings;
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        AutoUpdate.IsOn = _settings.AutoUpdate;
        UpdateFrequency.Value = _settings.UpdateCheckFrequencyHours;
        CrashReporting.IsOn = _settings.CrashReporting;
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        _settings.AutoUpdate = AutoUpdate.IsOn;
        _settings.UpdateCheckFrequencyHours = (int)UpdateFrequency.Value;
        _settings.CrashReporting = CrashReporting.IsOn;
        _settingsService.Save(_settings);
    }

    private async void OnClickCheckUpdates(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = LocalizationService.Get("settings.updates"),
            Content = LocalizationService.Get("updates.checking"),
            CloseButtonText = LocalizationService.Get("common.ok"),
            XamlRoot = this.XamlRoot
        };

        var showTask = dialog.ShowAsync();

        var result = await _updateService.CheckAsync(ReleaseFeed);

        dialog.Content = result.Error != null
            ? string.Format(LocalizationService.Get("updates.failed"), result.Error)
            : result.UpdateAvailable && result.Latest != null
                ? string.Format(LocalizationService.Get("updates.available"), result.Latest.Version)
                : LocalizationService.Get("updates.upToDate");

        _ = showTask;
    }

    private async void OnClickViewCrashReports(object sender, RoutedEventArgs e)
    {
        var reporter = new CrashReporter(enabled: true);
        var reports = reporter.GetPendingReports();

        var content = reports.Count == 0
            ? "No crash reports found."
            : string.Join("\n", reports.Select(Path.GetFileName));

        var dialog = new ContentDialog
        {
            Title = LocalizationService.Get("crash.viewReports"),
            Content = new ScrollViewer
            {
                Content = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap },
                MaxHeight = 300
            },
            CloseButtonText = LocalizationService.Get("common.ok"),
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }
}
