using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ArchSkin.Shell.Views;

public abstract class DesktopWidget
{
    public string Title { get; set; } = "";
    public object Content { get; set; } = new object();
}

public sealed class ClockWidget : DesktopWidget
{
    public ClockWidget()
    {
        Title = "Clock";
        Content = new TextBlock
        {
            Text = DateTime.Now.ToString("HH:mm:ss"),
            FontSize = 32,
            FontWeight = Microsoft.UI.Text.FontWeights.Light,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
        };
    }
}

public sealed class SystemMonitorWidget : DesktopWidget
{
    public SystemMonitorWidget()
    {
        Title = "System Monitor";
        Content = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new TextBlock { Text = "CPU: 15%", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), FontSize = 13 },
                new TextBlock { Text = "RAM: 4.2/16 GB", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), FontSize = 13 },
                new TextBlock { Text = "Disk: 45%", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), FontSize = 13 }
            }
        };
    }
}

public sealed class TodoWidget : DesktopWidget
{
    public TodoWidget()
    {
        Title = "Tasks";
        Content = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new CheckBox { Content = "Review PR #42", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), FontSize = 13 },
                new CheckBox { Content = "Update dependencies", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), FontSize = 13 },
                new CheckBox { Content = "Write documentation", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White), FontSize = 13 }
            }
        };
    }
}