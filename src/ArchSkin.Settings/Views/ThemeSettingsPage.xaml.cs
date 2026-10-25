using ArchSkin.Core.LicenseTracker;
using ArchSkin.Core.Settings;
using ArchSkin.Core.ThemeRegistry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Text;
using System.IO;
using Windows.UI;

namespace ArchSkin.Settings.Views;

public sealed partial class ThemeSettingsPage : UserControl
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly LicenseTracker _licenseTracker;
    private ThemeManifest? _selectedTheme;

    public ThemeSettingsPage(SettingsService settingsService, AppSettings settings)
    {
        _settingsService = settingsService;
        _settings = settings;
        _licenseTracker = new LicenseTracker(new ThemeRegistryService());
        this.InitializeComponent();
        LoadThemes();
    }

    private void LoadThemes()
    {
        var registry = new ThemeRegistryService();
        var themes = registry.GetInstalledThemes();
        
        ThemeList.Items.Clear();
        foreach (var theme in themes)
        {
            var item = new ListViewItem();
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 59, 66, 82)),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 4)
            };
            var stack = new StackPanel { Spacing = 4 };
            stack.Children.Add(new TextBlock { Text = theme.ThemeId, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromArgb(255, 236, 239, 244)) });
            stack.Children.Add(new TextBlock { Text = $"{theme.SourcePackage} {theme.SourceVersion}", FontSize = 11, Foreground = new SolidColorBrush(Color.FromArgb(255, 136, 192, 208)) });
            stack.Children.Add(new TextBlock { Text = theme.License, FontSize = 10, Foreground = new SolidColorBrush(Color.FromArgb(255, 154, 165, 177)) });
            border.Child = stack;
            item.Content = border;
            item.Tag = theme;
            ThemeList.Items.Add(item);
        }

        var currentTheme = themes.FirstOrDefault(t => t.ThemeId == _settings.ThemeId);
        if (currentTheme != null)
        {
            foreach (ListViewItem item in ThemeList.Items)
            {
                if (item.Tag is ThemeManifest t && t.ThemeId == currentTheme.ThemeId)
                {
                    ThemeList.SelectedItem = item;
                    break;
                }
            }
            UpdatePreview(currentTheme);
            ApplyThemeButton.IsEnabled = true;
        }
    }

    private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListViewItem item && item.Tag is ThemeManifest theme)
        {
            _selectedTheme = theme;
            UpdatePreview(theme);
            ApplyThemeButton.IsEnabled = true;
        }
    }

    private void UpdatePreview(ThemeManifest theme)
    {
        PreviewThemeName.Text = theme.ThemeId;
        PreviewDescription.Text = $"{theme.SourcePackage} {theme.SourceVersion} • {theme.License}";

        var registry = new ThemeRegistryService();
        var iconPath = registry.GetThemeIconPath(theme.ThemeId, "arch");
        
        if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
        {
            PreviewImage.Source = new BitmapImage(new Uri(iconPath));
            PreviewImage.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PreviewFallback.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            PreviewImage.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PreviewFallback.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PreviewThemeName.Text = theme.ThemeId;
            PreviewDescription.Text = $"{theme.SourcePackage} {theme.SourceVersion} • {theme.License}";
        }
    }

    private void OnApplyThemeClick(object sender, RoutedEventArgs e)
    {
        if (_selectedTheme != null)
        {
            _settings.ThemeId = _selectedTheme.ThemeId;
            _settingsService.Save(_settings);
            ApplyThemeToShell(_selectedTheme.ThemeId);
        }
    }

    private void ApplyThemeToShell(string themeId)
    {
        // The Shell reads ThemeId from settings.json on startup and at runtime.
        // Settings are already persisted by the caller; nothing else required here.
    }

    private void OnInstallThemeClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Install Theme",
            Content = "Run 'archskin install <package-name>' from the command line to install new themes.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        _ = dialog.ShowAsync();
    }

    private void OnVerifyClick(object sender, RoutedEventArgs e)
    {
        var allValid = _licenseTracker.VerifyLicenseHeaders(_settings.ThemeId);
        var dialog = new ContentDialog
        {
            Title = "License Verification",
            Content = allValid ? "License headers verified successfully." : "License verification failed.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        _ = dialog.ShowAsync();
    }
}