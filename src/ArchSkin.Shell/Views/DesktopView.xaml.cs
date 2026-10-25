using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.IO;

namespace ArchSkin.Shell.Views;

public sealed partial class DesktopView : UserControl
{
    public ObservableCollection<DesktopWidget> Widgets { get; } = new();

    public DesktopView()
    {
        this.InitializeComponent();
        LoadDefaultWidgets();
    }

    private void LoadDefaultWidgets()
    {
        Widgets.Add(new ClockWidget());
        Widgets.Add(new SystemMonitorWidget());
        Widgets.Add(new TodoWidget());
        WidgetsPanel.ItemsSource = Widgets;
    }

    public void SetWallpaper(string? imagePath)
    {
        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            var bitmap = new BitmapImage(new Uri(imagePath));
            WallpaperImage.Source = bitmap;
            WallpaperImage.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            FallbackGrid.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            WallpaperImage.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            FallbackGrid.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }

    public void ToggleWidgets(bool visible)
    {
        WidgetsPanel.Visibility = visible ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    }
}