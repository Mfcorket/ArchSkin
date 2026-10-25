using ArchSkin.Core.Settings;
using ArchSkin.Core.SystemInfo;
using ArchSkin.Shell.Mvvm;
using Microsoft.UI.Xaml;

namespace ArchSkin.Shell.ViewGroup;

public sealed class PanelViewModel : ObservableObject, IDisposable
{
    private readonly AppSettings _settings;
    private readonly SystemInfoProvider _systemInfo;
    private readonly DispatcherTimer _timer;

    public PanelViewModel(AppSettings settings, SystemInfoProvider systemInfo)
    {
        _settings = settings;
        _systemInfo = systemInfo;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        UpdateClock();
        UpdateTray();
    }

    private string _clockText = "";
    public string ClockText
    {
        get => _clockText;
        private set => SetProperty(ref _clockText, value);
    }

    private string _batteryText = "";
    public string BatteryText
    {
        get => _batteryText;
        private set => SetProperty(ref _batteryText, value);
    }

    private string _batteryIcon = "";
    public string BatteryIcon
    {
        get => _batteryIcon;
        private set => SetProperty(ref _batteryIcon, value);
    }

    private string _networkIcon = "";
    public string NetworkIcon
    {
        get => _networkIcon;
        private set => SetProperty(ref _networkIcon, value);
    }

    private string _volumeIcon = "";
    public string VolumeIcon
    {
        get => _volumeIcon;
        private set => SetProperty(ref _volumeIcon, value);
    }

    private void OnTimerTick(object? sender, object e)
    {
        UpdateClock();
        UpdateTray();
    }

    private void UpdateClock()
    {
        var format = _settings.ClockShowSeconds ? "HH:mm:ss" : _settings.ClockFormat;
        var text = _systemInfo.Now.ToString(format);
        if (_settings.ClockShowDate)
        {
            text = _systemInfo.Now.ToString("ddd MMM dd ") + text;
        }
        ClockText = text;
    }

    private void UpdateTray()
    {
        var batteryPct = _systemInfo.GetBatteryPercent();
        BatteryText = $"{(int)(batteryPct * 100)}%";
        BatteryIcon = _systemInfo.GetBatteryStatusIcon();
        NetworkIcon = _systemInfo.GetNetworkStatusIcon();
        VolumeIcon = _systemInfo.GetVolumeIcon();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
    }
}
