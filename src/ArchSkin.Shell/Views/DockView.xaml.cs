using ArchSkin.Core.AppLauncher;
using ArchSkin.Core.ProcessMonitor;
using ArchSkin.Shell.ViewGroup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArchSkin.Shell.Views;

public sealed partial class DockView : UserControl
{
    public DockViewModel ViewModel { get; }

    public event EventHandler? AppsClicked;

    public DockView(DockViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
        DataContext = viewModel;

        PinnedList.ItemsSource = ViewModel.PinnedApps;
        RunningList.ItemsSource = ViewModel.RunningApps;
    }

    private void OnAppsClick(object sender, RoutedEventArgs e)
    {
        AppsClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnPinnedClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is AppEntry app)
        {
            ViewModel.LaunchApp(app);
        }
    }

    private void OnAppButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is AppEntry app)
        {
            ViewModel.LaunchApp(app);
        }
    }

    private void OnRunningClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RunningApp app)
        {
            ViewModel.LaunchRunningApp(app);
        }
    }

    private void OnRunningButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is RunningApp app)
        {
            ViewModel.LaunchRunningApp(app);
        }
    }
}
