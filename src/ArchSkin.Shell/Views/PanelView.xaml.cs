using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace ArchSkin.Shell.Views;

public sealed partial class PanelView : UserControl
{
    public PanelViewModel ViewModel { get; }

    public event EventHandler? ActivitiesClicked;
    public event EventHandler? AppsClicked;
    public event EventHandler? ClockClicked;
    public event EventHandler? QuickSettingsOpened;
    public event EventHandler? SettingsRequested;
    public event EventHandler<int>? BrightnessChanged;
    public event EventHandler<int>? VolumeChanged;
    public event EventHandler<bool>? NetworkToggled;

    private bool _isPopupOpen;
    private bool _suppressSystemEvents;

    public PanelView(PanelViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
        DataContext = viewModel;

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateBindings();
    }

    /// <summary>
    /// Populates the Quick Settings controls with current system values
    /// without raising change events.
    /// </summary>
    public void InitializeQuickSettings(int brightness, int volume, bool network)
    {
        _suppressSystemEvents = true;
        try
        {
            BrightnessSlider.Value = Math.Clamp(brightness, 0, 100);
            VolumeSlider.Value = Math.Clamp(volume, 0, 100);
            NetworkToggle.IsOn = network;
        }
        finally
        {
            _suppressSystemEvents = false;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _ = DispatcherQueue.TryEnqueue(UpdateBindings);
    }

    private void UpdateBindings()
    {
        ClockText.Text = ViewModel.ClockText;
        BatteryText.Text = ViewModel.BatteryText;
        BatteryIcon.Text = ViewModel.BatteryIcon;
        NetworkIcon.Text = ViewModel.NetworkIcon;
        VolumeIcon.Text = ViewModel.VolumeIcon;
        BatteryPercent.Text = ViewModel.BatteryText;
    }

    private void OnActivitiesClick(object sender, RoutedEventArgs e)
    {
        ActivitiesClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnAppsClick(object sender, RoutedEventArgs e)
    {
        AppsClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnClockTapped(object sender, TappedRoutedEventArgs e)
    {
        ClockClicked?.Invoke(this, EventArgs.Empty);
        ToggleQuickSettings();
    }

    private void ToggleQuickSettings()
    {
        _isPopupOpen = !_isPopupOpen;
        QuickSettingsPopup.IsOpen = _isPopupOpen;
        if (_isPopupOpen)
        {
            QuickSettingsOpened?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnBrightnessChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_suppressSystemEvents) return;
        BrightnessChanged?.Invoke(this, (int)e.NewValue);
    }

    private void OnVolumeChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_suppressSystemEvents) return;
        VolumeChanged?.Invoke(this, (int)e.NewValue);
    }

    private void OnNetworkToggled(object sender, RoutedEventArgs e)
    {
        if (_suppressSystemEvents) return;
        NetworkToggled?.Invoke(this, NetworkToggle.IsOn);
    }

    private void OnNetworkSettingsClick(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ms-settings:network",
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        QuickSettingsPopup.IsOpen = false;
        _isPopupOpen = false;
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }
}