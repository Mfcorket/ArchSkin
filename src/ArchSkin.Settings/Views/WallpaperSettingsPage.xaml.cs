using ArchSkin.Core.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ArchSkin.Settings.Views;

public sealed partial class WallpaperSettingsPage : UserControl
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly IntPtr _windowHandle;

    public WallpaperSettingsPage(SettingsService settingsService, AppSettings settings, IntPtr windowHandle)
    {
        _settingsService = settingsService;
        _settings = settings;
        _windowHandle = windowHandle;
        this.InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        CurrentWallpaper.Text = string.IsNullOrEmpty(_settings.WallpaperPath)
            ? "System default"
            : _settings.WallpaperPath;
    }

    private async void OnClickChooseImage(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");

        InitializeWithWindow.Initialize(picker, _windowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            _settings.WallpaperPath = file.Path;
            _settingsService.Save(_settings);
            CurrentWallpaper.Text = file.Path;
        }
    }

    private void OnClickReset(object sender, RoutedEventArgs e)
    {
        _settings.WallpaperPath = string.Empty;
        _settingsService.Save(_settings);
        CurrentWallpaper.Text = "System default";
    }
}
